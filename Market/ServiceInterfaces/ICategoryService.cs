using System;
using System.Threading.Tasks;
using Market.Context;

namespace Market.ServiceInterfaces
{
    public interface ICategoryService
    {
        Task<DbCategory[]> GetCategories();
        Task<DbCategory> SaveCategory(DbCategory category);
        Task DeleteCategory(Guid categoryId);
    }
}