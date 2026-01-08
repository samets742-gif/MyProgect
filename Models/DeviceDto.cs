namespace CRUDSamsonovCAD.Models;

public sealed class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? NxModelPath { get; set; }
    public byte[]? DrawingImage { get; set; }
}
