using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Application.Services;
using CodeScanner.Infrastructure.Persistence;
using CodeScanner.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeScanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=.;Initial Catalog=BarCodeSannerV3;Integrated Security=True;TrustServerCertificate=True";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<ISerialPortService, SerialPortService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITestingService, TestingService>();
        services.AddScoped<ILineService, LineService>();

        return services;
    }
}
