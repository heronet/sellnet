using System;
using System.Collections.Generic;

namespace sellnet.DTO
{
    public class SupplierInfoDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> Roles { get; set; }
    }
}