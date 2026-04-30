using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventoPS.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/reservedseats")]
    public class ReservedSeatsController  : ControllerBase
    {
        private readonly IReservedSeatService _reservedSeatService;

        public ReservedSeatsController(IReservedSeatService reservedSeatService)
        {
            _reservedSeatService = reservedSeatService;
        }
        /*
        [HttpGet]
        public async Task<IActionResult> GetReservedSeats()
        {
            var result = await _reservedSeatService.GetReservedSeatsAsync();
            return Ok(result); 

        }
        */
        [HttpGet]
        public async Task<IActionResult> GetReservedSeats(int eventId)
        {
            try
            {
                var result = await _reservedSeatService.GetReservedSeatsAsync(eventId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
