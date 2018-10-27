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
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "dap.db3");
            database = new SQLiteAsyncConnection(dbPath);
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (!initialized)
                {
                    database.CreateTableAsync<PlanificationAction>().Wait();
                    if(Get<PlanificationAction>().Result.Count == 0)
                    {
                        //database.InsertAsync(new PlanificationAction() { Id = 2, Name = "Ninguna", Description = "La planificación seguirá dispensando la medicación en los horarios establecidos." });
                        database.InsertAsync(new PlanificationAction() { Id = 1, Name = "Replanificar", Description = "En caso de no haber tomado la medicación en el momento indicado, se replanificarán los próximos expendios, corriendo el horario para cumplir con los intervalos establecidos." });
                        database.InsertAsync(new PlanificationAction() { Id = 2, Name = "Bloquear", Description = "Al pasar una hora sin haber tomado la medicación, la planificación se bloqueará y no se dispensarán más medicamentos, dando por finalizado el tratamiento." });
                    }
                    database.CreateTableAsync<Planification>().Wait();
                    database.CreateTableAsync<Pill>().Wait();
                    initialized = true;
                }
            }
            catch
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
            try
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
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public Task<int> Delete<TEntity>(TEntity item) where TEntity : Entity, new()
        {
            return database.DeleteAsync(item);
        }
    }
}