using IBanKing.Models;
using IBanKing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IBanKing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("changeCurrency")]
        public async Task<IActionResult> ChangeCurrency([FromBody] ChangeAccountCurrencyRequest request)
        {
            var success = await _accountService.ChangeAccountCurrencyAsync(request.AccountId, request.NewCurrency);
            return success ? Ok() : BadRequest();
        }
    }
}