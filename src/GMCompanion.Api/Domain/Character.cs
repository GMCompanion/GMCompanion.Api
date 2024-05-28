namespace GMCompanion.Api.Domain;

public class Character : BaseStorage
{
    public string Name { get; set; } = "";
    public string Player { get; set; } = "";
    public virtual ICollection<InventoryItem> Inventory{ get; set; }
}
