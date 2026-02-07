using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga.Models
{
    public class AppDbContext : DbContext
    {
        // 1. Konstruktor: Most üresen hagyjuk, nem kell paraméter
        public AppDbContext()
        {
            // Üres maradhat
        }

        public DbSet<Sheet> Sheets { get; set; }

        // 2. Itt mondjuk meg, hogy SQLite-ot használunk és mi legyen a fájl neve
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // A "Data Source=szakdoga.db" jelenti, hogy a program mappájában 
                // jön létre a szakdoga.db fájl.
                DotNetEnv.Env.Load();
                string connectionString = DotNetEnv.Env.GetString("DB_CONNECTION_STRING");
                optionsBuilder.UseSqlServer(connectionString);
                Console.WriteLine(connectionString);
            }
        }
    }
}