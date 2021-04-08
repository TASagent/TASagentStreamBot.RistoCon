using Microsoft.EntityFrameworkCore;

namespace TASagentTwitchBot.RistoCon.Database;

//Create the database 
public class DatabaseContext : Core.Database.BaseDatabaseContext
{

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={BGC.IO.DataManagement.PathForDataFile("Config", "data.sqlite")}");
    }
}