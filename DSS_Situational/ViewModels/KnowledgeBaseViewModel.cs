using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DSS_Situational.Models;
using DSS_Situational.Services;

namespace DSS_Situational.ViewModels
{
    public class KnowledgeBaseViewModel : BaseViewModel
    {
        private readonly KnowledgeBaseService _kbService;
        private readonly DecisionEngine _engine;

        public ObservableCollection<KnowledgeBaseEntry> Entries { get; } = new();

        private KnowledgeBaseEntry? _selectedEntry;
        public KnowledgeBaseEntry? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                Set(ref _selectedEntry, value);
                OnPropertyChanged(nameof(SelectedParameters));
                OnPropertyChanged(nameof(SelectedActions));
            }
        }

        public ObservableCollection<SituationParameter> SelectedParameters =>
            SelectedEntry != null
                ? new ObservableCollection<SituationParameter>(
                    SelectedEntry.ReferenceSituation.Parameters)
                : new ObservableCollection<SituationParameter>();

        public ObservableCollection<string> SelectedActions =>
            SelectedEntry != null
                ? new ObservableCollection<string>(SelectedEntry.Decision.Actions)
                : new ObservableCollection<string>();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                Set(ref _searchText, value);
                FilterEntries();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public RelayCommand DeleteCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public KnowledgeBaseViewModel(KnowledgeBaseService kbService, DecisionEngine engine)
        {
            _kbService = kbService;
            _engine = engine;

            DeleteCommand = new RelayCommand(OnDelete, () => SelectedEntry != null);
            RefreshCommand = new RelayCommand(_ => FilterEntries());

            FilterEntries();
        }

        private void FilterEntries()
        {
            Entries.Clear();
            var all = _kbService.Entries;

            var filtered = string.IsNullOrWhiteSpace(_searchText)
                ? all
                : all.Where(e =>
                    e.ReferenceSituation.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Category.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Decision.Title.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var entry in filtered)
                Entries.Add(entry);

            StatusMessage = $"Записей в базе знаний: {Entries.Count} / {all.Count}";
        }

        private void OnDelete(object? _)
        {
            if (SelectedEntry == null) return;

            var confirm = MessageBox.Show(
                $"Удалить прецедент «{SelectedEntry.ReferenceSituation.Name}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _kbService.Remove(SelectedEntry.Id);
                _kbService.Save();
                FilterEntries();
                StatusMessage = "Запись удалена.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
