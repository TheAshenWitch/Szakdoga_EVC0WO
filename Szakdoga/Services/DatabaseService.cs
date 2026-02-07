using Szakdoga.Models;
using System.Collections.Generic;
using System.Linq;

namespace Szakdoga.Services
{
    public class DatabaseService
    {
        // Példa: Új Sheet mentése
        public void AddSheet(Sheet sheet)
        {
            // Így már működik a sima new AppDbContext()!
            using (var db = new AppDbContext())
            {
                db.Sheets.Add(sheet);
                db.SaveChanges();
            }
        }

        // Példa: Lista lekérése
        public List<Sheet> GetAllSheets()
        {
            using (var db = new AppDbContext())
            {
                return db.Sheets.ToList();
            }
        }
    }
}