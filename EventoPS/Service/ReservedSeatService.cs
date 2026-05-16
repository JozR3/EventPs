using Application.Dtos;
using Application.Interfaces;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventoPS.Service
{
    public class ReservedSeatService : IReservedSeatService
    {
        private readonly EventDbContext _context;

        public ReservedSeatService(EventDbContext context)
        {
            _context = context;
        }
        public async Task<List<SectorSeatsDto>> GetReservedSeatsAsync(int eventId)
        {
            return await _context.Sectors  
                .Where(s => s.EventId == eventId)
                .Select(s => new SectorSeatsDto
                {
                    SectorId = s.Id,
                    SectorName = s.Name,
                    Price = s.Price,
                    Seats = s.Seats.Where(seat => seat.Status == "Reserved")
                    .Select(seat => new SeatDto
                    {
                        Id = seat.Id,
                        SectorId = s.Id,
                        SectorName = s.Name,
                        SeatNumber = seat.SeatNumber,
                        RowIdentifier = seat.RowIdentifier,
                        Status = seat.Status
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}