using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Market.Context
{
    [Table("product", Schema = "market")]
    public class DbProduct
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public Guid OwnerId { get; set; }
        [Required]
        public int Count { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        public DbCategory Category { get; set; }
        [Required]
        public byte[] Image { get; set; }
    }
}