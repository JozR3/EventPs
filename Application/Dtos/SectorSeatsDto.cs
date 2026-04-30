using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class SectorSeatsDto    ///Agrupado por sector
    {
        public int SectorId { get; set; }
        public string SectorName { get; set; }
        public List<SeatDto> Seats { get; set; }
    }
}
