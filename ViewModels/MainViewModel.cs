using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.Services;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly DeviceRepository _deviceRepo;
    private readonly PartRepository _partRepo;
    private readonly SpecRepository _specRepo;
    private readonly CustomPartRepository _customRepo;
    private readonly ExportService _exportService;
    private readonly ImportService _importService;
    private readonly WindowService _windowService;
    private readonly IFileDialogService _fileDialog;

    public ObservableCollection<DeviceDto> Devices { get; } = new();
    public ObservableCollection<PartDto> Parts { get; } = new();
    public ObservableCollection<PartDto> SelectedParts { get; } = new();

    public ICollectionView DevicesView { get; }
    public ICollectionView PartsView { get; }

    private string? _deviceSearchText;
    public string? DeviceSearchText
    {
        get => _deviceSearchText;
        set
        {
            _deviceSearchText = value;
            OnPropertyChanged();
            DevicesView.Refresh();
        }
    }

    private string? _partSearchText;
    public string? PartSearchText
    {
        get => _partSearchText;
        set
        {
            _partSearchText = value;
            OnPropertyChanged();
            UpdateDevicePartSearchCache();
            PartsView.Refresh();
            DevicesView.Refresh();
        }
    }

    private HashSet<int>? _deviceIdsByPartSearch;

    private DeviceDto? _selectedDevice;
    public DeviceDto? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
            LoadParts();
            SelectedDeviceImage = ImageService.ToImageSource(_selectedDevice?.DrawingImage);
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private PartDto? _selectedPart;
    public PartDto? SelectedPart
    {
        get => _selectedPart;
        set
        {
            _selectedPart = value;
            OnPropertyChanged();
            LoadSelectedSpec();
            SelectedPartImage = ImageService.ToImageSource(_selectedPart?.DrawingImage);
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    private object? _selectedSpec;
    public object? SelectedSpec
    {
        get => _selectedSpec;
        set { _selectedSpec = value; OnPropertyChanged(); }
    }

    public ObservableCollection<CustomParamFieldViewModel> SelectedCustomFields { get; } = new();

    private ImageSource? _selectedPartImage;
    public ImageSource? SelectedPartImage
    {
        get => _selectedPartImage;
        set { _selectedPartImage = value; OnPropertyChanged(); }
    }

    private ImageSource? _selectedDeviceImage;
    public ImageSource? SelectedDeviceImage
    {
        get => _selectedDeviceImage;
        set { _selectedDeviceImage = value; OnPropertyChanged(); }
    }

    public ICommand AddDeviceCommand { get; }
    public ICommand EditDeviceCommand { get; }
    public ICommand DeleteDeviceCommand { get; }
    public ICommand OpenDeviceNxCommand { get; }
    public ICommand PrintDeviceDrawingCommand { get; }
    public ICommand PrintPartDrawingCommand { get; }
    public ICommand ManageCustomTypesCommand { get; }
    public ICommand AddPartCommand { get; }
    public ICommand EditPartCommand { get; }
    public ICommand DeletePartCommand { get; }
    public ICommand OpenNxCommand { get; }
    public ICommand ExportDeviceWordCommand { get; }
    public ICommand ExportDeviceExcelCommand { get; }
    public ICommand ExportDeviceXmlCommand { get; }
    public ICommand ExportSelectedWordCommand { get; }
    public ICommand ExportSelectedExcelCommand { get; }
    public ICommand ExportSelectedXmlCommand { get; }
    public ICommand ImportDeviceWordCommand { get; }
    public ICommand ImportDeviceExcelCommand { get; }
    public ICommand ImportDeviceXmlCommand { get; }
    public ICommand ImportPartsWordCommand { get; }
    public ICommand ImportPartsExcelCommand { get; }
    public ICommand ImportPartsXmlCommand { get; }

    public MainViewModel(
        DeviceRepository deviceRepo,
        PartRepository partRepo,
        SpecRepository specRepo,
        CustomPartRepository customRepo,
        ExportService exportService,
        ImportService importService,
        WindowService windowService,
        IFileDialogService fileDialog)
    {
        _deviceRepo = deviceRepo;
        _partRepo = partRepo;
        _specRepo = specRepo;
        _customRepo = customRepo;
        _exportService = exportService;
        _importService = importService;
        _windowService = windowService;
        _fileDialog = fileDialog;

        DevicesView = CollectionViewSource.GetDefaultView(Devices);
        DevicesView.Filter = FilterDevice;
        PartsView = CollectionViewSource.GetDefaultView(Parts);
        PartsView.Filter = FilterPart;

        AddDeviceCommand = new RelayCommand(_ => AddDevice());
        EditDeviceCommand = new RelayCommand(_ => EditDevice(), _ => SelectedDevice != null);
        DeleteDeviceCommand = new RelayCommand(_ => DeleteDevice(), _ => SelectedDevice != null);
        OpenDeviceNxCommand = new RelayCommand(_ => NxService.OpenNxModel(SelectedDevice?.NxModelPath), _ => !string.IsNullOrWhiteSpace(SelectedDevice?.NxModelPath));
        PrintDeviceDrawingCommand = new RelayCommand(_ => PrintDeviceDrawing(), _ => SelectedDevice?.DrawingImage != null);
        PrintPartDrawingCommand = new RelayCommand(_ => PrintPartDrawing(), _ => SelectedPart?.DrawingImage != null);
        ManageCustomTypesCommand = new RelayCommand(_ => ManageCustomTypes());

        AddPartCommand = new RelayCommand(_ => AddPart(), _ => SelectedDevice != null);
        EditPartCommand = new RelayCommand(_ => EditPart(), _ => SelectedPart != null);
        DeletePartCommand = new RelayCommand(_ => DeletePart(), _ => SelectedPart != null);

        OpenNxCommand = new RelayCommand(_ => NxService.OpenNxModel(SelectedPart?.NxModelPath), _ => !string.IsNullOrWhiteSpace(SelectedPart?.NxModelPath));
        ExportDeviceWordCommand = new RelayCommand(_ => ExportDeviceWord(), _ => SelectedDevice != null);
        ExportDeviceExcelCommand = new RelayCommand(_ => ExportDeviceExcel(), _ => SelectedDevice != null);
        ExportDeviceXmlCommand = new RelayCommand(_ => ExportDeviceXml(), _ => SelectedDevice != null);
        ExportSelectedWordCommand = new RelayCommand(_ => ExportSelectedWord(), _ => SelectedDevice != null && SelectedParts.Count > 0);
        ExportSelectedExcelCommand = new RelayCommand(_ => ExportSelectedExcel(), _ => SelectedDevice != null && SelectedParts.Count > 0);
        ExportSelectedXmlCommand = new RelayCommand(_ => ExportSelectedXml(), _ => SelectedDevice != null && SelectedParts.Count > 0);
        ImportDeviceWordCommand = new RelayCommand(_ => ImportDeviceWord());
        ImportDeviceExcelCommand = new RelayCommand(_ => ImportDeviceExcel());
        ImportDeviceXmlCommand = new RelayCommand(_ => ImportDeviceXml());
        ImportPartsWordCommand = new RelayCommand(_ => ImportPartsWord(), _ => SelectedDevice != null);
        ImportPartsExcelCommand = new RelayCommand(_ => ImportPartsExcel(), _ => SelectedDevice != null);
        ImportPartsXmlCommand = new RelayCommand(_ => ImportPartsXml(), _ => SelectedDevice != null);

        SelectedParts.CollectionChanged += OnSelectedPartsChanged;

        LoadDevices();
    }

    private void LoadDevices()
    {
        Devices.Clear();
        foreach (var d in _deviceRepo.GetAll())
            Devices.Add(d);
        UpdateDevicePartSearchCache();
        DevicesView.Refresh();
    }

    private void LoadParts()
    {
        Parts.Clear();
        SelectedParts.Clear();
        SelectedPart = null;
        SelectedSpec = null;
        SelectedPartImage = null;
        SelectedDeviceImage = ImageService.ToImageSource(SelectedDevice?.DrawingImage);
        if (SelectedDevice == null) return;

        foreach (var p in _partRepo.GetByDevice(SelectedDevice.Id))
        {
            if (PartTypeProvider.IsCustom(p.PartType, out var customId))
            {
                var custom = _customRepo.GetTypes().FirstOrDefault(t => t.Id == customId);
                p.PartTypeDisplayName = custom?.Name ?? p.PartType;
            }
            else
            {
                p.PartTypeDisplayName = PartTypeProvider.GetDisplayName(p.PartType);
            }
            Parts.Add(p);
        }
        PartsView.Refresh();
    }

    private void LoadSelectedSpec()
    {
        SelectedSpec = null;
        SelectedCustomFields.Clear();
        if (SelectedPart == null) return;
        if (PartTypeProvider.IsCustom(SelectedPart.PartType, out _))
        {
            foreach (var f in _customRepo.GetFields(SelectedPart.Id))
                SelectedCustomFields.Add(new CustomParamFieldViewModel { Name = f.Name, Value = f.Value });
        }
        else
        {
            SelectedSpec = _specRepo.GetSpec(SelectedPart.PartType, SelectedPart.Id);
        }
    }

    private void AddDevice()
    {
        var dto = new DeviceDto();
        if (!_windowService.ShowDeviceEditor(dto)) return;
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            MessageBox.Show("Введите наименование прибора.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            dto.Id = _deviceRepo.Insert(dto);
            LoadDevices();
            SelectedDevice = Devices.FirstOrDefault(d => d.Id == dto.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditDevice()
    {
        if (SelectedDevice == null) return;
        var dto = new DeviceDto
        {
            Id = SelectedDevice.Id,
            Name = SelectedDevice.Name,
            Code = SelectedDevice.Code,
            Description = SelectedDevice.Description,
            NxModelPath = SelectedDevice.NxModelPath,
            DrawingImage = SelectedDevice.DrawingImage
        };
        if (!_windowService.ShowDeviceEditor(dto)) return;
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            MessageBox.Show("Введите наименование прибора.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            _deviceRepo.Update(dto);
            LoadDevices();
            SelectedDevice = Devices.FirstOrDefault(d => d.Id == dto.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteDevice()
    {
        if (SelectedDevice == null) return;
        if (MessageBox.Show("Удалить выбранный прибор?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _deviceRepo.Delete(SelectedDevice.Id);
        LoadDevices();
    }

    private void AddPart()
    {
        if (SelectedDevice == null) return;
        var partType = PartTypeProvider.Items.First().Key;
        var dto = new PartDto { DeviceId = SelectedDevice.Id, PartType = partType };
        var spec = _specRepo.CreateEmptySpec(partType, 0);

        if (!_windowService.ShowPartEditor(dto, spec, out var result)) return;
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            MessageBox.Show("Введите наименование детали.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            dto.Id = _partRepo.Insert(dto);
            if (result.IsCustom)
            {
                _customRepo.UpsertValues(dto.Id, result.CustomValues.Select(v => new CustomPartValueDto
                {
                    PartId = dto.Id,
                    ParamId = v.ParamId,
                    ValueText = v.ValueText
                }));
            }
            else
            {
                SpecFormatter.SetPartId(result.Spec!, dto.Id);
                _specRepo.UpsertSpec(dto.PartType, result.Spec!);
            }

            LoadParts();
            SelectedPart = Parts.FirstOrDefault(p => p.Id == dto.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditPart()
    {
        if (SelectedPart == null) return;

        var previousType = SelectedPart.PartType;
        var dto = new PartDto
        {
            Id = SelectedPart.Id,
            DeviceId = SelectedPart.DeviceId,
            Name = SelectedPart.Name,
            PartType = SelectedPart.PartType,
            NxModelPath = SelectedPart.NxModelPath,
            DrawingImage = SelectedPart.DrawingImage,
            Notes = SelectedPart.Notes
        };

        var spec = _specRepo.GetSpec(dto.PartType, dto.Id) ?? _specRepo.CreateEmptySpec(dto.PartType, dto.Id);

        if (!_windowService.ShowPartEditor(dto, spec, out var result)) return;
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            MessageBox.Show("Введите наименование детали.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _partRepo.Update(dto);
            if (previousType != dto.PartType)
            {
                if (PartTypeProvider.IsCustom(previousType, out _))
                    _customRepo.DeleteValues(dto.Id);
                else
                    _specRepo.DeleteSpec(previousType, dto.Id);
            }

            if (result.IsCustom)
            {
                _customRepo.UpsertValues(dto.Id, result.CustomValues.Select(v => new CustomPartValueDto
                {
                    PartId = dto.Id,
                    ParamId = v.ParamId,
                    ValueText = v.ValueText
                }));
            }
            else
            {
                _specRepo.UpsertSpec(dto.PartType, result.Spec!);
            }

            LoadParts();
            SelectedPart = Parts.FirstOrDefault(p => p.Id == dto.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeletePart()
    {
        if (SelectedPart == null) return;
        if (MessageBox.Show("Удалить выбранную деталь?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        if (PartTypeProvider.IsCustom(SelectedPart.PartType, out _))
            _customRepo.DeleteValues(SelectedPart.Id);
        else
            _specRepo.DeleteSpec(SelectedPart.PartType, SelectedPart.Id);
        _partRepo.Delete(SelectedPart.Id);
        LoadParts();
    }

    private bool FilterDevice(object obj)
    {
        if (obj is not DeviceDto d) return false;
        if (!string.IsNullOrWhiteSpace(DeviceSearchText))
        {
            var text = DeviceSearchText.Trim();
            var matchesDevice = d.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(d.Code) && d.Code.Contains(text, StringComparison.OrdinalIgnoreCase));
            if (!matchesDevice) return false;
        }

        if (!string.IsNullOrWhiteSpace(PartSearchText))
        {
            if (_deviceIdsByPartSearch == null || !_deviceIdsByPartSearch.Contains(d.Id))
                return false;
        }

        return true;
    }

    private bool FilterPart(object obj)
    {
        if (obj is not PartDto p) return false;
        if (string.IsNullOrWhiteSpace(PartSearchText)) return true;
        var text = PartSearchText.Trim();
        return p.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
               || p.PartType.Contains(text, StringComparison.OrdinalIgnoreCase)
               || p.PartTypeDisplayName.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateDevicePartSearchCache()
    {
        if (string.IsNullOrWhiteSpace(PartSearchText))
        {
            _deviceIdsByPartSearch = null;
            return;
        }

        var text = PartSearchText.Trim();
        var matchingTypes = PartTypeProvider.Items
            .Where(p => p.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Key)
            .ToList();
        foreach (var t in _customRepo.GetTypes().Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase)))
            matchingTypes.Add(PartTypeProvider.GetCustomKey(t.Id));

        var ids = _partRepo.GetDeviceIdsByPartSearch(text, matchingTypes);
        _deviceIdsByPartSearch = new HashSet<int>(ids);
    }

    private void ManageCustomTypes()
    {
        _windowService.ShowCustomTypesWindow();
        LoadParts();
        UpdateDevicePartSearchCache();
        DevicesView.Refresh();
    }

    private void PrintDeviceDrawing()
    {
        if (SelectedDevice?.DrawingImage == null || SelectedDevice.DrawingImage.Length == 0)
        {
            MessageBox.Show("Чертеж сборки отсутствует.", "Печать", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        PrintImage(SelectedDevice.DrawingImage, "Чертеж сборки");
    }

    private void PrintPartDrawing()
    {
        if (SelectedPart?.DrawingImage == null || SelectedPart.DrawingImage.Length == 0)
        {
            MessageBox.Show("Чертеж детали отсутствует.", "Печать", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        PrintImage(SelectedPart.DrawingImage, "Чертеж детали");
    }

    private static void PrintImage(byte[] imageBytes, string title)
    {
        var source = ImageService.ToImageSource(imageBytes);
        if (source == null)
        {
            MessageBox.Show("Не удалось загрузить изображение.", "Печать", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var dlg = new PrintDialog();
        if (dlg.ShowDialog() != true) return;

        var img = new Image
        {
            Source = source,
            Stretch = Stretch.Uniform
        };

        var printableWidth = dlg.PrintableAreaWidth;
        var printableHeight = dlg.PrintableAreaHeight;
        img.Width = printableWidth;
        img.Height = printableHeight;
        img.Measure(new Size(printableWidth, printableHeight));
        img.Arrange(new Rect(new Point(0, 0), new Size(printableWidth, printableHeight)));

        dlg.PrintVisual(img, title);
    }

    private void ExportDeviceWord()
    {
        if (SelectedDevice == null) return;
        var path = _fileDialog.ShowSaveWord();
        if (string.IsNullOrWhiteSpace(path)) return;
        var parts = _partRepo.GetByDevice(SelectedDevice.Id);
        _exportService.ExportToWord(path, SelectedDevice, parts, includeDeviceDrawing: true);
        MessageBox.Show("Экспорт прибора в Word завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportDeviceExcel()
    {
        if (SelectedDevice == null) return;
        var path = _fileDialog.ShowSaveExcel();
        if (string.IsNullOrWhiteSpace(path)) return;
        var parts = _partRepo.GetByDevice(SelectedDevice.Id);
        _exportService.ExportToExcel(path, SelectedDevice, parts);
        MessageBox.Show("Экспорт прибора в Excel завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportDeviceXml()
    {
        if (SelectedDevice == null) return;
        var path = _fileDialog.ShowSaveXml();
        if (string.IsNullOrWhiteSpace(path)) return;
        var parts = _partRepo.GetByDevice(SelectedDevice.Id);
        _exportService.ExportToXml(path, SelectedDevice, parts);
        MessageBox.Show("Экспорт прибора в XML завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportSelectedWord()
    {
        if (SelectedDevice == null || SelectedParts.Count == 0) return;
        var path = _fileDialog.ShowSaveWord();
        if (string.IsNullOrWhiteSpace(path)) return;
        _exportService.ExportToWord(path, SelectedDevice, SelectedParts.ToList(), includeDeviceDrawing: false);
        MessageBox.Show("Экспорт выбранных деталей в Word завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportSelectedExcel()
    {
        if (SelectedDevice == null || SelectedParts.Count == 0) return;
        var path = _fileDialog.ShowSaveExcel();
        if (string.IsNullOrWhiteSpace(path)) return;
        _exportService.ExportToExcel(path, SelectedDevice, SelectedParts.ToList());
        MessageBox.Show("Экспорт выбранных деталей в Excel завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportSelectedXml()
    {
        if (SelectedDevice == null || SelectedParts.Count == 0) return;
        var path = _fileDialog.ShowSaveXml();
        if (string.IsNullOrWhiteSpace(path)) return;
        _exportService.ExportToXml(path, SelectedDevice, SelectedParts.ToList());
        MessageBox.Show("Экспорт выбранных деталей в XML завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ImportDeviceWord()
    {
        var path = _fileDialog.ShowOpenWord();
        if (string.IsNullOrWhiteSpace(path)) return;
        var (device, parts) = _importService.ImportFromWord(path);
        ImportDeviceWithParts(device, parts);
    }

    private void ImportDeviceExcel()
    {
        var path = _fileDialog.ShowOpenExcel();
        if (string.IsNullOrWhiteSpace(path)) return;
        var (device, parts) = _importService.ImportFromExcel(path);
        ImportDeviceWithParts(device, parts);
    }

    private void ImportDeviceXml()
    {
        var path = _fileDialog.ShowOpenXml();
        if (string.IsNullOrWhiteSpace(path)) return;
        var (device, parts) = _importService.ImportFromXml(path);
        ImportDeviceWithParts(device, parts);
    }

    private void ImportPartsWord()
    {
        if (SelectedDevice == null) return;
        var path = _fileDialog.ShowOpenWord();
        if (string.IsNullOrWhiteSpace(path)) return;
        var (_, parts) = _importService.ImportFromWord(path);
        ImportPartsIntoDevice(SelectedDevice, parts);
    }

    private void ImportPartsExcel()
    {
        if (SelectedDevice == null) return;
        var path = _fileDialog.ShowOpenExcel();
        if (string.IsNullOrWhiteSpace(path)) return;
        var (_, parts) = _importService.ImportFromExcel(path);
        ImportPartsIntoDevice(SelectedDevice, parts);
    }

    private void ImportPartsXml()
    {
        if (SelectedDevice == null) return;
        var path = _fileDialog.ShowOpenXml();
        if (string.IsNullOrWhiteSpace(path)) return;
        var (_, parts) = _importService.ImportFromXml(path);
        ImportPartsIntoDevice(SelectedDevice, parts);
    }

    private void ImportDeviceWithParts(DeviceDto device, List<PartImportItem> parts)
    {
        if (string.IsNullOrWhiteSpace(device.Name))
        {
            MessageBox.Show("Импорт: не найдено наименование прибора.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        device.Id = _deviceRepo.Insert(device);
        foreach (var item in parts)
        {
            item.Part.DeviceId = device.Id;
            item.Part.Id = _partRepo.Insert(item.Part);
            ApplyImportedParameters(item);
        }
        LoadDevices();
        SelectedDevice = Devices.FirstOrDefault(d => d.Id == device.Id);
        MessageBox.Show("Импорт прибора завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ImportPartsIntoDevice(DeviceDto device, List<PartImportItem> parts)
    {
        foreach (var item in parts)
        {
            item.Part.DeviceId = device.Id;
            item.Part.Id = _partRepo.Insert(item.Part);
            ApplyImportedParameters(item);
        }
        LoadParts();
        MessageBox.Show("Импорт деталей завершен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ApplyImportedParameters(PartImportItem item)
    {
        if (PartTypeProvider.IsCustom(item.Part.PartType, out var typeId))
        {
            var values = _importService.CreateCustomValues(typeId, item.Parameters);
            foreach (var v in values) v.PartId = item.Part.Id;
            _customRepo.UpsertValues(item.Part.Id, values);
        }
        else
        {
            var spec = _importService.CreateSpecFromParameters(item.Part.PartType, item.Parameters);
            SpecFormatter.SetPartId(spec, item.Part.Id);
            _specRepo.UpsertSpec(item.Part.PartType, spec);
        }
    }

    private void OnSelectedPartsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

