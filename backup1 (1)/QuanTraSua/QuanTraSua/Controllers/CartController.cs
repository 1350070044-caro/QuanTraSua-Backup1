using Microsoft.AspNetCore.Mvc;
using QuanTraSua.Data;
using QuanTraSua.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace QuanTraSua.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context) { _context = context; }

        private List<CartItem> GetCartItems() {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart) {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }

        public IActionResult Index() {
            var cart = GetCartItems();
            ViewBag.Total = cart.Sum(s => s.Price * s.Quantity);
            return View(cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1) {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item == null) {
                cart.Add(new CartItem {
                    ProductId = id, 
                    ProductName = product.Name ?? "Sản phẩm", 
                    Price = product.Price, 
                    Quantity = quantity, 
                    ImageUrl = product.ImageUrl ?? "" 
                });
            } else {
                item.Quantity += quantity;
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id) {
            var cart = GetCartItems();
            cart.RemoveAll(c => c.ProductId == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }
    }

    public class CartItem {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}