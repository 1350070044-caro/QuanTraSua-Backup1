using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanTraSua.Data;
using System.Linq;

namespace QuanTraSua.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminAuth") == "true")
                return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("AdminAuth", "true");
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
            return View();
        }

        public IActionResult Dashboard()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login");

            var orders = _context.Orders.ToList();
            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalRevenue = orders.Any() ? orders.Sum(o => o.TotalPrice) : 0;
            ViewBag.TotalQuantity = orders.Sum(o => o.OrderDetails.Split(',').Length);

            var recentOrders = _context.Orders.OrderByDescending(o => o.OrderDate).Take(10).ToList();
            return View(recentOrders);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminAuth");
            return RedirectToAction("Login");
        }

        private bool IsAdminAuthenticated()
        {
            return HttpContext.Session.GetString("AdminAuth") == "true";
        }
    }
}
