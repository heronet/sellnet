using System;
using System.Collections.Generic;

namespace sellnet.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public Supplier Supplier { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public Category Category { get; set; }
        public string SubCategory { get; set; }
        public string CategoryName { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int BuyersCount { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ThumbnailId { get; set; }
        public string Brand { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(6.00);
        public ICollection<Photo> Photos { get; set; }
    }
}