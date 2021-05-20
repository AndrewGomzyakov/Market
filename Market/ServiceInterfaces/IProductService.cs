using System;
using System.Threading.Tasks;
using Market.Context;

namespace Market.ServiceInterfaces
{
    public interface IProductService
    {
        Task<DbProduct[]> GetRegistry(Guid categoryId);
        Task<DbProduct> Save(DbProduct product);
        Task DeleteProduct(Guid productId);
        Task Reserve(Guid productId);
        Task Unreserve(Guid productId);
    }
}