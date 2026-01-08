namespace CRUDSamsonovCAD.Models;

public sealed class Spec07_15_ScrewDto
{
    public int PartId { get; set; }
    public decimal? Diameter { get; set; }
    public decimal? Length { get; set; }
    public decimal? ThreadPitch { get; set; }
    public string? HeadType { get; set; }
    public string? Material { get; set; }
}
