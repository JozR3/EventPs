using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class SeatDto
    {
        public Guid Id { get; set; }
        public int SectorId { get; set; }
        public string SectorName { get; set; }
        public int SeatNumber { get; set; }
        public string RowIdentifier { get; set; }
        public string Status { get; set; }
    }
}
