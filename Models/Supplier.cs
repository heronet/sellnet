using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace sellnet.Models
{
    public class Supplier : IdentityUser
    {
        public ICollection<Product> Products { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Division { get; set; }
    }
}