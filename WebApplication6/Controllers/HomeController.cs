using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string connStr = "Server=localhost;Database=account;User=root;Password=;";
        private const int PageSize = 5; // Number of users per page

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string searchQuery = "", int page = 1)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUsername")))
            {
                return RedirectToAction("Login", "Admin");
            }

            List<User> users;
            int totalUsers;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                (users, totalUsers) = SearchUsers(searchQuery, page, PageSize);
            }
            else
            {
                (users, totalUsers) = GetUsers(page, PageSize);
            }

            ViewBag.SearchQuery = searchQuery; // Retain search input value
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / PageSize);

            return View(users);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private (List<User>, int) GetUsers(int page, int pageSize)
        {
            var users = new List<User>();
            int totalUsers = 0;
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var countCmd = new MySqlCommand("SELECT COUNT(*) FROM user", conn);
            totalUsers = Convert.ToInt32(countCmd.ExecuteScalar());

            using var cmd = new MySqlCommand("SELECT * FROM user LIMIT @limit OFFSET @offset", conn);
            cmd.Parameters.AddWithValue("@limit", pageSize);
            cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password")
                });
            }
            return (users, totalUsers);
        }

        private (List<User>, int) SearchUsers(string searchQuery, int page, int pageSize)
        {
            var users = new List<User>();
            int totalUsers = 0;
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var countCmd = new MySqlCommand("SELECT COUNT(*) FROM user WHERE username LIKE @query", conn);
            countCmd.Parameters.AddWithValue("@query", "%" + searchQuery + "%");
            totalUsers = Convert.ToInt32(countCmd.ExecuteScalar());

            using var cmd = new MySqlCommand("SELECT * FROM user WHERE username LIKE @query LIMIT @limit OFFSET @offset", conn);
            cmd.Parameters.AddWithValue("@query", "%" + searchQuery + "%");
            cmd.Parameters.AddWithValue("@limit", pageSize);
            cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password")
                });
            }
            return (users, totalUsers);
        }

        public IActionResult InsertUser(RetrieveUsers retrieveUsers)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO user (username, password) VALUES (@username, @password)", conn);
            cmd.Parameters.AddWithValue("@username", retrieveUsers.Username);
            cmd.Parameters.AddWithValue("@password", retrieveUsers.Password);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateUser(RetrieveUsers retrieve)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE user SET password = @password WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", retrieve.Id);
            cmd.Parameters.AddWithValue("@password", retrieve.Password);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteUser(RetrieveUsers bsit)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM user WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", bsit.Id);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Login", "Admin"); // Redirect to admin login
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
