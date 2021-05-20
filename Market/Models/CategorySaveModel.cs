using System;

namespace Market.Models
{
    public class CategorySaveModel
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string ErrorMessage { get; set; }
    }
}