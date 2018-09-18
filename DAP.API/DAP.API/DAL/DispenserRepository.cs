using System;
using System.Collections.Generic;
using System.Linq;
using DAP.API.Models;

using Dapper;

namespace DAP.API.DAL
{
    public class DispenserRepository: AbstractRepository<Dispenser>
    {
        private string selectSql = "SELECT D.*, U.* FROM Dispenser AS D LEFT JOIN UsuarioDispenser UD ON (D.ID = UD.DispenserId) LEFT JOIN Usuario U ON (UD.UsuarioID = U.ID)";
        public DispenserRepository()
        {
            _conn.Open();
        }

        public override Dispenser Get(int ID)
        {
            Dispenser dispenser = null;
            _conn.Query<Dispenser, Usuario, Dispenser>(selectSql + " WHERE D.ID = @Id", (d, u) => {
                if (dispenser == null)
                {
                    dispenser = d;
                    dispenser.Usuarios = new List<Usuario>();
                }
                dispenser.Usuarios.Add(u);
                return dispenser;
            }, new { Id = ID }, splitOn: "DispenserID, ID").AsQueryable();

            return dispenser;
        }

        public Dispenser Get(string DireccionMAC) 
        {
            Dispenser dispenser = null;
            _conn.Query<Dispenser, Usuario, Dispenser>(selectSql + " WHERE D.DireccionMAC = @DireccionMAC", (d, u) =>
            {
                if (dispenser == null)
                {
                    dispenser = d;
                    dispenser.Usuarios = new List<Usuario>();
                }
                if (u != null)
                    dispenser.Usuarios.Add(u);
                return dispenser;
            }, new { DireccionMAC }, splitOn: "DispenserID, ID").AsQueryable();

            return dispenser;
        }

        public override List<Dispenser> GetAll()
        {
            var lookup = new Dictionary<int, Dispenser>();
            _conn.Query<Dispenser, Usuario, Dispenser>(selectSql, (d, u) =>
            {
                if (!lookup.TryGetValue(d.ID, out Dispenser dispenser))
                {
                    lookup.Add(d.ID, dispenser = d);
                }
                if (dispenser.Usuarios == null)
                {
                    dispenser.Usuarios = new List<Usuario>();
                }
                dispenser.Usuarios.Add(u);
                return dispenser;
            }, splitOn: "DispenserID,ID").AsQueryable();

            return lookup.Select(x => x.Value).ToList();
        }

        internal bool InsertRelationship(int UsuarioId, int DispenserID)
        {
            string sql = @"INSERT INTO UsuarioDispenser(UsuarioID, DispenserID) VALUES(@UsuarioID, @DispenserID)";
            int rowsAffected = _conn.Execute(sql, new { UsuarioId, DispenserID });

            return (rowsAffected > 0);
        }

        public override Dispenser Insert(Dispenser t)
        {
            Dispenser dispenser = Get(t.DireccionMAC);
            if (dispenser != null)
            {
                return dispenser;
            }
            string sql = @"INSERT INTO Dispenser(DireccionMAC, Nombre) VALUES(@DireccionMAC, @Nombre);
                            SELECT LAST_INSERT_ID()";

            int insertedId = _conn.Query<int>(sql, t).SingleOrDefault();
            t.ID = insertedId;

            return t;
        }
    }
}
