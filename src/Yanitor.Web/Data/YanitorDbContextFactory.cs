using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Yanitor.Web.Data;

public class YanitorDbContextFactory : IDesignTimeDbContextFactory<YanitorDbContext>
{
    public YanitorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<YanitorDbContext>();
        // Use same default as runtime if no connection string provided
        optionsBuilder.UseSqlite("Data Source=yanitor.db");
        return new YanitorDbContext(optionsBuilder.Options);
    }
}
