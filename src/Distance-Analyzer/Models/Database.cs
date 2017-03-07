using Microsoft.EntityFrameworkCore;

namespace Distance_Analyzer.Models
{
    public class Database : DbContext
    {
        public DbSet<Node> Nodes { get; set; }

        public Database(DbContextOptions options) : base(options)
        {
            // Nothing to do here
        }

        public static void Init(Database db)
        {
            // Check if the table exists, if not, create

            //db.Database.ExecuteSqlCommand("create table if not exists Nodes (id text, Date datetime, Description text, Amount decimal, IsBalance boolean)");

            // Seed database(?)

            //if (!db.Nodes.Any())
            //{
            //    db.Nodes.Add(new Node { });

            //    db.SaveChanges();
            //}
        }
    }
}