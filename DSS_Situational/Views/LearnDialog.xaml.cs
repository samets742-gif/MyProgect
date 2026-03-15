using System.Windows;

namespace DSS_Situational.Views
{
    public partial class LearnDialog : Window
    {
        public string EnteredTitle => TitleBox.Text.Trim();
        public string EnteredCategory => CategoryBox.Text.Trim();
        public string EnteredDecisionTitle => DecisionTitleBox.Text.Trim();
        public string EnteredDecisionDescription => DecisionDescBox.Text.Trim();
        public int EnteredPriority => PriorityBox.SelectedIndex + 1;

        public LearnDialog()
        {
            InitializeComponent();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DecisionTitleBox.Text))
            {
                MessageBox.Show("Введите название решения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e) =>
            DialogResult = false;
    }
}
