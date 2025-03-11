using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;

namespace WebApplication6.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; }

        // Method to hash password before saving to database
        public void HashPassword()
        {
            Password = BCrypt.Net.BCrypt.HashPassword(Password);
        }
    }
}