using System.Threading.Tasks;

namespace Market.ServiceInterfaces
{
    public interface ISmsProvider
    {
        Task SendMessage(string phone, string message);
    }
}