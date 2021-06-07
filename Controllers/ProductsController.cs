using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sellnet.Data;
using sellnet.DTO;
using sellnet.Models;
using sellnet.Services;

namespace sellnet.Controllers
{
    [Authorize(Roles = "Admin, Member")]
    public class ProductsController : DefaultController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<Supplier> _userManager;
        private readonly PhotoService _photoService;
        public ProductsController(ApplicationDbContext dbContext, UserManager<Supplier> userManager, PhotoService photoService)
        {
            _photoService = photoService;
            _userManager = userManager;
            _dbContext = dbContext;
        }
        [AllowAnonymous]
        [HttpGet("all", Name = "GetProducts")]
        public async Task<ActionResult> GetProducts(string name, string category, string division, string city, int pageSize, int pageNumber, string sortParam)
        {
            var productsQuery = _dbContext.Products
                .Include(p => p.Supplier)
                .Include(p => p.Photos)
                .Where(p => (name == null || p.Name.ToLower().Contains(name.ToLower())))
                .Where(p => (category == null || p.CategoryName == category.ToLower()))
                .Where(p => (city == null || p.Supplier.City == city.ToLower()))
                .Where(p => (division == null || p.Supplier.Division == division.ToLower()));

            switch (sortParam?.ToLower())
            {
                case "price: low to high":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price: high to low":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "date: old to new":
                    productsQuery = productsQuery.OrderBy(p => p.CreatedAt);
                    break;
                case "date: new to old":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
            }
            var products = await productsQuery.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToListAsync();
            var totalItems = await productsQuery.CountAsync();
            var productDtos = new List<ProductDTO>();
            products.ForEach(p => productDtos.Add(ProductToDto(p)));

            return Ok(new GetResponseWithPageDTO<ProductDTO>(productDtos, totalItems));
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(Guid id)
        {
            var product = await _dbContext.Products
                .Include(p => p.Supplier)
                .Include(p => p.Photos)
                .Where(p => p.Id == id)
                .SingleOrDefaultAsync();
            if (product == null)
                return BadRequest("Product not found");
            var productDto = ProductToDto(product);
            productDto.Supplier = SupplierToDto(product.Supplier);
            var photos = new List<PhotoDTO>();
            foreach (var photo in product.Photos)
                photos.Add(PhotoToDto(photo));
            productDto.Photos = photos;
            return Ok(productDto);
        }
        [HttpPost]
        public async Task<ActionResult> AddProduct([FromForm] ProductDTO productDTO, [FromForm] List<IFormFile> photos)
        {
            var supplier = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (supplier == null)
                return Unauthorized("You must login to add products");
            var category = await _dbContext.Categories
                .Where(c => c.Name == productDTO.Category.ToLower().Trim())
                .SingleOrDefaultAsync();
            if (category == null)
            {
                category = new Category
                {
                    Name = productDTO.Category.ToLower().Trim()
                };
                _dbContext.Categories.Add(category);
            }
            if (photos.Count > 5)
                return BadRequest("Can't add more than 5 photos");
            var uploadedPhotos = new List<Photo>();
            foreach (var photo in photos)
            {
                var photoResult = await _photoService.AddPhotoAsync(photo);
                if (photoResult.Error != null)
                    return BadRequest(photoResult.Error.Message);
                var newPhoto = new Photo
                {
                    ImageUrl = photoResult.SecureUrl.AbsoluteUri,
                    PublicId = photoResult.PublicId
                };
                uploadedPhotos.Add(newPhoto);
            }
            var thmbnailResult = await _photoService.AddPhotoAsync(photos[0], "placeholder");
            if (thmbnailResult.Error != null)
                BadRequest("Can't add product");
            var product = new Product
            {
                Name = productDTO.Name.Trim(),
                Price = productDTO.Price,
                Category = category,
                SubCategory = productDTO.SubCategory,
                Description = productDTO.Description,
                CategoryName = category.Name,
                Supplier = supplier,
                SupplierName = supplier.Name,
                Photos = uploadedPhotos,
                ThumbnailUrl = thmbnailResult.SecureUrl.AbsoluteUri,
                ThumbnailId = thmbnailResult.PublicId,
                Brand = productDTO.Brand
            };
            _dbContext.Products.Add(product);
            if (await _dbContext.SaveChangesAsync() > 0)
                return CreatedAtAction("GetProducts", new { Response = $"Succesfully added product by {supplier.Name}" });
            return BadRequest("Adding product failed");
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            var supplier = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var roles = await _userManager.GetRolesAsync(supplier);
            if (supplier == null)
                return Unauthorized("You cannot delete this product");
            var product = await _dbContext
                .Products
                .Include(p => p.Photos)
                .Where(p => p.Id == id)
                .SingleOrDefaultAsync();
            if (product == null)
                return BadRequest("Couldn't find product");
            if (product.SupplierId != supplier.Id)
                return Unauthorized("You cannot delete this product");
            foreach (var photo in product.Photos)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                    BadRequest("Can't Delete Product");
            }
            await _photoService.DeletePhotoAsync(product.ThumbnailId);
            _dbContext.Products.Remove(product);
            if (await _dbContext.SaveChangesAsync() > 0)
                return Ok(new { Message = "Product Deleted successfully" });
            return BadRequest("Cant't Delete Product");
        }
        private ProductDTO ProductToDto(Product product)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var productDto = new ProductDTO
            {
                Name = textInfo.ToTitleCase(product.Name),
                Price = product.Price,
                Id = product.Id,
                Description = product.Description,
                BuyersCount = product.BuyersCount,
                Category = textInfo.ToTitleCase(product.CategoryName),
                SubCategory = product.SubCategory,
                CategoryId = product.CategoryId,
                CreatedAt = product.CreatedAt,
                City = textInfo.ToTitleCase(product.Supplier.City),
                Division = textInfo.ToTitleCase(product.Supplier.Division),
                Thumbnail = new PhotoDTO { ImageUrl = product.ThumbnailUrl, PublicId = product.ThumbnailUrl },
                Brand = product.Brand
            };
            var photos = new List<PhotoDTO>();
            foreach (var photo in product.Photos)
                photos.Add(PhotoToDto(photo));
            productDto.Photos = photos;
            return productDto;
        }
        private PhotoDTO PhotoToDto(Photo photo)
        {
            return new PhotoDTO
            {
                ImageUrl = photo.ImageUrl,
                PublicId = photo.PublicId
            };
        }
        private SupplierInfoDTO SupplierToDto(Supplier supplier)
        {
            return new SupplierInfoDTO
            {
                Name = supplier.Name,
                Id = supplier.Id,
                Phone = supplier.PhoneNumber,
                Email = supplier.Email
            };
        }
    }
}