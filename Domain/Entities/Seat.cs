using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public int SectorId { get; set; }
        public string RowIdentifier { get; set; }
        public int SeatNumber { get; set; }
        public string Status { get; set; } // Available, Reserved, Sold
        [Timestamp]
        public byte[] Version { get; set; }

        public Sector Sector { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
