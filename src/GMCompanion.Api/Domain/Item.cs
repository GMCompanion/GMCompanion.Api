namespace GMCompanion.Api.Domain;

public class Item : BaseStorage
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string NormalCost { get; set; }
    public string CheapCost { get; set; }
    public string ExpensiveCost { get; set; }
    public bool LimitedStock { get; set; }
    public bool RuralStock { get; set; }
    public bool UrbanStock { get; set; }
    public bool PremiumStock { get; set;}
    public List<Character> Characters { get; set; }
}