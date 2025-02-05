using GMCompanion.Api.Domain;
using TaskManager.Domain.Common;

namespace GMCompanion.Api.Infrastucture;

public interface IRepository<T> where T : class
{
    public Task<Result<T>> Get(uint id);
    public Task<Result<IEnumerable<T>>> GetAll();
    public Task<Result<T>> Delete(T element);
    public Task<Result<T>> Update(T character);
    public Task<Result<T>> Create(T character);
}