using GMCompanion.Api.Domain;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Common;

namespace GMCompanion.Api.Infrastucture;

public class EFRepository<T> : IRepository<T> where T : BaseStorage
{
    private readonly MarketContext _dbContext;

    public EFRepository(MarketContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T>> Create(T element)
    {
        _dbContext.Add<T>(element);
        await _dbContext.SaveChangesAsync();
        return element;
    }

    public async Task<Result<T>> Delete(T element)
    {
        _dbContext.Remove(element);
        await _dbContext.SaveChangesAsync();
        return element;
    }

    public async Task<Result<T>> Get(uint id)
    {
        var element = await _dbContext.Set<T>().FirstOrDefaultAsync(s => s.Id == id);
        return element;
    }

    public async Task<Result<IEnumerable<T>>> GetAll()
    {
        var elements = await _dbContext.Set<T>().ToListAsync();
        return elements;
    }

    public async Task<Result<T>> Update(T element)
    {
        _dbContext.Set<T>().Update(element);
        await _dbContext.SaveChangesAsync();
        return element;
    }
}
