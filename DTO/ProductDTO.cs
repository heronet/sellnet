using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace sellnet.DTO
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public Guid CategoryId { get; set; }
        public int BuyersCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string City { get; set; }
        public string Division { get; set; }
        public double Price { get; set; }
        public PhotoDTO Thumbnail { get; set; }
        public string Brand { get; set; }
        public List<PhotoDTO> Photos { get; set; }
        public SupplierInfoDTO Supplier { get; set; }
    }
}