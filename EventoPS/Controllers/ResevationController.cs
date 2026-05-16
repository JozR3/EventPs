using Application.Dtos;
using Application.Interfaces;
using EventoPS.Service;
using Microsoft.AspNetCore.Mvc;

namespace EventoPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservation _reservationService;

        public ReservationController(IReservation reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto request)
        {
            var result = await _reservationService.CreateReservationAsync(request);

            if (!result)
                return Conflict(new
                {
                    message = "Seat already reserved"
                });

            return Created("", "Reserva creada correctamente");
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelReservationDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Request inválido" });

            var result = await _reservationService.CancelReservationAsync(request);

            if (!result)
                return BadRequest(new { message = "No se pudo cancelar la reserva" });

            return Ok(new { message = "Reserva cancelada y asiento liberado" });
        }
    }

}
