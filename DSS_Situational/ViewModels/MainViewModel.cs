using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using DSS_Situational.Models;
using DSS_Situational.Services;

namespace DSS_Situational.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly KnowledgeBaseService _kbService;
        private readonly DecisionEngine _engine;

        // ---- Текущая ситуация ----
        private string _situationName = string.Empty;
        private string _situationDescription = string.Empty;
        private string _selectedCategory = string.Empty;

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

        // ---- Категории и фильтр ----
        public ObservableCollection<string> Categories { get; } = new();

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
        private int _maxResults = 5;

        public double SimilarityThreshold
        {
            get => _threshold;
            set
            {
                Set(ref _threshold, value);
                _engine.SimilarityThreshold = value;
            }
        }

        public int MaxResults
        {
            get => _maxResults;
            set
            {
                Set(ref _maxResults, value);
                _engine.MaxResults = value;
            }
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
        public RelayCommand AnalyzeCommand { get; }
        public RelayCommand AddParameterCommand { get; }
        public RelayCommand RemoveParameterCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand LearnCommand { get; }
        public RelayCommand OpenKnowledgeBaseCommand { get; }

        public MainViewModel()
        {
            string kbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "knowledge_base.json");

            _kbService = new KnowledgeBaseService(kbPath);
            try { _kbService.Load(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки базы знаний:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            _engine = new DecisionEngine(_kbService)
            {
                SimilarityThreshold = _threshold,
                MaxResults = _maxResults
            };

            RefreshCategories();
            AddDefaultParameters();

            AnalyzeCommand = new RelayCommand(OnAnalyze);
            AddParameterCommand = new RelayCommand(OnAddParameter);
            RemoveParameterCommand = new RelayCommand(OnRemoveParameter);
            ClearCommand = new RelayCommand(OnClear);
            LearnCommand = new RelayCommand(OnLearn, () => SelectedResult != null || Results.Count > 0);
            OpenKnowledgeBaseCommand = new RelayCommand(OnOpenKnowledgeBase);
        }

        private void AddDefaultParameters()
        {
            CurrentParameters.Add(new ParameterRowViewModel
            { Name = "Температура", Value = 0, Weight = 1.0, Unit = "°C" });
            CurrentParameters.Add(new ParameterRowViewModel
            { Name = "Давление", Value = 0, Weight = 1.0, Unit = "бар" });
            CurrentParameters.Add(new ParameterRowViewModel
            { Name = "Вибрация", Value = 0, Weight = 1.0, Unit = "мм/с" });
            CurrentParameters.Add(new ParameterRowViewModel
            { Name = "Нагрузка", Value = 0, Weight = 0.8, Unit = "%" });
        }

        private void OnAnalyze(object? _)
        {
            if (CurrentParameters.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы один параметр.";
                return;
            }

            var situation = BuildCurrentSituation();

            try
            {
                var resultList = _engine.Analyze(situation);
                _kbService.Save();

                Results.Clear();
                foreach (var r in resultList)
                    Results.Add(r);

                SelectedResult = Results.FirstOrDefault();

                StatusMessage = resultList.Count > 0
                    ? $"Найдено {resultList.Count} подходящих прецедентов. " +
                      $"Наилучшее совпадение: {Results[0].SituationName} " +
                      $"({Results[0].SimilarityPercent})"
                    : "Подходящих прецедентов не найдено. Попробуйте снизить порог сходства.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка анализа: {ex.Message}";
            }
        }

        private void OnAddParameter(object? _) =>
            CurrentParameters.Add(new ParameterRowViewModel
            { Name = "Новый параметр", Value = 0, Weight = 1.0 });

        private void OnRemoveParameter(object? param)
        {
            if (param is ParameterRowViewModel row)
                CurrentParameters.Remove(row);
            else if (CurrentParameters.Count > 0)
                CurrentParameters.RemoveAt(CurrentParameters.Count - 1);
        }

        private void OnClear(object? _)
        {
            Results.Clear();
            SelectedResult = null;
            foreach (var p in CurrentParameters) p.Value = 0;
            SituationName = string.Empty;
            SituationDescription = string.Empty;
            StatusMessage = "Параметры сброшены.";
        }

        private void OnLearn(object? _)
        {
            if (CurrentParameters.All(p => p.Value == 0))
            {
                StatusMessage = "Заполните параметры ситуации перед сохранением.";
                return;
            }

            var dialog = new Views.LearnDialog();
            if (dialog.ShowDialog() == true)
            {
                var situation = BuildCurrentSituation();
                situation.Name = string.IsNullOrWhiteSpace(SituationName)
                    ? dialog.EnteredTitle
                    : SituationName;

                var decision = new Decision(
                    dialog.EnteredDecisionTitle,
                    dialog.EnteredDecisionDescription,
                    dialog.EnteredPriority);

                try
                {
                    _engine.LearnFromSituation(situation, decision, dialog.EnteredCategory);
                    RefreshCategories();
                    StatusMessage = $"Новый прецедент «{situation.Name}» добавлен в базу знаний.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка сохранения: {ex.Message}";
                }
            }
        }

        private void OnOpenKnowledgeBase(object? _)
        {
            var win = new Views.KnowledgeBaseWindow(_kbService, _engine);
            win.ShowDialog();
            RefreshCategories();
        }

        private Situation BuildCurrentSituation()
        {
            return new Situation
            {
                Name = SituationName,
                Description = SituationDescription,
                Parameters = CurrentParameters
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => p.ToModel())
                    .ToList()
            };
        }

        private void RefreshCategories()
        {
            string current = SelectedCategory;
            Categories.Clear();
            Categories.Add("");   // «Все категории»
            foreach (var c in _kbService.GetCategories())
                Categories.Add(c);
            SelectedCategory = Categories.Contains(current) ? current : "";
        }
    }
}
