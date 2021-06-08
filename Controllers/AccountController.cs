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
    public class AccountController : DefaultController
    {
        private readonly UserManager<Supplier> _userManager;
        private readonly TokenService _tokenService;
        private readonly UtilityService _utilityService;
        private readonly SignInManager<Supplier> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(
            UserManager<Supplier> userManager,
            SignInManager<Supplier> signInManager,
            RoleManager<IdentityRole> roleManager,
            TokenService tokenService,
            UtilityService utilityService
        )
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _utilityService = utilityService;
            _userManager = userManager;
        }
        /// <summary>
        /// POST api/account/register
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns><see cref="SupplierAuthDTO" /></returns>
        [HttpPost("register")]
        public async Task<ActionResult<SupplierAuthDTO>> RegisterUser(RegisterDTO registerDTO)
        {
            var memberRoleExists = await _roleManager.RoleExistsAsync("Member");
            if (!memberRoleExists)
            {
                var role = new IdentityRole("Member");
                var roleResult = await _roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                    return BadRequest("Can't add to Member");
            }
            var city = _utilityService.Cities.Where(city => city.ToLower() == registerDTO.City.Trim()).SingleOrDefault();
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

            var addToRoleResult = await _userManager.AddToRoleAsync(supplier, "Member");
            if (addToRoleResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(supplier);
                return await SupplierToDto(supplier, roles.ToList());
            }

            return BadRequest("Can't add supplier");
        }
        /// <summary>
        /// POST api/account/login
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns><see cref="SupplierAuthDTO" /></returns>
        [HttpPost("login")]
        public async Task<ActionResult<SupplierAuthDTO>> LoginUser(LoginDTO loginDTO)
        {
            var supplier = await _userManager.FindByEmailAsync(loginDTO.Email.ToLower().Trim());

            // Return If supplier was not found
            if (supplier == null) return BadRequest("Invalid Email");

            var result = await _signInManager.CheckPasswordSignInAsync(supplier, password: loginDTO.Password, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(supplier);
                return await SupplierToDto(supplier, roles.ToList());
            }

            return BadRequest("Invalid Password");
        }
        /// <summary>
        /// POST api/account/refresh
        /// </summary>
        /// <param name="supplierAuthDTO"></param>
        /// <returns><see cref="SupplierAuthDTO" /></returns>
        [Authorize]
        [HttpPost("refresh")]
        public async Task<ActionResult<SupplierAuthDTO>> RefreshToken(SupplierAuthDTO supplierAuthDTO)
        {

            var supplier = await _userManager.FindByIdAsync(supplierAuthDTO.Id);

            // Return If supplier was not found
            if (supplier == null) return BadRequest("Invalid User");

            var roles = await _userManager.GetRolesAsync(supplier);
            return await SupplierToDto(supplier, roles.ToList());
        }

        /// <summary>
        /// Utility Method.
        /// Converts a WhotUser to an AuthUserDto
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns><see cref="SupplierAuthDTO" /></returns>
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