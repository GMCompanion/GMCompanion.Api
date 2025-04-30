using GMCompanion.Api.Domain;
using GMCompanion.Api.DTOs;
using GMCompanion.Api.Infrastucture;
using GMCompanion.Api.SocketHubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Domain.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

builder.Services.AddPostgresTaskContext(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors_local",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000",
                                              "https://localhost:3000",
                                              "https://localhost:7164",
                                              "http://localhost:5173")
                          .AllowAnyHeader().AllowAnyMethod()
                          .SetIsOriginAllowed((host) => true)
                          .AllowCredentials(); ;
                      });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("cors_local");

app.MapGet("/items", async (EFRepository<Item> itemRepository) =>
{
    var getAllItemsResult = await itemRepository.GetAll();
    if (!getAllItemsResult.IsSuccess) Results.Problem();
    return Results.Ok(getAllItemsResult.Response);
});

app.MapGet("/items/{id}", async (uint id, EFRepository<Item> itemRepository) =>
{
    var getResult = await itemRepository.Get(id);
    if (!getResult.IsSuccess) return Results.Problem();
    return Results.Ok(getResult.Response);
});

app.MapPost("/items", async ([FromBody] Item item, EFRepository<Item> itemRepository) =>
{
    var createResult = await itemRepository.Create(item);
    if (!createResult.IsSuccess) return Results.Problem();
    return Results.Ok(createResult.Response);
});

app.MapPut("/items/{id}", async (uint id, [FromBody] Item item, EFRepository<Item> itemRepository) =>
{
    var itemToUpdateResult = await itemRepository.Get(id);

    if (itemToUpdateResult is null) return Results.NotFound();

    var itemToUpdate = itemToUpdateResult.Response;

    itemToUpdate.Name = item.Name;

    var updated = await itemRepository.Update(itemToUpdate);

    return Results.Ok();
});

app.MapDelete("/items/{id}", async (uint id, EFRepository<Item> itemRepository) =>
{
    var itemToDeleteResult = await itemRepository.Get(id);

    if (!itemToDeleteResult.IsSuccess) return Results.BadRequest("Not Found");

    var itemToDelete = itemToDeleteResult.Response;

    await itemRepository.Delete(itemToDelete);

    return Results.Ok();
});

app.MapGet("/characters", async (EFRepository<Character> characterRepository) =>
{
    var getCharacterResult = await characterRepository.GetAll();
    return Results.Ok(getCharacterResult.Response);
});

app.MapGet("/characters/{id}", async (uint id, EFRepository<Character> characterRepository) =>
{
    var getcharacterResult = await characterRepository.Get(id);
    if (!getcharacterResult.IsSuccess) return Results.Problem();
    return Results.Ok(getcharacterResult.Response);
});

app.MapPost("/characters", async ([FromBody] Character character, EFRepository<Character> characterRepository, IHubContext<InventoryHub, IInventoryClient> context) =>
{
    var createResult = await characterRepository.Create(character);
    if (!createResult.IsSuccess) return Results.Problem();
    await context.Clients.All.AddCharacter(createResult.Response);
    return Results.Ok(createResult.Response);
});

app.MapPut("/characters/{id}", async (uint id, [FromBody] Character character, EFRepository<Character> characterRepository, IHubContext<InventoryHub, IInventoryClient> context) =>
{
    var characterToUpdateResult = await characterRepository.Get(id);

    if (characterToUpdateResult is null) return Results.NotFound();

    var characterToUpdate = characterToUpdateResult.Response;

    characterToUpdate.Name = character.Name;

    var updated = await characterRepository.Update(characterToUpdate);

    context.Clients.All.UpdateCharacter(characterToUpdate);


    return Results.Ok();
});

app.MapDelete("/characters/{id}", async (uint id, EFRepository<Character> characterRepository, IHubContext<InventoryHub, IInventoryClient> context) =>
{
    var characterToDeleteResult = await characterRepository.Get(id);

    if (!characterToDeleteResult.IsSuccess) return Results.BadRequest("Not Found");

    var characterToDelete = characterToDeleteResult.Response;

    await characterRepository.Delete(characterToDelete);

    context.Clients.All.DeleteCharacter(characterToDelete);

    return Results.Ok();
});

