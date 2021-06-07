using System;

namespace sellnet.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}