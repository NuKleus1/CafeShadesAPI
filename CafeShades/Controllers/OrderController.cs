using AutoMapper;
using Cafeshades.Models.Dtos;
using CafeShades.Models;
using CafeShades.Models.Request;
using Core.Entities;
using Core.Interfaces;
using FirebaseAdmin.Messaging;
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
        private readonly IGenericRepository<UserToken> _userTokenRepo;
        private readonly IConfiguration _config;

        public OrderController(
            IMapper mapper,
            IGenericRepository<Order> orderRepo,
            IGenericRepository<OrderStatus> orderStatusRepo,
            ILogger<OrderController> logger,
            IWebHostEnvironment env,
            IGenericRepository<Product> productRepo,
            IGenericRepository<User> userRepo,
            StoreDbContext context,
            IGenericRepository<UserToken> userTokenRepo,
            IConfiguration config)
        {
            _mapper = mapper;
            _orderRepo = orderRepo;
            _orderStatusRepo = orderStatusRepo;
            _logger = logger;
            _env = env;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _context = context;
            _userTokenRepo = userTokenRepo;
            _config = config;
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

                if (record.OrderItems != null)
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

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetAllByUser(int id)
        {
            IReadOnlyList<Order> record;
            try
            {
                record = await _orderRepo.ListAllAsync(new List<Expression<Func<Order, object>>>
                {
                    or => or.OrderStatus,
                    or => or.OrderItems,
                }, or => or.UserId == id);

                if (record.IsNullOrEmpty()) return NotFound(new ApiResponse("Orders Not Found!"));


                foreach (var item in record)
                    foreach (var orderItem in item.OrderItems)
                        await _context.Entry(orderItem).Reference(oi => oi.Product).LoadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Retrieving Orders");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }


            return Ok(new { responseStatus = true, orderList = _mapper.Map<IReadOnlyList<OrderDto>>(record) });
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

            var data = _mapper.Map<IReadOnlyList<OrderDto>>(record);

            foreach (var order in data) { 
                var sum = 0;
                foreach (var item in order.productList)
                {
                    if (item != null) sum += item.productPrice * item.productQuantity;
                }
                order.totalAmount = sum;
            }


            return Ok(new { responseStatus = true, orderList =  data});
        }

        // POST api/<OrderController>
        [HttpPost()]
        public async Task<IActionResult> AddOrder([FromBody] OrderRequest orderRequest)
        {
            User user;
            try
            {
                //var status = await _orderStatusRepo.GetByIdAsync(orderRequest.OrderStatusId);
                //if (status == null)
                //    return BadRequest(new ApiResponse("Status does not Exists !"));

                user = await _userRepo.GetByIdAsync(orderRequest.UserId);
                if (user == null)
                    return NotFound(new ApiResponse("User does not Exists !"));

                foreach (var item in orderRequest.OrderItems)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product == null)
                        return NotFound(new ApiResponse("Product : " + item.ProductId + " Not Found!"));
                    //if (!(product.Quantity >= item.Quantity))
                    //    return BadRequest(new ApiResponse("Reduce Quantity for Product : " + product.Name));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while adding Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            Order newOrder = new Order
            {
                OrderStatusId = _orderStatusRepo.FirstOrDefault()?.Id ?? 1,
                UserId = orderRequest.UserId,
                CreatedAt = DateTime.UtcNow,
                Date = DateTime.UtcNow,
            };

            newOrder.OrderItems = new List<OrderItem>();

            foreach (var item in orderRequest.OrderItems)
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.ProductQuantity,
                });
            try
            {
                _orderRepo.Add(newOrder);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Submitting Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            try
            {
                var isNotificationSent = await SendNewOrderNotification(user.Name);
                //if(isNotificationSent)
                //    return Ok(new { responseStatus = true, responseMessage = "Order submitted Successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sending Notification for New Order");
                //return Ok(new { responseStatus = true, responseMessage = "Order submitted Successfully, Notification Not Sent" });
            }

            return Ok(new { responseStatus = true, responseMessage = "Order submitted Successfully" });
        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, [FromBody] OrderUpdateRequest orderUpdateRequest)
        {
            Order oldOrder;
            OrderStatus status;
            try
            {
                oldOrder = await _orderRepo.GetByIdAsync(id, or => or.OrderItems);

                if (oldOrder.UserId != orderUpdateRequest.UserId)
                    return Conflict(new ApiResponse("This order is not associated with the user"));

                if (oldOrder == null)
                    return NotFound(new ApiResponse("Order Not Found!"));

                status = await _orderStatusRepo.GetByIdAsync(orderUpdateRequest.OrderStatusId);

                if (status == null)
                    return NotFound(new ApiResponse("Status does not Exists !"));

                User user = await _userRepo.GetByIdAsync(orderUpdateRequest.UserId);

                if (user == null)
                    return NotFound(new ApiResponse("User does not Exists !"));

                foreach (var item in orderUpdateRequest.OrderItems)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);

                    if (product == null)
                        return NotFound(new ApiResponse("Product : " + item.ProductId + " Not Found!"));

                    //if (!(product.Quantity >= item.Quantity))
                    //    return NotFound(new ApiResponse("Reduce Quantity for Product : " + product.Name + " | by : " + (item.Quantity - product.Quantity)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while updating Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            //oldOrder = new Order
            //{
            //    Id = id,
            //    OrderStatusId = orderUpdateRequest.OrderStatusId,
            //    UserId = orderUpdateRequest.UserId,
            //    ModifiedAt = DateTime.UtcNow,
            //    CreatedAt = oldOrder.CreatedAt,
            //    Date = oldOrder.Date
            //};


            oldOrder.Id = id;
            oldOrder.OrderStatusId = orderUpdateRequest.OrderStatusId;
            oldOrder.UserId = orderUpdateRequest.UserId;
            oldOrder.ModifiedAt = DateTime.UtcNow;
            oldOrder.CreatedAt = oldOrder.CreatedAt;
            oldOrder.Date = oldOrder.Date;

            //oldOrder.OrderItems = new List<OrderItem>();

            foreach (var item in oldOrder.OrderItems)
                foreach (var itemRequest in orderUpdateRequest.OrderItems)
                    item.Quantity = item.ProductId == itemRequest.ProductId ? itemRequest.ProductQuantity : item.Quantity;


            try
            {
                //_orderRepo.Update(oldOrder);
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Updatings Order");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            try
            {
                
                UserToken userToken = await _userTokenRepo.GetByIdAsync(oldOrder.UserId);
                    
                if (userToken == null) return Ok(new { responseStatus = true, responseMessage = "Order updated Successfully, Notification not sent" });

                var isNotificationSent = await SendOrderUpdateChangeNotification(userToken.FcmToken, 
                        status.Id != oldOrder.Id ? "There's a product update for your Order " : "The status of your order is updated to " + status.StatusName
                    );
                
                if (isNotificationSent) return Ok(new { responseStatus = true, responseMessage = "Order updated Successfully, Notification Sent" });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Sending Notification");
                return BadRequest(new ApiResponse("Unknown Server Error Occured"));
            }

            return Ok(new { responseStatus = true, responseMessage = "Order updated Successfully" });
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

        //[HttpPost("{id}/changeStatus")]
        //public async Task<IActionResult> ChangeOrderStatus(int id, [FromBody] int orderStatusId)
        //{
        //    var oldOrder = await _orderRepo.GetByIdAsync(id);

        //    if (oldOrder != null)
        //        return NotFound(new ApiResponse("Order Not Found!"));


        //    return Ok(new { responseStatus = true, responseMessage = "Order status changed successfully" });
        //}

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
            try
            {
                var statusRepo = await _orderStatusRepo.GetByIdAsync(id);

                if (statusRepo == null) return NotFound(new ApiResponse("Status Not Found!"));

                statusRepo.StatusName = orderStatusName;
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

        [HttpPost("{orderId}/changeOrderStatus/{statusId}")]
        public async Task<IActionResult> ChangeOrderStatus(int orderId, int statusId)
        {
            Order record;
            OrderStatus status;
            try
            {
                record = await _orderRepo.GetByIdAsync(orderId, new List<Expression<Func<Order, object>>>
                {
                    or => or.OrderStatus,
                    or => or.OrderItems,
                });

                if (record == null) return NotFound(new ApiResponse("Order Not Found!"));

                status = await _orderStatusRepo.GetByIdAsync(statusId);

                if (status == null) return NotFound(new ApiResponse("Status Not Found!"));

                record.OrderStatusId = statusId;
                _orderRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sending notification!");
                return BadRequest(new ApiResponse("Error Occurred!"));
            }

            var userToken = await _userTokenRepo.GetByIdAsync(usrtoken => usrtoken.UserId == record.UserId);

            if (userToken == null)
                return Ok(new { responseStatus = true, responseMessage = "Order Status changed successfully, Notification not sent!" }); ;

            var isNotificationSent = await SendOrderUpdateChangeNotification(userToken.FcmToken, "The status of your order is updated to " + status.StatusName);

            if (!isNotificationSent)
                return Ok(new { responseStatus = true, responseMessage = "Order Status changed successfully, Notification sent" });

            return Ok(new { responseStatus = true, responseMessage = "Order Status changed successfully" });
        }

        private async Task<bool> SendOrderUpdateChangeNotification(string fcmToken, string notificationBody)
        {

            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = "Here's an Update",
                    Body = notificationBody,
                },
                Token = fcmToken
            };

            string result;

            try
            {
                var messaging = FirebaseMessaging.DefaultInstance;
                result = await messaging.SendAsync(message);
                if (result == null)
                    return false;

                _logger.LogInformation("Notification Sent to FCM : " + fcmToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sending notification!");
            }


            return true;

        }

        private async Task<bool> SendNewOrderNotification(string userName)
        {
            int adminId = int.Parse(_config["AdminId"]);

            var userToken = await _userTokenRepo.GetByIdAsync(uT => uT.UserId == adminId);

            if (userToken == null)
                return false;

            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = "Order Recieved",
                    Body = "Received an Order from " + userName,
                },
                Token = userToken.FcmToken
            };

            string result;

            try
            {
                var messaging = FirebaseMessaging.DefaultInstance;
                result = await messaging.SendAsync(message);
                if (result == null)
                    return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while sending notification!");
            }

            _logger.LogInformation("Notification Sent to FCM : " + userToken.FcmToken);

            return true;

        }

    }
}
