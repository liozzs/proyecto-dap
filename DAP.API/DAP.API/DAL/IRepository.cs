using System.Collections.Generic;

namespace DAP.API.DAL
{
    public interface IRepository<T>
    {
        List<T> GetAll();

        T Get(int ID);

        T Insert(T t);
    }
}
