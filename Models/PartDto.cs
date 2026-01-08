namespace CRUDSamsonovCAD.Models;

public sealed class PartDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string Name { get; set; } = "";
    public string PartType { get; set; } = "";
    public string PartTypeDisplayName { get; set; } = "";
    public string? NxModelPath { get; set; }
    public byte[]? DrawingImage { get; set; }
    public string? Notes { get; set; }
}
