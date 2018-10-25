using DAP.Mobile.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DAP.Mobile.Services
{
    public class SqliteService : ISqliteService
    {
        bool initialized;
        readonly SQLiteAsyncConnection database;

        public SqliteService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dap.db3");
            database = new SQLiteAsyncConnection(dbPath);
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (!initialized)
                {
                    //database.CreateTableAsync<Notification>().Wait();
                    database.CreateTableAsync<Planification>().Wait();
                    database.CreateTableAsync<Pill>().Wait();
                    initialized = true;
                }
            }
            catch (Exception)
            {
                initialized = false;
            }
        }

        public Task<List<TEntity>> Get<TEntity>() where TEntity : Entity, new()
        {
            return database.Table<TEntity>().ToListAsync();
        }

        public Task<TEntity> Get<TEntity>(int id) where TEntity : Entity, new()
        {
            return database.Table<TEntity>().FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<int> Save<TEntity>(TEntity item) where TEntity : Entity, new()
        {
            if (item.Id != 0)
            {
                return database.UpdateAsync(item);
            }
            else
            {
                return database.InsertAsync(item);
            }
        }

        public Task<int> Delete<TEntity>(TEntity item) where TEntity : Entity, new()
        {
            return database.DeleteAsync(item);
        }
    }
}