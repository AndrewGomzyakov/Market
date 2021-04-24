using System;
using System.Collections.Generic;
using Market.Context;

namespace Market.Models
{
    public class ProductRegistryModel
    {
        public IEnumerable<DbProduct> Products { get; set; }
        public Guid CategoryId { get; set; }
    }
}