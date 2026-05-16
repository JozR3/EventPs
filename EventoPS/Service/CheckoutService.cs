using Application.Dtos;
using Application.Interfaces;
using Azure.Core;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventoPS.Service
{
    public class CheckoutService : ICheckoutService
    {
        private readonly EventDbContext _context;

        public CheckoutService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateCheckoutAsync(CreateCheckoutDto request)
        {
            if(request == null) return false;
            if (request.SeatId == Guid.Empty) return false;
            // Buscar asiento
            var seat = await _context.Seats.FindAsync(request.SeatId);
            if (seat == null)
                return false;
            // Verificar usuario existe (evita violación FK)
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return false;
            // Buscar Reserva.
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.SeatId == request.SeatId && r.Status == "Pending");
            if (reservation == null)
                return false;


            var exists = await _context.Seats
                .AnyAsync(r => r.Id == request.SeatId && (r.Status == "Available" || r.Status == "Paid"));
            if (exists)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                seat.Status = "Sold";

                reservation.Status = "Paid";

                _context.AuditLogs.Add(new Audit_Log
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Action = "RESERVE_SUCCESS",
                    EntityType = "Seat",
                    EntityId = request.SeatId.ToString(),
                    Details = "reserva exitosa",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateException)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return false;
            }
            catch (Exception)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return false;
            }
        }
    }
}
