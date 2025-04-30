using GMCompanion.Api.Domain;
using Riok.Mapperly.Abstractions;

namespace GMCompanion.Api.DTOs;

[Mapper]
public static partial class Mapping 
{
    public static partial InventoryItemDto ToInventoryItemDto(this InventoryItem entity);
}
