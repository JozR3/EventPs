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
            if (request == null)
            {
                var null_request_audit_log = new Audit_Log
                {
                    Id = Guid.NewGuid(),
                    UserId = null,
                    Action = "RESERVE_FAILED",
                    EntityType = "Seat",
                    EntityId = "null",
                    Details = "Request nulo",
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(null_request_audit_log);
                await _context.SaveChangesAsync();

                return false;
            }

            var audit_log = new Audit_Log
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = "RESERVE_ATTEMPT",
                EntityType = "Seat",
                EntityId = request.SeatId.ToString(),
                Details = "Intento de reserva",
                CreatedAt = DateTime.UtcNow
            };

            if (request.SeatId == Guid.Empty)
            {
                audit_log.Action = "RESERVE_FAILED";
                audit_log.Details = "SeatId vacio";

                _context.AuditLogs.Add(audit_log);
                await _context.SaveChangesAsync();

                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                
                var seat = await _context.Seats
                    .FromSqlRaw(
                        "SELECT * FROM Seats WITH (UPDLOCK, ROWLOCK) WHERE Id = {0}",
                        request.SeatId)
                    .FirstOrDefaultAsync();
                if (seat == null)
                {
                    audit_log.Action = "RESERVE_FAILED";
                    audit_log.Details = "Asiento no existente";
                    audit_log.CreatedAt = DateTime.UtcNow;

                    _context.AuditLogs.Add(audit_log);
                    await _context.SaveChangesAsync();

                    return false;
                }
                if (seat.Status != "Available")
                {
                    audit_log.Action = "RESERVE_FAILED";
                    audit_log.Details = "Asiento no disponible";
                    audit_log.CreatedAt = DateTime.UtcNow;

                    _context.AuditLogs.Add(audit_log);
                    await _context.SaveChangesAsync();

                    return false;
                }
                
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    audit_log.Action = "RESERVE_FAILED";
                    audit_log.Details = "Usuario no existente";
                    audit_log.CreatedAt = DateTime.UtcNow;

                    _context.AuditLogs.Add(audit_log);
                    await _context.SaveChangesAsync();

                    return false;
                }

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    SeatId = request.SeatId,
                    Status = "Pending",
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };

                seat.Status = "Reserved";

                audit_log.Action = "RESERVE_SUCCESS";
                audit_log.Details = "Reserva realizada correctamente";
                audit_log.CreatedAt = DateTime.UtcNow;

                _context.Seats.Update(seat);
                _context.Reservations.Add(reservation);
                _context.AuditLogs.Add(audit_log);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                try { await transaction.RollbackAsync(); } catch { }

                audit_log.Action = "RESERVE_FAILED";
                audit_log.Details = $"DB Exception: {ex.Message}";
                audit_log.CreatedAt = DateTime.UtcNow;

                _context.AuditLogs.Add(audit_log);
                await _context.SaveChangesAsync();

                return false;
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }

                audit_log.Action = "RESERVE_FAILED";
                audit_log.Details = $"Exception: {ex.Message}";
                audit_log.CreatedAt = DateTime.UtcNow;

                _context.AuditLogs.Add(audit_log);
                await _context.SaveChangesAsync();

                return false;
            }
        }

        public async Task<bool> CancelReservationAsync(CancelReservationDto request)
        {
            if (request == null || request.SeatId == Guid.Empty)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.SeatId == request.SeatId && r.UserId == request.UserId && r.Status == "Pending");

                if (reservation == null)
                    return false;

                reservation.Status = "Expired";
                _context.Reservations.Update(reservation);

                var seat = await _context.Seats.FindAsync(request.SeatId);
                if (seat != null)
                {
                    seat.Status = "Available";
                    _context.Seats.Update(seat);
                }

                _context.AuditLogs.Add(new Audit_Log
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Action = "RESERVE_CANCELLED",
                    EntityType = "Reservation",
                    EntityId = request.SeatId.ToString(),
                    Details = "Reserva cancelada por el usuario",
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