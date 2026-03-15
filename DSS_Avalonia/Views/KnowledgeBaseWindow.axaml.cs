using Avalonia.Controls;
using DSS_Avalonia.Services;
using DSS_Avalonia.ViewModels;

namespace DSS_Avalonia.Views
{
    public partial class KnowledgeBaseWindow : Window, IDialogService
    {
        public KnowledgeBaseWindow(KnowledgeBaseService kb, DecisionEngine engine)
        {
            InitializeComponent();
            DataContext = new KnowledgeBaseViewModel(kb, this);
        }

        public async System.Threading.Tasks.Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
        {
            var dlg = new ConfirmDialog(title, message);
            var result = await dlg.ShowDialog<bool?>(this);
            return result == true;
        }

        public async System.Threading.Tasks.Task AlertAsync(string message, string title = "Внимание")
        {
            var dlg = new ConfirmDialog(title, message);
            await dlg.ShowDialog(this);
        }

        public System.Threading.Tasks.Task<Models.LearnDialogResult?> ShowLearnDialogAsync() =>
            System.Threading.Tasks.Task.FromResult<Models.LearnDialogResult?>(null);

        public System.Threading.Tasks.Task OpenKnowledgeBaseAsync(KnowledgeBaseService kb, DecisionEngine engine) =>
            System.Threading.Tasks.Task.CompletedTask;
    }
}
