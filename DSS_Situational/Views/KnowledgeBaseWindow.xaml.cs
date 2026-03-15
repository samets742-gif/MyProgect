using System.Windows;
using DSS_Situational.Services;
using DSS_Situational.ViewModels;

namespace DSS_Situational.Views
{
    public partial class KnowledgeBaseWindow : Window
    {
        public KnowledgeBaseWindow(KnowledgeBaseService kbService, DecisionEngine engine)
        {
            InitializeComponent();
            DataContext = new KnowledgeBaseViewModel(kbService, engine);
        }
    }
}
