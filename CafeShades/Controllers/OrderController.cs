using AutoMapper;
using Cafeshades.Models.Dtos;
using CafeShades.Models;
using CafeShades.Models.Request;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<OrderStatus> _orderStatusRepo;
        private readonly ILogger<OrderController> _logger;
        private readonly StoreDbContext _context;

        public OrderController(IMapper mapper,
                               IGenericRepository<Order> orderRepo,
                               IGenericRepository<OrderStatus> orderStatusRepo,
                               ILogger<OrderController> logger,
            IWebHostEnvironment env,
            IGenericRepository<Product> productRepo,
            IGenericRepository<User> userRepo,
            StoreDbContext context)
        {
            _mapper = mapper;
            _orderRepo = orderRepo;
            _orderStatusRepo = orderStatusRepo;
            _logger = logger;
            _env = env;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _context = context;
        }

        #region Order

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Order record;
            try
            {
                record = await _orderRepo.GetByIdAsync(id, new List<Expression<Func<Order, object>>>
                {
                    or => or.OrderStatus,
                    or => or.OrderItems,
                });

                if (record == null)
                    return NotFound(new ApiResponse("Order Not Found!"));

                if (record != null && record.OrderItems != null)
                {
                    foreach (var item in record.OrderItems)
                    {
                        await _context.Entry(item).Reference(oi => oi.Product).LoadAsync();
                    }
                }


                

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }


            return Ok(new { responseStatus = true, order = _mapper.Map<OrderDto>(record) });

        }

        // GET api/<OrderController>
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            IReadOnlyList<Order> record;
            try
            {
                record = await _orderRepo.ListAllAsync(new List<Expression<Func<Order, object>>>
                {
                    or => or.OrderStatus,
                    or => or.OrderItems,
                });

                if (record == null)
                    return NotFound(new ApiResponse("Orders Not Found!"));

                foreach (var item in record)
                    foreach (var orderItem in item.OrderItems)
                        await _context.Entry(orderItem).Reference(oi => oi.Product).LoadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving Orders");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            if (record.IsNullOrEmpty()) return NotFound(new ApiResponse("Orders Not Found!"));

            return Ok(new { responseStatus = true, orderList = _mapper.Map<IReadOnlyList<OrderDto>>(record) });
        }


        // TODO : Check This API
        // POST api/<OrderController>
        [HttpPost()]
        public async Task<IActionResult> AddOrder([FromBody] OrderRequest orderRequest)
        {
            try
            {
                var status = await _orderStatusRepo.GetByIdAsync(orderRequest.OrderStatusId);
                if (status == null)
                    return BadRequest(new ApiResponse("Status does not Exists !"));

                User user = await _userRepo.GetByIdAsync(orderRequest.UserId);
                if (user == null)
                    return BadRequest(new ApiResponse("User does not Exists !"));

                foreach (var item in orderRequest.OrderItems)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product == null)
                        return BadRequest(new ApiResponse("Product : " + product.Name + " Not Found!"));
                    if (! (product.Quantity >= item.Quantity))
                        return BadRequest(new ApiResponse("Reduce Quantity for Product : " + product.Name));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while adding Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            Order newOrder = new Order
            {
                OrderStatusId = orderRequest.OrderStatusId,
                UserId = orderRequest.UserId,
                CreatedAt = DateTime.UtcNow,
                Date = DateTime.UtcNow,
                TotalAmount = orderRequest.TotalAmount,
            };

            newOrder.OrderItems = new List<OrderItem>();

            foreach (var item in orderRequest.OrderItems)
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });


            try
            {
                _orderRepo.Add(newOrder);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Deleting Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            return Ok(new { responseStatus = true, responseMessage = "Order submitted Successfully" });
        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, [FromBody] OrderRequest orderRequest)
        {
            var oldOrder = new Order();

            try
            {
                oldOrder = await _orderRepo.GetByIdAsync(id);

                if (oldOrder != null)
                    return NotFound(new ApiResponse("Order Not Found!"));

                var status = await _orderStatusRepo.GetByIdAsync(orderRequest.OrderStatusId);
                
                if (status == null)
                    return BadRequest(new ApiResponse("Status does not Exists !"));

                User user = await _userRepo.GetByIdAsync(orderRequest.UserId);
                
                if (user == null)
                    return BadRequest(new ApiResponse("User does not Exists !"));

                foreach (var item in orderRequest.OrderItems)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product == null)
                        return BadRequest(new ApiResponse("Product : " + product.Name + " Not Found!"));
                    if (!(product.Quantity >= item.Quantity))
                        return BadRequest(new ApiResponse("Reduce Quantity for Product : " + product.Name + " | by : " +  (item.Quantity - product.Quantity)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while updating Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            Order newOrder = new Order
            {
                Id = id,
                OrderStatusId = orderRequest.OrderStatusId,
                UserId = orderRequest.UserId,
                ModifiedAt = DateTime.UtcNow,
                CreatedAt = oldOrder.CreatedAt,
                Date = oldOrder.Date,
                TotalAmount = orderRequest.TotalAmount,
            };

            newOrder.OrderItems = new List<OrderItem>();

            foreach (var item in orderRequest.OrderItems)
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });


            try
            {
                _orderRepo.Add(newOrder);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Deleting Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            return Ok(new { responseStatus = true, responseMessage = "Order submitted Successfully" });
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var record = await _orderRepo.GetByIdAsync(id);

                if (record == null) return NotFound(new ApiResponse("Order Not Found!"));

                _orderRepo.Delete(record);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Deleting Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }
            return Ok(new { responseStatus = true, responseMessage = "Order deleted successfully" });
        }

        #endregion

        #region Status

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
        //        _logger.LogError(ex, "Error Occurred while Deleting Order");
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

                if (record.IsNullOrEmpty()) return NotFound(new ApiResponse("No Status Found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while GetAll OrderStatus");
                return BadRequest(new ApiResponse("Unknown Error Occured"));
            }

            return Ok(new { responseStatus = true, statusList = record });
        }

        // POST api/<OrderController>
        [HttpPost("status")]
        public async Task<IActionResult> AddOrderStatus([FromForm] string orderStatusName)
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
                        return Conflict(new ApiResponse("Status Already Exists"));

                _orderStatusRepo.Add(status);
                _orderStatusRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Adding OrderStatus");
                return BadRequest(new ApiResponse("UnknownError Occured"));
            }

            return Ok(new ApiResponse(true, "Status added successfully!"));
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

                if (statusRepo == null) return NotFound(new ApiResponse("Status Not Found!"));

                _orderStatusRepo.Update(status);
                _orderStatusRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Updating OrderStatus");
                return BadRequest(new ApiResponse("Unknown Error Occured"));
            }
            return Ok(new ApiResponse(true, "Status updated successfully!"));
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("status/{id}")]
        public async Task<IActionResult> DeleteOrderStatus(int id)
        {
            try
            {
                var record = await _orderRepo.GetByIdAsync(id);

                if (record == null) return NotFound(new ApiResponse("Status Not Found!"));

                _orderRepo.Delete(record);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Deleting Order");
                return BadRequest();
            }
            return Ok(new ApiResponse(true, "Status deleted successfully!"));
        }

        #endregion

    }
}
