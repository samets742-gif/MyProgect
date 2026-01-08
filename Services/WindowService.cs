using System.Windows;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.ViewModels;
using CRUDSamsonovCAD.Views;

namespace CRUDSamsonovCAD.Services;

public sealed class WindowService
{
    private readonly SpecRepository _specRepo;
    private readonly IFileDialogService _fileDialog;
    private readonly CustomPartRepository _customRepo;

    public WindowService(SpecRepository specRepo, CustomPartRepository customRepo, IFileDialogService fileDialog)
    {
        _specRepo = specRepo;
        _customRepo = customRepo;
        _fileDialog = fileDialog;
    }

    public bool ShowDeviceEditor(DeviceDto dto)
    {
        var vm = new DeviceEditViewModel(dto, _fileDialog);
        var window = new DeviceEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = vm
        };
        return window.ShowDialog() == true;
    }

    public bool ShowPartEditor(PartDto dto, object spec, out PartEditResult result)
    {
        var vm = new PartEditViewModel(dto, spec, _specRepo, _customRepo, _fileDialog);
        var window = new PartEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = vm
        };
        var ok = window.ShowDialog() == true;
        result = new PartEditResult
        {
            IsCustom = vm.IsCustomType,
            Spec = vm.SelectedSpec,
            CustomValues = vm.GetCustomValues()
        };
        return ok;
    }

    public bool ShowCustomTypesWindow()
    {
        var vm = new CustomTypesViewModel(_customRepo, this);
        var window = new CustomTypesWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = vm
        };
        return window.ShowDialog() == true;
    }

    public bool ShowCustomTypeEditor(CustomPartTypeDto dto)
    {
        var vm = new CustomTypeEditViewModel(dto);
        var window = new CustomTypeEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = vm
        };
        return window.ShowDialog() == true;
    }

    public bool ShowCustomParamEditor(CustomPartParamDto dto)
    {
        var vm = new CustomParamEditViewModel(dto);
        var window = new CustomParamEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = vm
        };
        return window.ShowDialog() == true;
    }
}
