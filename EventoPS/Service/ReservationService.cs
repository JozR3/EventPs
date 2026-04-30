using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventoPS.Service
{
    public class ReservationService : IReservation
    {
        private readonly EventDbContext _context;

        public ReservationService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateReservationAsync(CreateReservationDto request)
        {
            if (request == null) return false;
            if (request.SeatId == Guid.Empty) return false;
            // Buscar asiento
            var seat = await _context.Seats.FindAsync(request.SeatId);
            if (seat == null)
                return false;
            // Verificar usuario existe (evita violación FK)
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return false;

            var exists = await _context.Reservations
                .AnyAsync(r => r.SeatId == request.SeatId && (r.Status == "Pending" || r.Status == "Paid"));
            if (exists)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    SeatId = request.SeatId,
                    Status = "Pending",
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };

                _context.Reservations.Add(reservation);

                seat.Status = "Reserved";
                _context.Seats.Update(seat);

                _context.AuditLogs.Add(new Audit_Log
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Action = "RESERVE_ATTEMPT",
                    EntityType = "Seat",
                    EntityId = request.SeatId.ToString(),
                    Details = "Intento de reserva",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateException)
            {
                // Log si deseas; por ahora devolvemos false para que el controlador informe error
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