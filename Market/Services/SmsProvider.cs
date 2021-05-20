using System.Net.Http;
using System.Threading.Tasks;
using Market.ServiceInterfaces;

namespace Market.Services
{
    public class SmsProvider : ISmsProvider
    {
        private const string Login = "Vershinin5";
        private const string Password = "MatMeh83";
        
        public async Task SendMessage(string phone, string message)
        {
            var httpClient = new HttpClient();
            await httpClient.GetAsync($"https://smsc.ru/sys/send.php?login={Login}&psw={Password}&phones={phone}&mes={message}");
        }
    }
}