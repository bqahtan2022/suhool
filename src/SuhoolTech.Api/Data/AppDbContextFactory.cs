using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SuhoolTech.Api.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=SuhoolTechDesignTimeOnly;User Id=design;Password=DesignTimeOnly123!;TrustServerCertificate=True")
            .Options;
        return new AppDbContext(options);
    }
}
