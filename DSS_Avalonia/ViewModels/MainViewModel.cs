using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSS_Avalonia.Models;
using DSS_Avalonia.Services;

namespace DSS_Avalonia.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly KnowledgeBaseService _kbService;
        private readonly DecisionEngine _engine;
        private readonly IDialogService _dialog;

        // ---- Текущая ситуация ----
        private string _situationName = string.Empty;
        private string _situationDescription = string.Empty;

        public string SituationName
        {
            get => _situationName;
            set => Set(ref _situationName, value);
        }

        public string SituationDescription
        {
            get => _situationDescription;
            set => Set(ref _situationDescription, value);
        }

        public ObservableCollection<ParameterRowViewModel> CurrentParameters { get; } = new();

        // ---- Категории ----
        public ObservableCollection<string> Categories { get; } = new();

        private string _selectedCategory = string.Empty;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                Set(ref _selectedCategory, value);
                _engine.CategoryFilter = string.IsNullOrEmpty(value) ? null : value;
            }
        }

        // ---- Настройки движка ----
        private double _threshold = 0.3;
        public double SimilarityThreshold
        {
            get => _threshold;
            set { Set(ref _threshold, value); _engine.SimilarityThreshold = value; }
        }

        private int _maxResults = 5;
        public int MaxResults
        {
            get => _maxResults;
            set { Set(ref _maxResults, value); _engine.MaxResults = value; }
        }

        // ---- Результаты ----
        public ObservableCollection<RecognitionResult> Results { get; } = new();

        private RecognitionResult? _selectedResult;
        public RecognitionResult? SelectedResult
        {
            get => _selectedResult;
            set => Set(ref _selectedResult, value);
        }

        // ---- Статус ----
        private string _statusMessage = "Готово. Заполните параметры ситуации и нажмите «Анализ».";
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        // ---- Команды ----
        public AsyncRelayCommand AnalyzeCommand { get; }
        public RelayCommand AddParameterCommand { get; }
        public RelayCommand RemoveParameterCommand { get; }
        public RelayCommand ClearCommand { get; }
        public AsyncRelayCommand LearnCommand { get; }
        public AsyncRelayCommand OpenKnowledgeBaseCommand { get; }

        public MainViewModel(IDialogService dialog)
        {
            _dialog = dialog;

            string kbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "knowledge_base.json");

            _kbService = new KnowledgeBaseService(kbPath);
            try { _kbService.Load(); }
            catch (Exception ex)
            {
                _ = _dialog.AlertAsync($"Ошибка загрузки базы знаний:\n{ex.Message}", "Ошибка");
            }

            _engine = new DecisionEngine(_kbService)
            {
                SimilarityThreshold = _threshold,
                MaxResults = _maxResults
            };

            RefreshCategories();
            AddDefaultParameters();

            AnalyzeCommand = new AsyncRelayCommand(OnAnalyzeAsync);
            AddParameterCommand = new RelayCommand(_ => CurrentParameters.Add(
                new ParameterRowViewModel { Name = "Новый параметр", Value = 0, Weight = 1.0 }));
            RemoveParameterCommand = new RelayCommand(param =>
            {
                if (param is ParameterRowViewModel row)
                    CurrentParameters.Remove(row);
                else if (CurrentParameters.Count > 0)
                    CurrentParameters.RemoveAt(CurrentParameters.Count - 1);
            });
            ClearCommand = new RelayCommand(_ => OnClear());
            LearnCommand = new AsyncRelayCommand(OnLearnAsync);
            OpenKnowledgeBaseCommand = new AsyncRelayCommand(OnOpenKnowledgeBaseAsync);
        }

        private void AddDefaultParameters()
        {
            CurrentParameters.Add(new ParameterRowViewModel { Name = "Температура", Value = 0, Weight = 1.0, Unit = "°C" });
            CurrentParameters.Add(new ParameterRowViewModel { Name = "Давление",    Value = 0, Weight = 1.0, Unit = "бар" });
            CurrentParameters.Add(new ParameterRowViewModel { Name = "Вибрация",    Value = 0, Weight = 1.0, Unit = "мм/с" });
            CurrentParameters.Add(new ParameterRowViewModel { Name = "Нагрузка",    Value = 0, Weight = 0.8, Unit = "%" });
        }

        private async Task OnAnalyzeAsync(object? _)
        {
            if (CurrentParameters.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы один параметр.";
                return;
            }

            try
            {
                var situation = BuildCurrentSituation();
                var resultList = _engine.Analyze(situation);
                _kbService.Save();

                Results.Clear();
                foreach (var r in resultList) Results.Add(r);
                SelectedResult = Results.FirstOrDefault();

                StatusMessage = resultList.Count > 0
                    ? $"Найдено {resultList.Count} прецедентов. Лучшее: «{Results[0].SituationName}» ({Results[0].SimilarityPercent})"
                    : "Подходящих прецедентов не найдено. Снизьте порог сходства.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка анализа: {ex.Message}";
            }

            await Task.CompletedTask;
        }

        private void OnClear()
        {
            Results.Clear();
            SelectedResult = null;
            foreach (var p in CurrentParameters) p.Value = 0;
            SituationName = string.Empty;
            SituationDescription = string.Empty;
            StatusMessage = "Параметры сброшены.";
        }

        private async Task OnLearnAsync(object? _)
        {
            if (CurrentParameters.All(p => p.Value == 0))
            {
                StatusMessage = "Заполните параметры ситуации перед сохранением.";
                return;
            }

            var result = await _dialog.ShowLearnDialogAsync();
            if (result == null) return;

            var situation = BuildCurrentSituation();
            situation.Name = string.IsNullOrWhiteSpace(SituationName)
                ? result.SituationTitle
                : SituationName;

            var decision = new Decision(
                result.DecisionTitle,
                result.DecisionDescription,
                result.Priority);

            try
            {
                _engine.LearnFromSituation(situation, decision, result.Category);
                RefreshCategories();
                StatusMessage = $"Прецедент «{situation.Name}» добавлен в базу знаний.";
            }
            catch (Exception ex)
            {
                await _dialog.AlertAsync($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private async Task OnOpenKnowledgeBaseAsync(object? _)
        {
            await _dialog.OpenKnowledgeBaseAsync(_kbService, _engine);
            RefreshCategories();
        }

        private Situation BuildCurrentSituation() => new()
        {
            Name = SituationName,
            Description = SituationDescription,
            Parameters = CurrentParameters
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Select(p => p.ToModel())
                .ToList()
        };

        private void RefreshCategories()
        {
            string current = SelectedCategory;
            Categories.Clear();
            Categories.Add("");
            foreach (var c in _kbService.GetCategories()) Categories.Add(c);
            SelectedCategory = Categories.Contains(current) ? current : "";
        }
    }
}
