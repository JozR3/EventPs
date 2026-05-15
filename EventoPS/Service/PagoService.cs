using Application.Dtos;
using Application.Interfaces;
using Azure.Core;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventoPS.Service
{
    public class PagoService : IPagoService
    {
        private readonly EventDbContext _context;

        public PagoService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreatePagoAsync(CreatePagoDto request)
        {
            if (request == null) return false;
            if (request.SeatId == Guid.Empty) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                // Otro fetch para asegurar que el asiento no cambió de estado entre la primera consulta y ahora.
                var seat = await _context.Seats.FindAsync(request.SeatId);
                if (seat == null || seat.Status != "Reserved")
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                seat.Status = "Sold";

                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.SeatId == request.SeatId && r.UserId == request.UserId && r.Status == "Pending");
                if (reservation == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                reservation.Status = "Paid";

                _context.AuditLogs.Add(new Audit_Log
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Action = "PAYMENT_SUCCESS",
                    EntityType = "Seat",
                    EntityId = request.SeatId.ToString(),
                    Details = "pago realizado correctamente",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                try { await transaction.RollbackAsync(); } catch { }
                return false;
            }
        }
    }
}
