namespace Market.ServiceInterfaces
{
    public interface IPermissionService
    {
        bool HasPermissionToChangeCategories();

        bool HasPermissionToChangeProducts();

    }
}