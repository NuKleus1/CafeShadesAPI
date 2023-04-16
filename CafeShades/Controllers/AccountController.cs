using AutoMapper;
using Cafeshades.Models.Dtos;
using CafeShades.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CafeShades.Controllers
{
    // TODO : Change Responses into ApiResponse Format
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<User> _userRepo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IMapper mapper, IWebHostEnvironment env, IGenericRepository<User> userRepo, ILogger<AccountController> logger)
        {
            _mapper = mapper;
            _env = env;
            _userRepo = userRepo;
            _logger = logger;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserbyPhoneNumber([FromBody] string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber))
                return BadRequest(new ApiResponse("Phone Number not Found"));

            User user;
            try
            {
                user = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, phoneNumber));

                if (user == null)
                    return NotFound(new ApiResponse("User Not Found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }

            return Ok(new { responseStatus = true, user = _mapper.Map<UserDto>(user) });
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserbyId(int id)
        {
            User user;
            try
            {
                user = await _userRepo.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new ApiResponse("User Not Found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }

            return Ok(new { responseStatus = true, user = _mapper.Map<UserDto>(user) });
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login([FromBody] string mobileNumber)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, mobileNumber));

                if (user == null)
                    return Unauthorized(new ApiResponse("User Not Found"));

                if (!user.isLoggedIn)
                    return Conflict(new ApiResponse("Already Logged In"));

                user.isLoggedIn = true;
                user.LastLoginAt = DateTime.UtcNow;

                _userRepo.SaveChanges();

                return Ok(new { responseStatus = true, user = user });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while logging in a User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromBody] string mobileNumber)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, mobileNumber));

                if (user == null)
                    return Unauthorized(new ApiResponse("User Not Found"));

                if (user.isLoggedIn)
                    return Conflict(new ApiResponse("User Not Logged In"));

                user.isLoggedIn = false;

                _userRepo.SaveChanges();

                return Ok(new { responseStatus = true, responseMessage = "User logged out" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Logging out a User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp([FromBody] UserDto userRequest)
        {

            var user = _mapper.Map<User>(userRequest);
            try
            {
                var checkUser = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, userRequest.mobileNumber));

                if (checkUser != null)
                    return Conflict(new ApiResponse("Phone Number is already registerd !"));

                user.isLoggedIn = true;
                user.LastLoginAt = DateTime.UtcNow;
                _userRepo.Add(user);
                _userRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sign up");
                return BadRequest(new ApiResponse("Error Occurred"));
            }

            return Ok(new { responseStatus = true, user = _mapper.Map<UserDto>(user) });
        }
    }
}