using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventoPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePagoDto pago)
        {
            if (pago == null)
                return BadRequest("Request inválido");

            var result = await _pagoService.CreatePagoAsync(pago);

            if (!result)
                return BadRequest("Pago inválido o no permitido");

            return Ok("Pago realizado correctamente");
        }
    }
}