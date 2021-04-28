using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Market.Models;

namespace Market.Context
{
    [Table("users", Schema = "account")]
    public class DbUser
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string SecondName { get; set; }

        [Required]
        public UserRole Role { get; set; }
        
        public string Code { get; set; }

        [Required]
        public UserStatus Status { get; set; }
    }
}