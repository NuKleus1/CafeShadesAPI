﻿using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CafeShades.Models.Request
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
