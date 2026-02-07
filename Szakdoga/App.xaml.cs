using System.Configuration;
using System.Data;
using System.Windows;
using Szakdoga.Models;
using Szakdoga.Services;

namespace Szakdoga
{
    public partial class App : Application
    {
        // Ez a globális elérés
        public static DatabaseService DB { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Service létrehozása
            DB = new DatabaseService();

            // 2. Adatbázis biztosítása (hogy tuti létrejöjjön a fájl, ha nincs)
            // Ez opcionális, mert az Update-Database parancs már megcsinálta,
            // de ha átviszed másik gépre a programot, ez automatikusan létrehozza.
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
