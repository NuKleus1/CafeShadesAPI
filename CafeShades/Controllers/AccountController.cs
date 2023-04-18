using AutoMapper;
using Cafeshades.Models.Dtos;
using Cafeshades.Models.Dtos.Request;
using CafeShades.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
        private readonly IGenericRepository<UserToken> _userTokenRepo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IMapper mapper, IWebHostEnvironment env, IGenericRepository<User> userRepo, ILogger<AccountController> logger, IGenericRepository<UserToken> userTokenRepo)
        {
            _mapper = mapper;
            _env = env;
            _userRepo = userRepo;
            _logger = logger;
            _userTokenRepo = userTokenRepo;
        }


        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUsers() 
        {
            IReadOnlyList<User> user;
            try
            {
                user = await _userRepo.ListAllAsync();
                if (user.IsNullOrEmpty())
                    return NotFound(new ApiResponse("User Not Found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving Users ");
                return BadRequest(new ApiResponse("Error Occurred"));
            }

            return Ok(new { responseStatus = true, user = user });
        }
        [HttpPost("user")]
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

            return Ok(new { responseStatus = true, user = _mapper.Map<User>(user) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string mobileNumber)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, mobileNumber));

                if (user == null)
                    return Unauthorized(new ApiResponse("User Not Found"));

                if (user.isLoggedIn)
                    return Conflict(new ApiResponse("Already Logged In"));

                user.isLoggedIn = true;
                user.LastLoginAt = DateTime.UtcNow;

                _userRepo.SaveChanges();

                return Ok(new { responseStatus = true, resonseMessage = "Login successfull" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while logging in a User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }
        }

        [HttpGet("logout/{id}")]
        public async Task<IActionResult> Logout(int id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id);

                if (user == null)
                    return Unauthorized(new ApiResponse("User Not Found"));

                if (!user.isLoggedIn)
                    return Conflict(new ApiResponse("User Not Logged In"));

                user.isLoggedIn = false;

                _userRepo.SaveChanges();

                return Ok(new { responseStatus = true, responseMessage = "Log out successfull" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Logging out a User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp([FromBody] UserRequest userRequest)
        {
            User user = new User
            {
                Name = userRequest.name,
                BuildingName = userRequest.buildingName,
                FloorNumber = userRequest.floorNumber,
                Landmark = userRequest.landmark,
                MobileNumber = userRequest.mobileNumber,
                OfficeNumber = userRequest.officeNumber,
            };

            try
            {
                var checkUser = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, userRequest.mobileNumber));

                if (checkUser != null)
                    return Conflict(new ApiResponse("Phone Number is already registerd !"));

                _userRepo.Add(user);
                user.isLoggedIn = true;
                user.LastLoginAt = DateTime.UtcNow;
                _userRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sign up");
                return BadRequest(new ApiResponse("Error Occurred"));
            }

            return Ok(new { responseStatus = true, user =_mapper.Map<UserDto>(user) });
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UserRequest userRequest)
        {
            User user = new User
            {
                Id = id,
                Name = userRequest.name,
                BuildingName = userRequest.buildingName,
                FloorNumber = userRequest.floorNumber,
                Landmark = userRequest.landmark,
                MobileNumber = userRequest.mobileNumber,
                OfficeNumber = userRequest.officeNumber,
            };

            try
            {
                var checkUser = await _userRepo.GetByIdAsync(user => string.Equals(user.MobileNumber, userRequest.mobileNumber));

                if (checkUser == null)
                    return NotFound(new ApiResponse("User Not Found !"));

                checkUser.Name = userRequest.name;
                checkUser.BuildingName = userRequest.buildingName;
                checkUser.FloorNumber = userRequest.floorNumber;
                checkUser.Landmark = userRequest.landmark;
                checkUser.MobileNumber = userRequest.mobileNumber;
                checkUser.OfficeNumber = userRequest.officeNumber;

                _userRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sign up");
                return BadRequest(new ApiResponse("Error Occurred"));
            }

            return Ok(new { responseStatus = true,responseMessage = "User profile Updated successfully!" });
        }


        [HttpPost("user/token/{id}")]
        public async Task<IActionResult> AddFcmToken(int id, [FromBody] string fcmToken)
        {
            User user;
            try
            {
                user = await _userRepo.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new ApiResponse("User Not Found"));

                var userToken = await _userTokenRepo.GetByIdAsync(ut => ut.UserId == id);

                if (userToken != null)
                {
                    userToken.FcmToken = fcmToken;
                }
                else
                {
                    UserToken newUserToken = new UserToken
                    {
                        UserId = id,
                        FcmToken = fcmToken
                    };

                    _userTokenRepo.Add(newUserToken);
                }
                _userTokenRepo.SaveChanges();

                return Ok(new { responseStatus = true, responseMessage = "Fcm Token Added !" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving User");
                return BadRequest(new ApiResponse("Error Occurred"));
            }
        }
    }
}