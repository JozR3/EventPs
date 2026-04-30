using Application.Dtos;
using Application.Interfaces;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventoPS.Service
{
    public class SeatService : ISeatService
    {
        private readonly EventDbContext _context;

        public SeatService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<List<SectorSeatsDto>> GetSeatsByEventAsync(int eventId)
        {
            return await _context.Sectors  ///MAPEO
                .Where(s => s.EventId == eventId)
                .Select(s => new SectorSeatsDto
                {
                    SectorId = s.Id,
                    SectorName = s.Name,

                    Seats = s.Seats.Select(seat => new SeatDto
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
