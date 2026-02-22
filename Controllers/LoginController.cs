using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace Prism.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // מאפשר לכולם לגשת כדי שיוכלו להתחבר
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto request)
        {
            try
            {
                // ה-AuthService בודק סיסמה, יוזר, ומייצר טוקן עם Role של Admin
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                // אם הסיסמה שגויה או המשתמש לא נמצא
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // שגיאה כללית במערכת
                return StatusCode(500, new { message = "שגיאה פנימית בשרת" });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = "טוקן לא תקין או פג תוקף" });
            }
        }
    }
}