app.MapGet("/characters/{id}/inventory/{itemId}", async (uint id, uint itemId, IHubContext<InventoryHub, IInventoryClient> context, MarketContext dbContext) =>
{
    Character? character = dbContext.Characters
        .Where(c => c.Id == id)
        .Include(c => c.Inventory
            .Where(inv => inv.Item.Id == itemId)) // filter Inventory based on Item.Id
        .ThenInclude(inv => inv.Item)          // include the related Item
        .FirstOrDefault();

    if (character is null) return Results.BadRequest($"Character {id} Not Found");
    if (character.Inventory is null || !character.Inventory.Any()) return Results.BadRequest($"Character {id} has not Item {itemId}");

    InventoryItem inventoryItem = character.Inventory.First();

    return Results.Ok(inventoryItem);
});

app.MapDelete("/characters/{id}/inventory/{itemId}", async (uint id, uint itemId, IHubContext<InventoryHub, IInventoryClient> context, MarketContext dbContext) =>
{
    Character? character = dbContext.Characters
        .Where(c => c.Id == id)
        .Include(c => c.Inventory
            .Where(inv => inv.Item.Id == itemId)) // filter Inventory based on Item.Id
            .ThenInclude(inv => inv.Item)          // include the related Item
        .FirstOrDefault();

    if (character is null) return Results.BadRequest($"Character {id} Not Found");
    if (character.Inventory is null || !character.Inventory.Any()) return Results.BadRequest($"Character {id} has not Item {itemId}");

    InventoryItem inventoryItem = character.Inventory.First();

    dbContext.InventoryItems.Remove(inventoryItem);
    await dbContext.SaveChangesAsync();

    await context.Clients.Group($"group_{id}").SendInventoryItemDelete(inventoryItem.ToInventoryItemDto());

    return Results.Ok(inventoryItem);
});

app.MapPost("/characters/{id}/inventory/{itemId}", async (uint id, uint itemId, IHubContext<InventoryHub, IInventoryClient> context, MarketContext dbContext) =>
{
    bool itemExists = await dbContext.Items
    .AnyAsync(i => i.Id == itemId);

    if (!itemExists) return Results.BadRequest($"Item {itemId} not found");

    Character? character = dbContext.Characters
        .Where(c => c.Id == id)
        .Include(c => c.Inventory
            .Where(inv => inv.Item.Id == itemId)) // filter Inventory based on Item.Id
            .ThenInclude(inv => inv.Item)          // include the related Item
        .FirstOrDefault();

    if (character is null) return Results.BadRequest($"Character {id} Not Found");
    if (character.Inventory is not null && character.Inventory.Any())
    {
        InventoryItem inventoryItem = character.Inventory.First();
        return Results.Ok(inventoryItem);
    }

    InventoryItem inventoryItemToAdd = new InventoryItem
    {
        CharacterId = id,
        ItemId = itemId,
        Quantity = 1
    };

    dbContext.InventoryItems.Add(inventoryItemToAdd);
    await dbContext.SaveChangesAsync();

    await context.Clients.Group($"group_{id}").SendInventoryItemAdd(inventoryItemToAdd.ToInventoryItemDto());

    return Results.Ok(inventoryItemToAdd);
});

app.MapPut("/characters/{id}/inventory/{itemId}", async (uint id, uint itemId, [FromBody] UpdateItemInventoryRq updateRq, IHubContext<InventoryHub, IInventoryClient> context, MarketContext dbContext) =>
{
    Character? character = dbContext.Characters
        .Where(c => c.Id == id)
        .Include(c => c.Inventory
            .Where(inv => inv.Item.Id == itemId)) // filter Inventory based on Item.Id
            .ThenInclude(inv => inv.Item)          // include the related Item
        .FirstOrDefault();

    if (character is null) return Results.BadRequest($"Character {id} Not Found");
    if (character.Inventory is null || !character.Inventory.Any()) return Results.BadRequest($"Character {id} has not Item {itemId}");

    InventoryItem inventoryItem = character.Inventory.First();
    inventoryItem.Quantity = updateRq.Quantity;

    dbContext.InventoryItems.Update(inventoryItem);
    await dbContext.SaveChangesAsync();

    await context.Clients.Group($"group_{id}").SendInventoryItemUpdate(inventoryItem.ToInventoryItemDto());

    return Results.Ok(inventoryItem);
});

app.MapHub<InventoryHub>("/characters/inventory");

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<MarketContext>())
    await context.Database.EnsureCreatedAsync();

app.Run();

public class UpdateItemInventoryRq
{
    public uint Quantity { get; set; }
}
