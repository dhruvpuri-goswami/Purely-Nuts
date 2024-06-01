using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Purely_Nuts.Models;

namespace Purely_Nuts.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user, string con_password)
        {
            if (ModelState.IsValid && user.Password == con_password)
            {
                //Checking if username or email already exists
                if (_context.User.Any(u => u.Username == user.Username) || _context.User.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("", "Username or Email already exists.");
                    return View();
                }

                //Adding user to the database
                _context.User.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Password and confirm password do not match.");
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username.Equals("admin") && password.Equals("admin"))
            {
                var adminId = 2;
                _httpContextAccessor.HttpContext.Session.SetString("UserId", adminId.ToString());
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                //Checking if the username or email exists in the database
                var user = _context.User.FirstOrDefault(u => u.Username == username || u.Email == username);

                if (user == null || user.Password != password)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View();
                }

                //Storing the user in session
                _httpContextAccessor.HttpContext.Session.SetString("UserId", user.Id.ToString());

                //Redirecting the user to a dashboard or home page
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext.Session.Remove("UserId");
            return RedirectToAction("Login"); // Redirect to home or login page
        }

    }
}