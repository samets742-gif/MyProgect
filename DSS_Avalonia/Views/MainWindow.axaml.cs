using System.Threading.Tasks;
using Avalonia.Controls;
using DSS_Avalonia.Models;
using DSS_Avalonia.Services;
using DSS_Avalonia.ViewModels;

namespace DSS_Avalonia.Views
{
    public partial class MainWindow : Window, IDialogService
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
        }

        // ─── IDialogService ────────────────────────────────────────────────

        public async Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
        {
            var dlg = new ConfirmDialog(title, message);
            var result = await dlg.ShowDialog<bool?>(this);
            return result == true;
        }

        public async Task AlertAsync(string message, string title = "Внимание")
        {
            var dlg = new ConfirmDialog(title, message);
            await dlg.ShowDialog(this);
        }

        public async Task<LearnDialogResult?> ShowLearnDialogAsync()
        {
            var dlg = new LearnDialog();
            return await dlg.ShowDialog<LearnDialogResult?>(this);
        }

        public async Task OpenKnowledgeBaseAsync(KnowledgeBaseService kb, DecisionEngine engine)
        {
            var win = new KnowledgeBaseWindow(kb, engine);
            await win.ShowDialog(this);
        }
    }
}
