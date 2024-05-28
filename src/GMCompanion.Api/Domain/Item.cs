namespace GMCompanion.Api.Domain;

public class Item : BaseStorage
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Rarity { get; set; }
    public Uri? Image { get; set; }
    public List<string> Tags { get; set; }
    public Double Weight { get; set; }
    public Double Cost { get; set; }
    public string FilterType { get; set; } 
    public virtual ICollection<InventoryItem> Inventories{ get; set; }
}