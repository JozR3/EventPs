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
                return BadRequest("No se pudo reservar el asiento");

            return Ok("Reserva creada correctamente");
        }
    }
}
