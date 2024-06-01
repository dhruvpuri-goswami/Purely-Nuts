using Microsoft.AspNetCore.Mvc;
using Purely_Nuts.Models;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Purely_Nuts.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var products = _context.Product.ToList();
            ViewBag.IsProductAvailable = new Func<int, bool>(IsProductAvailable);
            var session = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            ViewBag.sess = session;
            ViewBag.Cart = GetCart();
            return View(products);
        }

        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            ViewBag.CartCount = 0;

            var product = _context.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null || product.Quantity < 1)
            {
                TempData["ErrorMessage"] = "Product not available.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            if (cart.ContainsKey(productId))
            {
                cart[productId] += quantity;
            }
            else
            {
                cart.Add(productId, quantity);
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = "Added to cart successfully!";
            ViewBag.CartCount = cart.Sum(x => x.Value);
            return RedirectToAction("Index");
        }


        private Dictionary<int, int> GetCart()
        {
            string sessionData = _httpContextAccessor.HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData))
                return new Dictionary<int, int>();
            return JsonConvert.DeserializeObject<Dictionary<int, int>>(sessionData);
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            string sessionData = JsonConvert.SerializeObject(cart);
            _httpContextAccessor.HttpContext.Session.SetString("Cart", sessionData);
        }


        private bool IsProductAvailable(int productId)
        {
            var product = _context.Product.FirstOrDefault(p => p.ProductId == productId);
            return product != null && product.Quantity > 0;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
