using Avalonia.Controls;
using Avalonia.Interactivity;
using DSS_Avalonia.Models;

namespace DSS_Avalonia.Views
{
    public partial class LearnDialog : Window
    {
        public LearnDialog()
        {
            InitializeComponent();
        }

        private void OnSave(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DecisionTitleBox.Text))
            {
                // простое визуальное предупреждение через border
                DecisionTitleBox.Classes.Add("error");
                return;
            }

            Close(new LearnDialogResult
            {
                SituationTitle       = TitleBox.Text?.Trim() ?? string.Empty,
                Category             = CategoryBox.Text?.Trim() ?? "Общее",
                DecisionTitle        = DecisionTitleBox.Text.Trim(),
                DecisionDescription  = DecisionDescBox.Text?.Trim() ?? string.Empty,
                Priority             = PriorityBox.SelectedIndex + 1
            });
        }

        private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
    }
}
