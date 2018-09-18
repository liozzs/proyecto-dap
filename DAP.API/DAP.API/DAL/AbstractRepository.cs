using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace DAP.API.DAL
{
    public abstract class AbstractRepository<T>: IRepository<T>
    {
        protected IDbConnection _conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["DAP_DB"].ConnectionString);

        public abstract T Get(int ID);
        public abstract List<T> GetAll();
        public abstract T Insert(T t);
    }
}
