using System;

namespace Market.Models
{
    public class RegistryModel
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}