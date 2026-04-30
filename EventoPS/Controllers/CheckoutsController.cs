using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EventoPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutsController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutsController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCheckoutDto seat)
        {
            var result = await _checkoutService.CreateCheckoutAsync(seat);

            if (!result)
                return BadRequest("No se pudo comprar el asiento");

            return Ok("Compra realizada correctamente");
        }
    }
}