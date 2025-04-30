namespace GMCompanion.Api.DTOs;

public class InventoryItemDto
{
    public uint Id { get; set; }
    public uint CharacterId { get; set; }
    public uint ItemId { get; set; }
    public ItemDto Item { get; set; }
    public uint Quantity { get; set; }
}

public class ItemDto
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Rarity { get; set; }
    public Uri? Image { get; set; }
    public List<string> Tags { get; set; }
    public Double Weight { get; set; }
    public Double Cost { get; set; }
    public string FilterType { get; set; }
}
