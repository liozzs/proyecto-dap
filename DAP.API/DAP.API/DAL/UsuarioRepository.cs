using System;
using System.Collections.Generic;
using System.Linq;
using DAP.API.Models;

using Dapper;

namespace DAP.API.DAL
{
    public class UsuarioRepository: AbstractRepository<Usuario>
    {
        private string selectSql = "SELECT U.*, D.* FROM Usuario AS U LEFT JOIN UsuarioDispenser UD ON (U.ID = UD.UsuarioId) LEFT JOIN Dispenser D ON (UD.DispenserID = D.ID)";

        public UsuarioRepository()
        {
            _conn.Open();
        }

        public override Usuario Get(int ID)
        {
            Usuario usuario = null;
            _conn.Query<Usuario, Dispenser, Usuario>(selectSql + " WHERE U.ID = @Id", (u, d) => {
                if (usuario == null)
                {
                    usuario = u;
                    usuario.Dispensers = new List<Dispenser>();
                }
                usuario.Dispensers.Add(d);
                return usuario;
            }, new { Id = ID }, splitOn: "UsuarioID, ID").AsQueryable();

            return usuario;
        }
        /*
        public Usuario Get(LoginRequest loginRequest)
        {
            return _conn.Query<Usuario>("SELECT U.* FROM Usuario U WHERE U.Email = @Email AND U.Password = @Password", loginRequest).SingleOrDefault();
        }
        */
        public Usuario Get(string Email)
        {
            Usuario usuario = null;
            _conn.Query<Usuario, Dispenser, Usuario>(selectSql + " WHERE U.Email = @Email", (u, d) => {
                if (usuario == null)
                {
                    usuario = u;
                    usuario.Dispensers = new List<Dispenser>();
                }
                if (d != null)
                    usuario.Dispensers.Add(d);
                return usuario;
            }, new { Email }, splitOn: "UsuarioID, ID").AsQueryable();

            return usuario;
        }

        public override List<Usuario> GetAll()
        {
            var lookup = new Dictionary<int, Usuario>();
            _conn.Query<Usuario, Dispenser, Usuario>(selectSql, (u, d) =>
            {
                if (!lookup.TryGetValue(u.ID, out Usuario usuario))
                {
                    lookup.Add(u.ID, usuario = u);
                }
                if (usuario.Dispensers == null)
                {
                    usuario.Dispensers = new List<Dispenser>();
                }
                usuario.Dispensers.Add(d);
                return usuario;
            }, splitOn: "UsuarioID, ID").AsQueryable();

            return lookup.Select(x => x.Value).ToList();
        }

        public override Usuario Insert(Usuario t)
        {
            Usuario usuario = Get(t.Email);
            if (usuario != null) {
                return usuario;
            }
            string sql = @"INSERT INTO Usuario(Nombre, Password, Salt, Email) VALUES(@Nombre, @Password, @Salt, @Email);
                            SELECT LAST_INSERT_ID()";

            int insertedId = _conn.Query<int>(sql, t).SingleOrDefault();
            t.ID = insertedId;

            return t;
        }

        public bool Insert(Dispenser dispenser, Usuario usuario) {
            string sql = @"INSERT INTO Dispenser(DireccionMAC, Nombre) VALUES(@DireccionMAC, @Nombre);
                            SELECT LAST_INSERT_ID()";

            int insertedId = _conn.Query<int>(sql, dispenser).SingleOrDefault();
            dispenser.ID = insertedId;

            sql = @"INSERT INTO UsuarioDispenser(UsuarioID, DispenserID) VALUES(@UsuarioID, @DispenserID)";
            int rowsAffected = _conn.Execute(sql, new { UsuarioID = usuario.ID, DispenserID = dispenser.ID });

            return (rowsAffected > 0);
        }
    }
}
