using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReservation
    {
        Task<bool> CreateReservationAsync(CreateReservationDto request);
        Task<bool> CancelReservationAsync(CancelReservationDto request);

    }
}
