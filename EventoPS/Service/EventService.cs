using Application.Dtos;
using Application.Interfaces;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventoPS.Service
{
    public class EventService : IEventService
    {
        private readonly EventDbContext _context;

        public EventService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventDto>> GetEventsAsync()
        {
            return await _context.Events
                .Where(e => e.Status == "Active")
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Venue = e.Venue,
                    Status = e.Status,
                    Sectors = e.Sectors.Select(s => new SectorDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Price = s.Price,
                        TotalSeats = s.Seats.Count(),
                        AvailableSeats = s.Seats.Count(seat => seat.Status == "Available")
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
