using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventoPS.Service
{
    public class ExpiredReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredReservationCleanupService> _logger;

        public ExpiredReservationCleanupService(IServiceProvider serviceProvider, ILogger<ExpiredReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5)); // Frequency

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();

                    var now = DateTime.UtcNow;

                    var reservations = await context.Reservations
                        .Where(r => r.Status == "Pending" && r.ExpiresAt <= now)
                        .ToListAsync();

                    _logger.LogInformation("Found {count} expired reservations", reservations.Count);

                    foreach (var reservation in reservations)
                    {
                        reservation.Status = "Expired";

                        var seat = await context.Seats.FindAsync(reservation.SeatId);
                        if (seat != null)
                        {
                            seat.Status = "Available";
                        }

                        context.AuditLogs.Add(new Audit_Log
                        {
                            Id = Guid.NewGuid(),
                            UserId = reservation.UserId,
                            Action = "RESERVE_EXPIRED",
                            EntityType = "Seat",
                            EntityId = reservation.SeatId.ToString(),
                            Details = "reserva expirada",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while cleaning up expired reservations");
                }
            }
        }
    }
}