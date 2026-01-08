using System.Windows;

namespace CRUDSamsonovCAD.Views;

public partial class DeviceEditWindow : Window
{
    public DeviceEditWindow()
    {
        InitializeComponent();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.DeviceEditViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Device.Name))
            {
                MessageBox.Show("Введите наименование прибора.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
