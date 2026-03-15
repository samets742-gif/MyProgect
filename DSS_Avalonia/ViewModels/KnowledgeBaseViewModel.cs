using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSS_Avalonia.Models;
using DSS_Avalonia.Services;

namespace DSS_Avalonia.ViewModels
{
    public class KnowledgeBaseViewModel : BaseViewModel
    {
        private readonly KnowledgeBaseService _kbService;
        private readonly IDialogService _dialog;

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
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<SituationParameter> SelectedParameters =>
            SelectedEntry is not null
                ? new ObservableCollection<SituationParameter>(SelectedEntry.ReferenceSituation.Parameters)
                : new ObservableCollection<SituationParameter>();

        public ObservableCollection<string> SelectedActions =>
            SelectedEntry is not null
                ? new ObservableCollection<string>(SelectedEntry.Decision.Actions)
                : new ObservableCollection<string>();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); FilterEntries(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public AsyncRelayCommand DeleteCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public KnowledgeBaseViewModel(KnowledgeBaseService kbService, IDialogService dialog)
        {
            _kbService = kbService;
            _dialog = dialog;

            DeleteCommand = new AsyncRelayCommand(OnDeleteAsync, _ => SelectedEntry != null);
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

            foreach (var e in filtered) Entries.Add(e);
            StatusMessage = $"Записей: {Entries.Count} / {all.Count}";
        }

        private async Task OnDeleteAsync(object? _)
        {
            if (SelectedEntry == null) return;

            bool ok = await _dialog.ConfirmAsync(
                $"Удалить прецедент «{SelectedEntry.ReferenceSituation.Name}»?");
            if (!ok) return;

            try
            {
                _kbService.Remove(SelectedEntry.Id);
                _kbService.Save();
                FilterEntries();
                StatusMessage = "Запись удалена.";
            }
            catch (Exception ex)
            {
                await _dialog.AlertAsync($"Ошибка удаления: {ex.Message}", "Ошибка");
            }
        }
    }
}
