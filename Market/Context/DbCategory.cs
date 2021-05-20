using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Market.Context
{ 
    [Table("category", Schema = "market")]
    public class DbCategory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public byte[] Image { get; set; }
        
        public ICollection<DbProduct> Products { get; set; }
    }
}