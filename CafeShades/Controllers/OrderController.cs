using AutoMapper;
using Cafeshades.Models.Dtos;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

namespace CafeShades.Controllers
{
    // TODO : PATCH for Status Update
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<OrderStatus> _orderStatusRepo;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IMapper mapper,
                               IGenericRepository<Order> orderRepo,
                               IGenericRepository<OrderStatus> orderStatusRepo,
                               ILogger<OrderController> logger,
            IWebHostEnvironment env)
        {
            _mapper = mapper;
            _orderRepo = orderRepo;
            _orderStatusRepo = orderStatusRepo;
            _logger = logger;
            _env = env;
        }

        #region Order

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Order record;
            try
            {
                record = await _orderRepo.GetByIdAsync(id, order => order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Order");
                return BadRequest();
            }

            if (record == null) return NotFound();

            return Ok(_mapper.Map<OrderDto>(record));

        }

        // GET api/<OrderController>
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            IReadOnlyList<Order> record;
            try
            {
                record = await _orderRepo.ListAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Order");
                return BadRequest();
            }

            if (record.IsNullOrEmpty()) return NotFound();

            return Ok(_mapper.Map<IReadOnlyList<OrderDto>>(record));
        }

        // POST api/<OrderController>
        [HttpPost()]
        public async Task<IActionResult> AddOrder([FromBody] string orderName, [FromForm] IFormFile imageFile)
        {
            return Ok();
        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, [FromBody] string orderName, [FromForm] IFormFile imageFile)
        {
            return Ok();
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var record = await _orderRepo.GetByIdAsync(id);

                if (record == null) return NotFound();

                _orderRepo.Delete(record);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Order");
                return BadRequest();
            }
            return Ok();
        }

        #endregion

        #region OrderStatus

        //[HttpGet("/status/{id}")]
        //public async Task<IActionResult> GetOrderstatusById(int id)
        //{
        //    OrderStatus record;
        //    try
        //    {
        //        record = await _orderStatusRepo.GetByIdAsync(id, order => order.Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error Occured while Deleting Order");
        //        return BadRequest();
        //    }

        //    if (record == null) return NotFound();

        //    return Ok(record.StatusName);

        //}

        // GET api/<OrderController>

        [HttpGet("status")]
        public async Task<IActionResult> GetAllOrderStatus()
        {
            IReadOnlyList<OrderStatus> record;
            try
            {
                record = await _orderStatusRepo.ListAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while GetAll OrderStatus");
                return BadRequest();
            }

            if (record.IsNullOrEmpty()) return NotFound();
            //_mapper.Map<IReadOnlyList<OrderDto>>(record)
            return Ok(record);
        }

        // POST api/<OrderController>
        [HttpPost("status")]
        public async Task<IActionResult> AddOrderStatus([FromBody] string orderStatusName)
        {
            OrderStatus status = new OrderStatus
            {
                StatusName = orderStatusName
            };

            try
            {
                var allstatus = await _orderStatusRepo.ListAllAsync();
                foreach (var item in allstatus)
                    if (item.StatusName.Equals(orderStatusName))
                        return Conflict();

                _orderStatusRepo.Add(status);
                _orderStatusRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Adding OrderStatus");
                return BadRequest();
            }

            return Ok();
        }

        // PUT api/<OrderController>/5
        [HttpPut("status/{id}")]
        public async Task<IActionResult> PutOrderStatus(int id, [FromBody] string orderStatusName)
        {
            OrderStatus status = new OrderStatus
            {
                StatusName = orderStatusName
            };

            try
            {
                var statusRepo = await _orderStatusRepo.GetByIdAsync(id);

                if (statusRepo == null) return NotFound();

                _orderStatusRepo.Update(status);
                _orderStatusRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Updating OrderStatus");
                return BadRequest();
            }
            return Ok();
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("status/{id}")]
        public async Task<IActionResult> DeleteOrderStatus(int id)
        {
            try
            {
                var record = await _orderRepo.GetByIdAsync(id);

                if (record == null) return NotFound();

                _orderRepo.Delete(record);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Order");
                return BadRequest();
            }
            return Ok();
        }

        #endregion

    }
}
