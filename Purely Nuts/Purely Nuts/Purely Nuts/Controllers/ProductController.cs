using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Purely_Nuts.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Purely_Nuts.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly List<Item> _cartItems = new List<Item>();

        public ProductController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AllProducts()
        {
            var products = _context.Product.ToList();
            return View(products);
        }


        public IActionResult Cart()
        {
            List<int> cart = HttpContext.Session.Get<List<int>>("Cart") ?? new List<int>();
            List<Product> products = new List<Product>();
            foreach (var id in cart)
            {
                var product = _context.Product.Find(id);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            return View(products);
        }

        public IActionResult AddToCart(int productId)
        {
            var product = _context.Product.Find(productId);
            if (product != null)
            {
                List<int> cart = HttpContext.Session.Get<List<int>>("Cart") ?? new List<int>();
                cart.Add(productId);
                HttpContext.Session.Set("Cart", cart);

                TempData["SuccessMessage"] = "Product added to cart successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Product not found!";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            return View();
        }
        public IActionResult Receipt()
        {
            return View();
        }
       
    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return default; // or throw an exception, depending on your requirements
            }
            return JsonConvert.DeserializeObject<T>(value);
        }

    }
}
