using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using WebApplication6.Models;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly string connStr = "Server=localhost;Database=account;User=root;Password=;";

        // ✅ Admin Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(AdminUser admin)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM admin WHERE username = @username", conn);
            checkCmd.Parameters.AddWithValue("@username", admin.Username);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count > 0)
            {
                ViewBag.Error = "Username already exists. Please choose another.";
                return View();
            }


            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.Password);

            using var cmd = new MySqlCommand("INSERT INTO admin (username, password) VALUES (@username, @password)", conn);
            cmd.Parameters.AddWithValue("@username", admin.Username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AdminUser admin)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var cmd = new MySqlCommand("SELECT password FROM admin WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("@username", admin.Username);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string hashedPassword = reader.GetString("password");
                if (BCrypt.Net.BCrypt.Verify(admin.Password, hashedPassword))
                {
                    HttpContext.Session.SetString("AdminUsername", admin.Username);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminUsername");
            return RedirectToAction("Login");
        }
    }
}