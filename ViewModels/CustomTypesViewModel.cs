using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.Services;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class CustomTypesViewModel : BaseViewModel
{
    private readonly CustomPartRepository _repo;
    private readonly WindowService _windowService;

    public ObservableCollection<CustomPartTypeDto> Types { get; } = new();
    public ObservableCollection<CustomPartParamDto> Params { get; } = new();

    private CustomPartTypeDto? _selectedType;
    public CustomPartTypeDto? SelectedType
    {
        get => _selectedType;
        set { _selectedType = value; OnPropertyChanged(); LoadParams(); }
    }

    private CustomPartParamDto? _selectedParam;
    public CustomPartParamDto? SelectedParam
    {
        get => _selectedParam;
        set { _selectedParam = value; OnPropertyChanged(); }
    }

    public ICommand AddTypeCommand { get; }
    public ICommand EditTypeCommand { get; }
    public ICommand DeleteTypeCommand { get; }
    public ICommand AddParamCommand { get; }
    public ICommand EditParamCommand { get; }
    public ICommand DeleteParamCommand { get; }

    public CustomTypesViewModel(CustomPartRepository repo, WindowService windowService)
    {
        _repo = repo;
        _windowService = windowService;

        AddTypeCommand = new RelayCommand(_ => AddType());
        EditTypeCommand = new RelayCommand(_ => EditType(), _ => SelectedType != null);
        DeleteTypeCommand = new RelayCommand(_ => DeleteType(), _ => SelectedType != null);
        AddParamCommand = new RelayCommand(_ => AddParam(), _ => SelectedType != null);
        EditParamCommand = new RelayCommand(_ => EditParam(), _ => SelectedParam != null);
        DeleteParamCommand = new RelayCommand(_ => DeleteParam(), _ => SelectedParam != null);

        LoadTypes();
    }

    private void LoadTypes()
    {
        Types.Clear();
        foreach (var t in _repo.GetTypes())
            Types.Add(t);
    }

    private void LoadParams()
    {
        Params.Clear();
        if (SelectedType == null) return;
        foreach (var p in _repo.GetParams(SelectedType.Id))
            Params.Add(p);
    }

    private void AddType()
    {
        var dto = new CustomPartTypeDto();
        if (!_windowService.ShowCustomTypeEditor(dto)) return;
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            MessageBox.Show("Введите наименование типа.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        dto.Id = _repo.InsertType(dto);
        LoadTypes();
        SelectedType = Types.FirstOrDefault(t => t.Id == dto.Id);
    }

    private void EditType()
    {
        if (SelectedType == null) return;
        var dto = new CustomPartTypeDto { Id = SelectedType.Id, Name = SelectedType.Name };
        if (!_windowService.ShowCustomTypeEditor(dto)) return;
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            MessageBox.Show("Введите наименование типа.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        _repo.UpdateType(dto);
        LoadTypes();
        SelectedType = Types.FirstOrDefault(t => t.Id == dto.Id);
    }

    private void DeleteType()
    {
        if (SelectedType == null) return;
        if (MessageBox.Show("Удалить тип и все его параметры?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _repo.DeleteType(SelectedType.Id);
        LoadTypes();
        Params.Clear();
    }

    private void AddParam()
    {
        if (SelectedType == null) return;
        var dto = new CustomPartParamDto { TypeId = SelectedType.Id, ParamKind = "Text", SortOrder = Params.Count + 1 };
        if (!_windowService.ShowCustomParamEditor(dto)) return;
        if (string.IsNullOrWhiteSpace(dto.ParamName))
        {
            MessageBox.Show("Введите имя параметра.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        dto.Id = _repo.InsertParam(dto);
        LoadParams();
        SelectedParam = Params.FirstOrDefault(p => p.Id == dto.Id);
    }

    private void EditParam()
    {
        if (SelectedParam == null) return;
        var dto = new CustomPartParamDto
        {
            Id = SelectedParam.Id,
            TypeId = SelectedParam.TypeId,
            ParamName = SelectedParam.ParamName,
            ParamKind = SelectedParam.ParamKind,
            ListValues = SelectedParam.ListValues,
            SortOrder = SelectedParam.SortOrder
        };
        if (!_windowService.ShowCustomParamEditor(dto)) return;
        if (string.IsNullOrWhiteSpace(dto.ParamName))
        {
            MessageBox.Show("Введите имя параметра.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        _repo.UpdateParam(dto);
        LoadParams();
        SelectedParam = Params.FirstOrDefault(p => p.Id == dto.Id);
    }

    private void DeleteParam()
    {
        if (SelectedParam == null) return;
        if (MessageBox.Show("Удалить параметр?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _repo.DeleteParam(SelectedParam.Id);
        LoadParams();
    }
}
