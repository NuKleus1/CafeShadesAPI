using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CafeShades.Models.Request
{
    public class OrderRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public List<OrderItemRequest> OrderItems { get; set; }
    }
}
