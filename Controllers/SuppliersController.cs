using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sellnet.Data;
using sellnet.DTO;
using sellnet.Models;

namespace sellnet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SuppliersController : DefaultController
    {
        private readonly UserManager<Supplier> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public SuppliersController(UserManager<Supplier> userManager, ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<ActionResult> FindUsers(string searchBy, string query)
        {
            // Lookup by Email
            if (searchBy == "email")
            {
                var supplier = await _userManager.FindByEmailAsync(query);
                if (supplier == null)
                    return BadRequest("User Not Found");
                var roles = await _userManager.GetRolesAsync(supplier);
                return Ok(SupplierToDto(supplier, roles.ToList()));
            }
            // Lookup by Username
            else if (searchBy == "username")
            {
                var supplier = await _userManager.FindByNameAsync(query);
                if (supplier == null)
                    return BadRequest("User Not Found");
                var roles = await _userManager.GetRolesAsync(supplier);
                return Ok(SupplierToDto(supplier, roles.ToList()));
            }
            // If code reches this point, that means searchBy is invalid. So we return 400
            return BadRequest("Invalid Query");
        }
        [HttpGet("all")]
        public async Task<ActionResult<GetResponseWithPageDTO<SupplierInfoDTO>>> GetUsers(
            [FromQuery] int pageSize = 100,
            [FromQuery] int pageCount = 1
        )
        {
            var currentSupplier = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var suppliers = await _userManager.Users.Where(u => u.Id != currentSupplier).ToListAsync();
            long suppliersCount = suppliers.Count;

            var supplierDtos = new List<SupplierInfoDTO>();
            foreach (var supplier in suppliers.Skip(pageSize * (pageCount - 1)).Take(pageSize))
            {
                var roles = await _userManager.GetRolesAsync(supplier);
                supplierDtos.Add(SupplierToDto(supplier, roles.ToList()));
            }

            return Ok(new GetResponseWithPageDTO<SupplierInfoDTO>(supplierDtos, suppliersCount));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSupplier(string id)
        {
            var currentSupplier = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (currentSupplier == id)
                return BadRequest("You cannot delete yourself");
            Supplier supplier = await _dbContext.Users.Where(u => u.Id == id)
                .Include(s => s.Products)
                .SingleOrDefaultAsync();
            if (supplier == null)
                return BadRequest("User doesn't exist");
            var products = await _dbContext.Products
                .Where(e => e.SupplierId == supplier.Id)
                .ToListAsync();
            _dbContext.Products.RemoveRange(products);
            await _userManager.DeleteAsync(supplier);

            if (await _dbContext.SaveChangesAsync() > 0)
                return NoContent();
            return Ok("Failed to delete supplier");
        }

        private SupplierInfoDTO SupplierToDto(Supplier supplier, List<string> roles)
        {
            return new SupplierInfoDTO
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Email = supplier.Email,
                Phone = supplier.PhoneNumber,
                Roles = roles
            };
        }
    }
}