using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventoPS.Controllers
{
    [Route("api/events/{eventId}/seats")]
    [ApiController]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSeats(int eventId)
        {
            var result = await _seatService.GetSeatsByEventAsync(eventId);
            return Ok(result);
        }
    }
}
