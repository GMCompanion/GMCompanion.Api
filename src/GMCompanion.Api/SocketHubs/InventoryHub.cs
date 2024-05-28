using GMCompanion.Api.Domain;
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

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Connected Client {Context.ConnectionId}");
        return Task.CompletedTask;
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

        MemoryStream ms = new();
        var options = new JsonSerializerOptions();
        options.ReferenceHandler = ReferenceHandler.Preserve;

        JsonSerializer.Serialize(ms, character.Inventory, options);
        ms.Position = 0;
        StreamReader sr = new(ms);

        await Clients.Caller.SendItemsUpdate(sr.ReadToEnd());
    }
}

public interface IInventoryClient
{
    public Task SendItemUpdate(string itemMessage);
    public Task SendItemsUpdate(string itemMessage);

}