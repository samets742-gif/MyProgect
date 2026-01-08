namespace CRUDSamsonovCAD.Models;

public sealed class CustomPartParamDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string ParamName { get; set; } = "";
    public string ParamKind { get; set; } = "Text"; // Text, Number, Date, List
    public string? ListValues { get; set; }
    public int SortOrder { get; set; }
}
