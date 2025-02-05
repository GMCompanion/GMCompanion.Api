using GMCompanion.Api.Domain;
using TaskManager.Domain.Common;

namespace GMCompanion.Api.Infrastucture;

public interface IInventoryRepository : IRepository<InventoryItem>
{
    public Task<Result<InventoryItem>> GetByCharacterAndItem(uint characterId, uint itemId);
}
