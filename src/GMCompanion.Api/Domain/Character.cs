namespace GMCompanion.Api.Domain;

public class Character : BaseStorage
{
    public string Name { get; set; } = "";
    public string Player { get; set; } = "";
    public List<Item> Items { get; set; } = new List<Item>();
}
