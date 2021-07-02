using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using sellnet.DTO;
using sellnet.Models;
using sellnet.Services;

namespace sellnet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : DefaultController
    {
        private readonly UserManager<Supplier> _userManager;
        private readonly SignInManager<Supplier> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenService _tokenService;
        private readonly UtilityService _utilityService;

        public AdminController(
            UserManager<Supplier> userManager,
            SignInManager<Supplier> signInManager,
            RoleManager<IdentityRole> roleManager,
            TokenService tokenService,
            UtilityService utilityService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _utilityService = utilityService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<SupplierAuthDTO>> RegisterAdmin(RegisterDTO registerDTO)
        {
            var adminExists = await _roleManager.RoleExistsAsync("Admin");
            if (!adminExists)
            {
                var role = new IdentityRole("Admin");
                var roleResult = await _roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult);
            }
            var city = _utilityService.Cities.Where(city => city.ToLower() == registerDTO.City.Trim().ToLower()).SingleOrDefault();
            if (city == null)
                return BadRequest("Invalid City");
            var division = _utilityService.Divisions.Where(d => d.ToLower() == registerDTO.Division.ToLower().Trim()).SingleOrDefault();
            if (division == null)
                return BadRequest("Invalid Division");
            var supplier = new Supplier
            {
                Name = registerDTO.Name.Trim(),
                UserName = registerDTO.Email.ToLower().Trim(),
                Email = registerDTO.Email.ToLower().Trim(),
                PhoneNumber = registerDTO.Phone,
                City = city.ToLower(),
                Division = division.ToLower()
            };
            var result = await _userManager.CreateAsync(supplier, password: registerDTO.Password);
            if (!result.Succeeded) return BadRequest(result);

            var addToRoleResult = await _userManager.AddToRoleAsync(supplier, "Admin");
            if (addToRoleResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(supplier);
                return await SupplierToDto(supplier, roles.ToList());
            }
            return BadRequest("Can't add Admin");
        }
        private async Task<SupplierAuthDTO> SupplierToDto(Supplier supplier, List<string> roles)
        {
            return new SupplierAuthDTO
            {
                Name = supplier.Name,
                Token = await _tokenService.GenerateToken(supplier),
                Id = supplier.Id,
                Roles = roles
            };
        }
    }
}