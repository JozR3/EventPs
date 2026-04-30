using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Persistence
{
    public class EventDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Audit_Log> AuditLogs { get; set; }

        public EventDbContext(DbContextOptions<EventDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relaciones
            modelBuilder.Entity<Sector>()
                .HasOne(s => s.Event)
                .WithMany(e => e.Sectors)
                .HasForeignKey(s => s.EventId);

            modelBuilder.Entity<Event>()
            .Property(e => e.EventDate)
            .HasColumnType("datetime2(0)");
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Sector)
                .WithMany(s => s.Seats)
                .HasForeignKey(s => s.SectorId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SeatId);

            modelBuilder.Entity<Audit_Log>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Concurrency
            modelBuilder.Entity<Seat>()
                .Property(s => s.Version)
                .IsConcurrencyToken();

            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.SectorId, s.SeatNumber })
                .IsUnique();
            SeedData(modelBuilder);

        }
        private void SeedData(ModelBuilder modelBuilder)
        {
            var eventId = 1;
            var user = new User
            {
                Id = 1,
                Name = "Usuario Invitado",
                Email = "guest@example.com",
                PasswordHash = "hashed_password_placeholder"
            };
            modelBuilder.Entity<User>().HasData(user);

            modelBuilder.Entity<Event>().HasData(new Event
            {
                Id = eventId,
                Name = "Concierto Rock",
                EventDate = new DateTime(2026,1,1,21,30,0),
                Venue = "Estadio Monumental",
                Status = "Active"
            });

            // Sectores
            var sector1 = new Sector
            {
                Id = 1,
                EventId = eventId,
                Name = "VIP",
                Price = 15000,
                Capacity = 50
            };

            var sector2 = new Sector
            {
                Id = 2,
                EventId = eventId,
                Name = "General",
                Price = 8000,
                Capacity = 50
            };

            modelBuilder.Entity<Sector>().HasData(sector1, sector2);

            // Butacas
            var seats = new List<Seat>();

            for (int sectorId = 1; sectorId <= 2; sectorId++)
            {
                for (int i = 1; i <= 50; i++)
                {
                    seats.Add(new Seat
                    {
                        Id = Guid.Parse($"00000000-0000-0000-0000-{sectorId:D2}{i:D10}"),
                        SectorId = sectorId,
                        RowIdentifier = "A",
                        SeatNumber = i,
                        Status = "Available",
                        Version = new byte[] { 0 }
                    });
                }
            }

            modelBuilder.Entity<Seat>().HasData(seats);
        }
    }
}
