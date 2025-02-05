namespace GMCompanion.Api.Domain;

public class InventoryItem : BaseStorage
{
    public uint CharacterId { get; set; }
    public virtual Character Character { get; set; }
    public uint ItemId { get; set; }    
    public virtual Item Item { get; set; }
    public uint Quantity { get; set; }
}
