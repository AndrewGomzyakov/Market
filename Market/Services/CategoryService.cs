using System;
using System.Linq;
using System.Threading.Tasks;
using Market.Context;
using Market.ServiceInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Market.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly MarketContext _context;
        private readonly IPermissionService _permissionService;

        public CategoryService(MarketContext context, IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        public async Task<DbCategory[]> GetCategories()
        {
            return await _context.Categories.ToArrayAsync();
        }

        public async Task<DbCategory> SaveCategory(DbCategory category)
        {
            if (!_permissionService.HasPermissionToChangeCategories())
            {
                throw new InvalidOperationException("Нет прав доступа для изменения категорий");
            }

            if (category.Id != Guid.Empty)
            {
                var existCategory = await _context.Categories.Where(x => x.Id == category.Id).FirstOrDefaultAsync();
                if (existCategory == null)
                {
                    throw new InvalidOperationException("Категория не найдена.");
                }
                Update(existCategory, category);
                _context.Update(existCategory);
                await _context.SaveChangesAsync();
                return existCategory;
            }
            else
            {
                _context.Categories.Add(category);
            }
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategory(Guid categoryId)
        {
            if (!_permissionService.HasPermissionToChangeCategories())
            {
                throw new InvalidOperationException("Нет прав доступа для изменения категорий");
            }
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
            var products = await _context.Products.Where(x => x.CategoryId == categoryId).ToArrayAsync();
            var productIds = products.Select(x => x.Id);
            var bookings = await _context.Bookings.Where(x => productIds.Contains(x.ProductId)).ToArrayAsync();
            if (category != null)
            {
                _context.Remove(category);
                _context.RemoveRange(products);
                _context.RemoveRange(bookings);
                await _context.SaveChangesAsync();
            }
        }

        private void Update(DbCategory oldCategory, DbCategory newCategory)
        {
            oldCategory.Image = newCategory.Image;
            oldCategory.Name = newCategory.Name;
        }
    }
}