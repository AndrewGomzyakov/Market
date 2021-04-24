using System;
using System.Linq;
using System.Threading.Tasks;
using Market.Context;
using Market.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Market.Services
{
    public class ProductService : IProductService
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MarketContext _context;

        public ProductService(IPermissionService permissionService, MarketContext context, IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DbProduct[]> GetRegistry(Guid categoryId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId && p.Count > 0)
                .ToArrayAsync();
            return products;
        }
        
        public async Task<DbProduct> Save(DbProduct product)
        {
            if (!_permissionService.HasPermissionToChangeProducts())
            {
                throw new InvalidOperationException("Нет прав доступа для изменения Товаров");
            }

            if (product.Id != Guid.Empty)
            {
                var existProuct = await _context.Products.Where(x => x.Id == product.Id).FirstOrDefaultAsync();
                if (existProuct == null)
                {
                    throw new InvalidOperationException("Позиция не найдена.");
                }
                Update(existProuct, product);
                _context.Update(existProuct);
                await _context.SaveChangesAsync();
                return existProuct;
            }
            product.OwnerId = Guid.Parse(_httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserId").Value);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        
        public async Task DeleteProduct(Guid productId)
        {
            if (!_permissionService.HasPermissionToChangeProducts())
            {
                throw new InvalidOperationException("Нет прав доступа для изменения Товаров");
            }
            var product  = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
            var productBooking = await _context.Bookings.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (product != null)
            {
                _context.Remove(product);
            }

            if (productBooking != null)
            {
                _context.Remove(productBooking);
            }
            await _context.SaveChangesAsync();
        }

        public async Task Reserve(Guid productId)
        {
            var product = await _context.Products
                .FirstAsync(x => x.Id == productId);
            var productBooking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.ProductId == productId);
            if (product.Count == 0)
            {
                throw new InvalidOperationException();
            }
            if (productBooking != null)
            {
                product.Count--;
                productBooking.Count++;
            }
            else
            {
                product.Count--;
                productBooking = new DbBooking
                {
                    ProductName = product.Name,
                    Count = 1,
                    ProductId = productId,
                    BookerUserId = Guid.Parse(_httpContextAccessor.HttpContext.User.Claims
                        .First(c => c.Type == "UserId").Value),
                    OwnerUserId = product.OwnerId
                };
            }
            _context.Update(product);
            _context.Update(productBooking);
            await _context.SaveChangesAsync();
        }

        public async Task Unreserve(Guid productId)
        {
            var product = await _context.Products
                .FirstAsync(x => x.Id == productId);
            var productBooking = await _context.Bookings
                .FirstAsync(x => x.ProductId == productId);
            if (productBooking.Count == 0)
            {
                throw new InvalidOperationException();
            }

            product.Count++;
            productBooking.Count--;
            _context.Update(product);
            _context.Update(productBooking);
            await _context.SaveChangesAsync();
        }

        private void Update(DbProduct oldProduct, DbProduct newProduct)
        {
            oldProduct.Count = newProduct.Count;
            oldProduct.Image = newProduct.Image;
            oldProduct.Name = newProduct.Name;
            oldProduct.Price = newProduct.Price;
            oldProduct.CategoryId = newProduct.CategoryId;
            oldProduct.OwnerId = Guid.Parse(_httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserId").Value);
        }
    }
}