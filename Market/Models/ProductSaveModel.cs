using System;

namespace Market.Models
{
    public class ProductSaveModel
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public Guid CategoryId { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}