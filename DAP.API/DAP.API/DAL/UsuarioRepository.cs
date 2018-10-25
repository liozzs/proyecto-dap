using System;
using System.Collections.Generic;
using System.Linq;
using DAP.API.Models;

using Dapper;

namespace DAP.API.DAL
{
    public class UsuarioRepository: AbstractRepository<Usuario>
    {
        //private readonly string selectSql = "SELECT U.*, D.* FROM Usuario AS U LEFT JOIN UsuarioDispenser UD ON (U.ID = UD.UsuarioId) LEFT JOIN Dispenser D ON (UD.DispenserID = D.ID)";
        private readonly string selectSql = "SELECT U.*, D.*, DM.* FROM Usuario AS U " +
            "LEFT JOIN UsuarioDispenser UD ON (U.ID = UD.UsuarioId) LEFT JOIN Dispenser D ON (UD.DispenserID = D.ID) " +
            "LEFT JOIN DispenserMensaje DM ON (D.ID = DM.DispenserID)";

        public UsuarioRepository()
        {
            _conn.Open();
        }

        public override Usuario Get(int ID)
        {
            Usuario usuario = null;
            _conn.Query<Usuario, Dispenser, DispenserMensaje, Usuario>(selectSql + " WHERE U.ID = @Id", (u, d, dm) => {
                if (usuario == null)
                {
                    usuario = u;
                    usuario.Dispensers = new List<Dispenser>();
                }
                if (d != null)
                {
                    if (!usuario.Dispensers.Contains(d))
                    {
                        d.Mensajes = new List<DispenserMensaje>();
                        usuario.Dispensers.Add(d);
                    }
                    Dispenser dispenser = usuario.Dispensers.Find(disp => disp.ID == d.ID);
                    if (dm != null)
                    {
                        dispenser.Mensajes.Add(dm);
                    }
                }
                return usuario;
            }, new { Id = ID }, splitOn: "UsuarioID, ID, ID").AsQueryable();

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
            _conn.Query<Usuario, Dispenser, DispenserMensaje, Usuario>(selectSql + " WHERE U.Email = @Email", (u, d, dm) => {
                if (usuario == null)
                {
                    usuario = u;
                    usuario.Dispensers = new List<Dispenser>();
                }
                if (d != null)
                {
                    if (!usuario.Dispensers.Contains(d))
                    {
                        d.Mensajes = new List<DispenserMensaje>();
                        usuario.Dispensers.Add(d);
                    }
                    Dispenser dispenser = usuario.Dispensers.Find(disp => disp.ID == d.ID);
                    if (dm != null)
                    {
                        dispenser.Mensajes.Add(dm);
                    }
                }
                return usuario;
                /*
                if (usuario == null)
                {
                    usuario = u;
                    usuario.Dispensers = new List<Dispenser>();
                }
                if (d != null)
                    usuario.Dispensers.Add(d);
                return usuario;
                */
            }, new { Email }, splitOn: "UsuarioID, ID, ID").AsQueryable();

            return usuario;
        }

        public override List<Usuario> GetAll()
        {
            var lookup = new Dictionary<int, Usuario>();
            _conn.Query<Usuario, Dispenser, DispenserMensaje, Usuario>(selectSql, (u, d, dm) =>
            {
                if (!lookup.TryGetValue(u.ID, out Usuario usuario))
                {
                    lookup.Add(u.ID, usuario = u);
                }
                if (usuario.Dispensers == null)
                {
                    usuario.Dispensers = new List<Dispenser>();
                }
                if (d != null)
                {
                    if (!usuario.Dispensers.Contains(d))
                    {
                        d.Mensajes = new List<DispenserMensaje>();
                        usuario.Dispensers.Add(d);
                    }
                    if (dm != null)
                    {
                        Dispenser dispenser = usuario.Dispensers.Find(disp => disp.ID == d.ID);
                        dispenser.Mensajes.Add(dm);
                    }
                }
                return usuario;
                /*
                if (usuario == null)
                {
                    usuario = u;
                    usuario.Dispensers = new List<Dispenser>();
                }
                if (d != null)
                {
                    if (!usuario.Dispensers.Contains(d))
                    {
                        d.Mensajes = new List<DispenserMensaje>();
                        usuario.Dispensers.Add(d);
                    }
                    Dispenser dispenser = usuario.Dispensers.Find(disp => disp.ID == d.ID);
                    if (dm != null)
                    {
                        dispenser.Mensajes.Add(dm);
                    }
                }
                return usuario;
                */
            }, splitOn: "UsuarioID, ID, ID").AsQueryable();

            return lookup.Select(x => x.Value).ToList();
        }

        public override Usuario Insert(Usuario t)
        {
            Usuario usuario = Get(t.Email);
            if (usuario != null) {
                return usuario;
            }
            string sql = @"INSERT INTO Usuario(Nombre, Password, Salt, Email, Telefono) VALUES(@Nombre, @Password, @Salt, @Email, @Telefono);
                            SELECT LAST_INSERT_ID()";

            int insertedId = _conn.Query<int>(sql, t).SingleOrDefault();
            t.ID = insertedId;

            return t;
        }

        public bool Insert(Dispenser dispenser, Usuario usuario) {
            string sql;
            //= @"INSERT INTO Dispenser(DireccionMAC, Nombre) VALUES(@DireccionMAC, @Nombre); SELECT LAST_INSERT_ID()";

            if (dispenser.ID == 0)
            {
                sql = @"INSERT INTO Dispenser(DireccionMAC, Nombre) VALUES(@DireccionMAC, @Nombre);
                            SELECT LAST_INSERT_ID()";
                int insertedId = _conn.Query<int>(sql, dispenser).SingleOrDefault();
                dispenser.ID = insertedId;
            }
            else
            {
                sql = @"UPDATE Dispenser SET Nombre = @Nombre WHERE ID = @ID;";
                _conn.Execute(sql, dispenser);
            }

            return CreateRelation(usuario, dispenser);
        }

        public bool CreateRelation(Usuario usuario, Dispenser dispenser)
        {
            string sql = @"INSERT INTO UsuarioDispenser(UsuarioID, DispenserID) VALUES(@UsuarioID, @DispenserID)";
            int rowsAffected = _conn.Execute(sql, new { UsuarioID = usuario.ID, DispenserID = dispenser.ID });

            return (rowsAffected > 0);
        }
    }
}
