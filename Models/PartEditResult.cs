using System.Collections.Generic;

namespace CRUDSamsonovCAD.Models;

public sealed class PartEditResult
{
    public bool IsCustom { get; set; }
    public object? Spec { get; set; }
    public List<CustomPartValueDto> CustomValues { get; set; } = new();
}
