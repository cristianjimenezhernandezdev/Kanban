using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApplicationAPIDemo.Persistence
{
    public class DbContext
    {
        public static SQLiteConnection GetInstance()
        {
            var configuration = GetConfiguration();

            //obtenim la cadena de connexió del fitxer de configuració
            string connectionString = configuration.GetSection("ConnectionStrings").GetSection("SQLite").Value;

            var db = new SQLiteConnection(
               string.Format(connectionString)
            );

            db.Open();

            return db;
        }

        /// <summary>
        /// Per agafar les dades del fitxer de configuració appsettings.json
        /// </summary>
        /// <returns></returns>
        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
    }
}
