using DotNetEntranceExam.DTOs;
using DotNetEntranceExam.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotNetEntranceExam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: /api/auth/signup
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            try
            {
                var result = await _authService.SignUpAsync(request);
                if (result == null)
                    return BadRequest("Invalid input or email already exists");

                return Created("", result); // 201
            }
            catch(Exception ex)
            {
                return StatusCode(500);
            }

        }

        // POST: /api/auth/signin
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            try
            {
                var result = await _authService.SignInAsync(request);
                if (result == null)
                    return BadRequest("Invalid email or password");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        // POST: /api/auth/signout
        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                var userId = User.FindFirstValue("id");
                if (userId == null)
                    return Unauthorized();

                var success = await _authService.SignOutAsync(int.Parse(userId));
                if (!success)
                    return StatusCode(500, "Error while signing out");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        // POST: /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                if (result == null)
                    return NotFound("Refresh token not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }

        }

    }
}
