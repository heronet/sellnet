using System.Collections.Generic;

namespace sellnet.DTO
{
    public class SupplierAuthDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public List<string> Roles { get; set; }
    }
}