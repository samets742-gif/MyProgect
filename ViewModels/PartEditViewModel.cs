using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.Services;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class PartEditViewModel : BaseViewModel
{
    private readonly SpecRepository _specRepo;
    private readonly CustomPartRepository _customRepo;
    private readonly IFileDialogService _fileDialog;
    private bool _isInitializing;

    public PartDto Part { get; }

    public ObservableCollection<PartTypeItem> PartTypes { get; } = new();
    public ObservableCollection<CustomParamValueViewModel> CustomParams { get; } = new();

    private PartTypeItem? _selectedPartType;
    public PartTypeItem? SelectedPartType
    {
        get => _selectedPartType;
        set
        {
            _selectedPartType = value;
            OnPropertyChanged();

            if (_isInitializing || _selectedPartType == null) return;

            if (Part.PartType != _selectedPartType.Key)
            {
                Part.PartType = _selectedPartType.Key;
                UpdateSpecForType();
            }
        }
    }

    private bool _isCustomType;
    public bool IsCustomType
    {
        get => _isCustomType;
        private set { _isCustomType = value; OnPropertyChanged(); }
    }

    private object _selectedSpec;
    public object SelectedSpec
    {
        get => _selectedSpec;
        set { _selectedSpec = value; OnPropertyChanged(); }
    }

    private ImageSource? _drawingImagePreview;
    public ImageSource? DrawingImagePreview
    {
        get => _drawingImagePreview;
        set { _drawingImagePreview = value; OnPropertyChanged(); }
    }

    public ICommand BrowseImageCommand { get; }
    public ICommand ClearImageCommand { get; }
    public ICommand BrowseNxCommand { get; }

    public PartEditViewModel(PartDto part, object spec, SpecRepository specRepo, CustomPartRepository customRepo, IFileDialogService fileDialog)
    {
        Part = part;
        _specRepo = specRepo;
        _customRepo = customRepo;
        _fileDialog = fileDialog;
        _selectedSpec = spec;

        foreach (var item in PartTypeProvider.Items)
            PartTypes.Add(item);
        foreach (var t in _customRepo.GetTypes())
            PartTypes.Add(new PartTypeItem(PartTypeProvider.GetCustomKey(t.Id), t.Name));

        _isInitializing = true;
        SelectedPartType = PartTypes.FirstOrDefault(x => x.Key == Part.PartType) ?? PartTypes.First();
        _isInitializing = false;

        DrawingImagePreview = ImageService.ToImageSource(Part.DrawingImage);
        UpdateSpecForType();

        BrowseImageCommand = new RelayCommand(_ => BrowseImage());
        ClearImageCommand = new RelayCommand(_ => ClearImage());
        BrowseNxCommand = new RelayCommand(_ => BrowseNx());
    }

    private void BrowseImage()
    {
        var path = _fileDialog.ShowOpenImage();
        if (string.IsNullOrWhiteSpace(path)) return;
        Part.DrawingImage = ImageService.LoadImageBytes(path);
        DrawingImagePreview = ImageService.ToImageSource(Part.DrawingImage);
    }

    private void ClearImage()
    {
        Part.DrawingImage = null;
        DrawingImagePreview = null;
    }

    private void BrowseNx()
    {
        var path = _fileDialog.ShowOpenNxModel();
        if (string.IsNullOrWhiteSpace(path)) return;
        Part.NxModelPath = path;
        OnPropertyChanged(nameof(Part));
    }

    private void UpdateSpecForType()
    {
        if (PartTypeProvider.IsCustom(Part.PartType, out var typeId))
        {
            IsCustomType = true;
            SelectedSpec = _specRepo.CreateEmptySpec(PartTypeProvider.Items.First().Key, Part.Id);
            LoadCustomParams(typeId);
        }
        else
        {
            IsCustomType = false;
            CustomParams.Clear();
            SelectedSpec = _specRepo.CreateEmptySpec(Part.PartType, Part.Id);
        }
    }

    private void LoadCustomParams(int typeId)
    {
        CustomParams.Clear();
        var values = Part.Id > 0 ? _customRepo.GetValues(Part.Id) : new System.Collections.Generic.Dictionary<int, string?>();
        foreach (var p in _customRepo.GetParams(typeId))
        {
            var vm = new CustomParamValueViewModel
            {
                ParamId = p.Id,
                Name = p.ParamName,
                Kind = p.ParamKind,
                ValueText = values.TryGetValue(p.Id, out var v) ? v : null
            };
            if (!string.IsNullOrWhiteSpace(p.ListValues))
            {
                foreach (var item in p.ListValues.Split(';').Select(x => x.Trim()).Where(x => x.Length > 0))
                    vm.ListItems.Add(item);
            }
            CustomParams.Add(vm);
        }
    }

    public System.Collections.Generic.List<CustomPartValueDto> GetCustomValues()
    {
        return CustomParams.Select(p => new CustomPartValueDto
        {
            ParamId = p.ParamId,
            ValueText = p.ValueText
        }).ToList();
    }
}


