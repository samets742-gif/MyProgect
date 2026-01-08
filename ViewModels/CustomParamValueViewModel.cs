using System.Collections.ObjectModel;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class CustomParamValueViewModel : BaseViewModel
{
    public int ParamId { get; init; }
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "Text";

    private string? _valueText;
    public string? ValueText
    {
        get => _valueText;
        set { _valueText = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> ListItems { get; } = new();
}
