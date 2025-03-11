using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using WebApplication6.Models;
using Org.BouncyCastle.Crypto.Generators;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        private string connStr = "Server=localhost;Database=account;User=root;Password=;";

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public IActionResult RegisterUser(string username, string password)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();


            using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM user WHERE username = @username", conn);
            checkCmd.Parameters.AddWithValue("@username", username);
            int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (userCount > 0)
            {
                ViewBag.Error = "This username is already taken.";
                return View("Register");
            }


            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            using var cmd = new MySqlCommand("INSERT INTO user (username, password) VALUES (@username, @password)", conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Login");
        }


        [HttpPost]
        public IActionResult Authenticate(string username, string password)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT password FROM user WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string storedHashedPassword = reader.GetString("password");


                if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
                {
                    HttpContext.Session.SetString("Username", username);
                    return RedirectToAction("UserDashboard");
                }
            }

            ViewBag.Error = "Incorrect username or password.";
            return View("Login");
        }

        public IActionResult UserDashboard()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
