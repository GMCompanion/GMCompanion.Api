using GMCompanion.Api.Domain;
using GMCompanion.Api.DTOs;
using GMCompanion.Api.Infrastucture;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GMCompanion.Api.SocketHubs;

public sealed class InventoryHub : Hub<IInventoryClient>
{
    private readonly MarketContext _context;
    public InventoryHub(MarketContext dbContext)
    {
        _context = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Connected Client {Context.ConnectionId}");

        var characters = await _context.Characters.ToListAsync();

        await Clients.Caller.SendCharacters(characters);
        
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Disconnected Client {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ConnectToInventory(uint characterId)
    {
        var character = _context.Characters.Include(c => c.Inventory).ThenInclude(i => i.Item).FirstOrDefault(c => c.Id == characterId);

        if (character == null) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{characterId}");

        List<InventoryItem> items = character.Inventory.ToList();
        

        await Clients.Caller.SendInventory(items.Select(i => i.ToInventoryItemDto()).ToList());
    }

    public async Task DisconectToInventory(uint characterId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{characterId}");
    }
}

public interface IInventoryClient
{
    public Task SendInventory(List<InventoryItemDto> item);
    public Task SendInventoryItemUpdate(InventoryItemDto item);
    public Task SendInventoryItemDelete(InventoryItemDto item);
    public Task SendInventoryItemAdd(InventoryItemDto item);

    public Task SendCharacters(List<Character> characters);
    public Task UpdateCharacter(Character character);
    public Task DeleteCharacter(Character character);
    public Task AddCharacter(Character character);
}