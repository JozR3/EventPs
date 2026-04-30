using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CreateReservationDto
    {
        public int UserId { get; set; }
        public Guid SeatId { get; set; }
    }
}
