using System.Linq;
using Market.ServiceInterfaces;
using Microsoft.AspNetCore.Http;

namespace Market.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasPermissionToChangeCategories()
        {
            var role = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserRole");
            return role.Value == "Admin";
        }

        public bool HasPermissionToChangeProducts()
        {
            var role = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserRole");
            return role.Value == "Manager";
        }
    }
}