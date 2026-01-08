using System.Configuration;
using System.Windows;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Services;
using CRUDSamsonovCAD.ViewModels;

namespace CRUDSamsonovCAD;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, ex) =>
        {
            MessageBox.Show(ex.Exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
        {
            if (ex.ExceptionObject is Exception err)
                MessageBox.Show(err.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        var cs = ConfigurationManager.ConnectionStrings["CadDb"]?.ConnectionString
                 ?? "Server=localhost\\SQLEXPRESS;Database=CADDevicesDb;Trusted_Connection=True;Encrypt=False;";

        var deviceRepo = new DeviceRepository(cs);
        var partRepo = new PartRepository(cs);
        var specRepo = new SpecRepository(cs);
        var customRepo = new CustomPartRepository(cs);

        var exportService = new ExportService(specRepo, customRepo);
        var importService = new ImportService(specRepo, customRepo);
        var fileDialog = new FileDialogService();
        var windowService = new WindowService(specRepo, customRepo, fileDialog);

        var vm = new MainViewModel(deviceRepo, partRepo, specRepo, customRepo, exportService, importService, windowService, fileDialog);
        var window = new MainWindow { DataContext = vm };
        window.Show();
    }
}
