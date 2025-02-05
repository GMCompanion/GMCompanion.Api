using GMCompanion.Api.Domain;
using GMCompanion.Api.Infrastucture;
using GMCompanion.Api.SocketHubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPostgresTaskContext(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors_local",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000",
                                              "https://localhost:3000",
                                              "https://localhost:7164")
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
    var getAllItemsResult =  await itemRepository.GetAll();
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
    
    if(!itemToDeleteResult.IsSuccess) return Results.BadRequest("Not Found");

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

app.MapPost("/characters", async ([FromBody] Character character, EFRepository<Character> characterRepository) =>
{
    var createResult = await characterRepository.Create(character);
    if (!createResult.IsSuccess) return Results.Problem();
    return Results.Ok(createResult.Response);
});

app.MapPut("/characters/{id}", async (uint id, [FromBody] Character character, EFRepository<Character> characterRepository) =>
{
    var characterToUpdateResult = await characterRepository.Get(id);

    if (characterToUpdateResult is null) return Results.NotFound();

    var characterToUpdate = characterToUpdateResult.Response;

    characterToUpdate.Name = character.Name;

    var updated = await characterRepository.Update(characterToUpdate);

    return Results.Ok();
});

app.MapDelete("/characters/{id}", async (uint id, EFRepository<Character> characterRepository) =>
{
    var characterToDeleteResult = await characterRepository.Get(id);

    if (!characterToDeleteResult.IsSuccess) return Results.BadRequest("Not Found");

    var characterToDelete = characterToDeleteResult.Response;

    await characterRepository.Delete(characterToDelete);

    return Results.Ok();
});

app.MapPut("/characters/{id}/inventory", async (uint id, [FromBody] UpdateItemInventoryRq rq, IHubContext<InventoryHub, IInventoryClient> context, MarketContext dbContext) => 
{
    var item = dbContext.Items.FirstOrDefault(i => i.Id == rq.ItemId);
    if (item is null) return Results.BadRequest($"Item {rq.ItemId} Not Found");

    var character = dbContext.Characters.Include(c => c.Inventory).First(c => c.Id == id);
    if (character is null) return Results.BadRequest($"Character {id} Not Found");

    var inventoryItemToUpdate = character.Inventory.FirstOrDefault(i => i.ItemId == rq.ItemId);

    if (inventoryItemToUpdate is null)
    {
        inventoryItemToUpdate = new InventoryItem()
        {
            CharacterId = character.Id,
            ItemId = rq.ItemId,
            Quantity = rq.Quantity,
        };

        character.Inventory.Add(inventoryItemToUpdate);
    }
    else
    {
        inventoryItemToUpdate.Quantity = inventoryItemToUpdate.Quantity + rq.Quantity;
    }

    dbContext.SaveChanges();

    var options = new JsonSerializerOptions();
    options.ReferenceHandler = ReferenceHandler.Preserve;

    MemoryStream ms = new();
    JsonSerializer.Serialize(ms, inventoryItemToUpdate, options);
    ms.Position = 0;
    StreamReader sr = new(ms);
    await context.Clients.Group($"group_{id}").SendItemUpdate(sr.ReadToEnd());

    return Results.Ok();
});

app.MapDelete("/characters/{id}/inventory", async (uint id, [FromBody] UpdateItemInventoryRq rq, IHubContext<InventoryHub, IInventoryClient> context, MarketContext dbContext) =>
{
    var item = dbContext.Items.First(i => i.Id == rq.ItemId);
    if (item is null) return Results.BadRequest($"Item {rq.ItemId} Not Found");

    var character = dbContext.Characters.Include(c => c.Inventory).First(c => c.Id == id);
    if (character is null) return Results.BadRequest($"Character {id} Not Found");

    var inventoryItemToUpdate = character.Inventory.FirstOrDefault(i => i.ItemId == rq.ItemId);

    if (inventoryItemToUpdate is null)
    {
        return Results.BadRequest($"Can not subtract to not added item");
    }
    
    inventoryItemToUpdate.Quantity = inventoryItemToUpdate.Quantity - rq.Quantity;
    
    if(inventoryItemToUpdate.Quantity <= 0)
    {
        character.Inventory.Remove(inventoryItemToUpdate); 
    } 

    dbContext.SaveChanges();


    var options = new JsonSerializerOptions();
    options.ReferenceHandler = ReferenceHandler.Preserve;

    MemoryStream ms = new();
    JsonSerializer.Serialize(ms, inventoryItemToUpdate, options);
    ms.Position = 0;
    StreamReader sr = new(ms);
    await context.Clients.Group($"group_{id}").SendItemUpdate(sr.ReadToEnd());

    return Results.Ok();    

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
    public uint ItemId { get; set; }
    public uint Quantity { get; set; }
}
