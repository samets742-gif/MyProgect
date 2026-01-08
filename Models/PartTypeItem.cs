namespace CRUDSamsonovCAD.Models;

public sealed class PartTypeItem
{
    public PartTypeItem(string key, string name)
    {
        Key = key;
        Name = name;
    }

    public string Key { get; }
    public string Name { get; }

    public override string ToString() => Name;
}
