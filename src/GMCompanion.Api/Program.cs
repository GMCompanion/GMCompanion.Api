using GMCompanion.Api.Domain;
using GMCompanion.Api.Infrastucture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                                              "https://localhost:3000").AllowAnyHeader().AllowAnyMethod();
                      });
});

var app = builder.Build();

app.UseCors("cors_local");

app.MapGet("/items", async (EFRepository<Item> itemRepository) =>
{
    return await itemRepository.GetAll();
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
    characterToUpdate.Items = character.Items;

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

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<MarketContext>())
    await context.Database.EnsureDeletedAsync();

using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<MarketContext>())
    await context.Database.EnsureCreatedAsync();

app.Run();
