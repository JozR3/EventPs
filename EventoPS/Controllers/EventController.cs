using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
namespace EventoPS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase 
    {
        private readonly IEventService _service;
        public EventsController(IEventService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var result = await _service.GetEventsAsync();
            return Ok(result);
        }
    }

}
