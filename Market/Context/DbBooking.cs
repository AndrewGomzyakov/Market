using System;
using System.ComponentModel.DataAnnotations;
using Market.Controllers;

namespace Market.Context
{
    public class DbBooking
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid BookerUserId { get; set; }

        public DbUser BookerUser { get; set; }
        
        public Guid OwnerUserId { get; set; }
        
        public Guid ProductId { get; set; }
        
        public string ProductName { get; set; }
        
        public int Count { get; set; }
    }
}