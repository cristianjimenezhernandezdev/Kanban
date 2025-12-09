using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class UserService
    {
        /// <summary>
        /// Obté tots els usuaris
        /// </summary>
        /// <returns></returns>
        public List<User> GetAll()
        {
            var result = new List<User>();

            using (var ctx = DbContext.GetInstance())
            {
                var query = "SELECT * FROM Users";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new User
                            {
                                Id = Convert.ToInt32(reader["id"].ToString()),
                                Name = reader["Name"].ToString(),
                                LastName = reader["Lastname"].ToString(),
                                Birthday = Convert.ToDateTime(reader["Birthday"]),
                            });
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Obté les dades de l'usuari indicat
        /// </summary>
        /// <param name="Id">Identificador d'usuari</param>
        /// <returns>Dades de l'Usuari</returns>
        public User GetById(int Id)
        {
            User user = null;

            using (var ctx = DbContext.GetInstance())
            {
                var query = "SELECT * FROM Users WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("Id", Id));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user = new User()
                            {
                                Id = Convert.ToInt32(reader["id"].ToString()),
                                Name = reader["Name"].ToString(),
                                LastName = reader["Lastname"].ToString(),
                                Birthday = Convert.ToDateTime(reader["Birthday"]),
                            };
                        }
                    }
                }
            }
            return user;
        }

        /// <summary>
        /// Afegeix un nou usuari a la base de dades
        /// </summary>
        /// <param name="user">Entitat usuari</param>
        /// <returns>Id de l'usuari afegit</returns>
        /// li passes el user i l'afegeixes amb el command a la part de insert @name, @lastname
        public User Add(User user)
        {
            using (var ctx = DbContext.GetInstance())
            {
                string query = "INSERT INTO Users (name, lastname, birthday) VALUES (@name, @lastname, @birthday)";
                using (var command = new System.Data.SQLite.SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("name", user.Name));
                    command.Parameters.Add(new SQLiteParameter("lastname", user.LastName));
                    command.Parameters.Add(new SQLiteParameter("birthday", user.Birthday));

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";

                    user.Id = (Int64) command.ExecuteScalar();
                }
            }

            return user;
        }

        /// <summary>
        /// Actualitza un usuari
        /// </summary>
        /// <param name="user">Entitat usuari que es vol modificar</param>
        /// <returns>Files afectades</returns>
        public int Update(User user)
        {
            int rows_affected = 0;
            using (var ctx = DbContext.GetInstance())
            {
                string query = "UPDATE Users SET name = @name, lastname = @lastname, birthday = @birthday WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("name", user.Name));
                    command.Parameters.Add(new SQLiteParameter("lastname", user.LastName));
                    command.Parameters.Add(new SQLiteParameter("birthday", user.Birthday));
                    command.Parameters.Add(new SQLiteParameter("Id", user.Id));

                    rows_affected = command.ExecuteNonQuery();
                }
            }

            return rows_affected;
        }

        /// <summary>
        /// Elimina un usuari
        /// </summary>
        /// <param name="Id">Codi d'usuari que es vol eliminar</param>
        /// <returns>Files afectades</returns>
        public int Delete(int Id)
        {
            int rows_affected = 0;
            using (var ctx = DbContext.GetInstance())
            {
                string query = "DELETE FROM Users WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("Id", Id));
                    rows_affected = command.ExecuteNonQuery();
                }
            }

            return rows_affected;
        }
    }
}
