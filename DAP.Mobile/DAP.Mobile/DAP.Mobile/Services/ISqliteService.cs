using System.Collections.Generic;
using System.Threading.Tasks;
using DAP.Mobile.Models;

namespace DAP.Mobile.Services
{
    public interface ISqliteService
    {
        Task<List<TEntity>> Get<TEntity>() where TEntity : Entity, new();
        Task<TEntity> Get<TEntity>(int id) where TEntity : Entity, new();
        Task<int> Save<TEntity>(TEntity item) where TEntity : Entity, new();
        Task<int> Delete<TEntity>(TEntity item) where TEntity : Entity, new();
    }
}
