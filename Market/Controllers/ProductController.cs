using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Market.Context;
using Market.Models;
using Market.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Market.Controllers
{
    [Route("Product")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly MarketContext _context;

        public ProductController(IProductService productService, MarketContext context)
        {
            _productService = productService;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("GetRegistry/{categoryId}")]
        public async Task<IActionResult> GetRegistry([FromRoute] Guid categoryId)
        {
            var products = await _productService.GetRegistry(categoryId);
            return View(new ProductRegistryModel{
                Products = products,
                CategoryId = categoryId
            });
        }

        [HttpGet("Save")]
        public async Task<IActionResult> Save([FromQuery]Guid productId, [FromQuery]Guid categoryId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
            return View(new ProductSaveModel
            {
                Count = product?.Count ?? 0,
                Id = productId,
                Name = product?.Name ?? "",
                Price = product?.Price ?? 0,
                CategoryId = categoryId
            });
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save(Guid id, string name, decimal price, int count, Guid categoryId)
        {
            var file = Request.Form.Files.FirstOrDefault();
            byte[] imageData = null;
            if (file != null)
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)file.Length);
                }

            var product = new DbProduct
            {
                Id = id,
                Count = count,
                Image = imageData,
                Name = name,
                Price = price,
                CategoryId = categoryId,
            };
            await _productService.Save(product);
            return Redirect($"/Product/GetRegistry/{categoryId}");
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> Delete([FromQuery]Guid productId, [FromQuery]Guid categoryId)
        {
            await _productService.DeleteProduct(productId);
            return Redirect($"/Product/GetRegistry/{categoryId}");
        }

        [HttpGet("Reserve")]
        public async Task<IActionResult> Reserve([FromQuery]Guid productId, [FromQuery]Guid categoryId)
        {
            await _productService.Reserve(productId);
            return Redirect($"/Product/GetRegistry/{categoryId}");
        }
        
        [HttpGet("Unreserve")]
        public async Task<IActionResult> Unreserve([FromQuery]Guid productId)
        {
            await _productService.Unreserve(productId);
            return Redirect("/User/GetUserBookings");
        }
    }
}