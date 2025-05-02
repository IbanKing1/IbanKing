using IBanKing.Data;
using IBanKing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class InactivityCheckerService : BackgroundService
{
    private readonly ILogger<InactivityCheckerService> _logger;
    private readonly IServiceProvider _services;

    public InactivityCheckerService(
        ILogger<InactivityCheckerService> logger,
        IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var targetTime = now.Date.AddHours(10).AddMinutes(30);

            if (now > targetTime)
                targetTime = targetTime.AddDays(1);

            var delay = targetTime - now;
            _logger.LogInformation($"Next check at: {targetTime}");

            await Task.Delay(delay, stoppingToken);

            try
            {
                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var today = DateTime.UtcNow.Date;
                    var thresholdDate = today.AddDays(-30);

                    var inactiveUsers = await dbContext.Users
                        .Where(u => u.Role == "Client" &&
                                    u.LastLog.Date == thresholdDate)
                        .ToListAsync();

                    foreach (var user in inactiveUsers)
                    {
                        await emailService.SendInactivityEmailAsync(user.Email, user.Name, user.LastLog);
                        _logger.LogInformation($"Email sent for {user.Email} - last login was exactly on {thresholdDate:dd.MM.yyyy}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inactivity check");
            }
        }
    }
}