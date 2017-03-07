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

            db.Database.ExecuteSqlCommand(@"
                CREATE TABLE `Nodes` (
	                `Id`	TEXT,
	                `Description`	TEXT,
	                `Raw`	TEXT,
	                `Address`	TEXT,
	                `Latitude`	DECIMAL,
	                `Longitude`	DECIMAL,
	                `TagsList`	TEXT,
	                `Is_Super_Node`	BOOLEAN,
	                `MappingsList`	TEXT,
                    PRIMARY KEY(`Id`)
                ); ");

            // Seed database(?)

            //if (!db.Nodes.Any())
            //{
            //    db.Nodes.Add(new Node { });

            //    db.SaveChanges();
            //}
        }
    }
}