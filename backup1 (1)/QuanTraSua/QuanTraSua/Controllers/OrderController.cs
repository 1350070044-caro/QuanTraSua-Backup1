using Microsoft.AspNetCore.Mvc;
using QuanTraSua.Data;
using QuanTraSua.Models;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace QuanTraSua.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. TRANG QUẢN LÝ ĐƠN HÀNG (Dành cho Admin)
        // ==========================================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("AdminAuth") != "true")
                return Redirect("/Admin/Login");

            // Lấy danh sách đơn hàng mới nhất lên đầu
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View("~/Areas/Admin/Views/Orders/Index.cshtml", orders);
        }

        // ==========================================
        // 2. TRANG THANH TOÁN (Dành cho Khách hàng)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Menu", "Home");
            }

            // Giải mã giỏ hàng từ Session
            var cart = JsonSerializer.Deserialize<List<QuanTraSua.Controllers.CartItem>>(sessionData) 
                       ?? new List<QuanTraSua.Controllers.CartItem>();
            
            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(x => x.Price * x.Quantity);
            
            return View();
        }

        // Xử lý khi khách bấm nút "Xác nhận đặt hàng"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order)
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData))
            {
                return RedirectToAction("Menu", "Home");
            }

            var cart = JsonSerializer.Deserialize<List<QuanTraSua.Controllers.CartItem>>(sessionData) 
                       ?? new List<QuanTraSua.Controllers.CartItem>();

            if (ModelState.IsValid)
            {
                // 1. Tính tổng tiền (Giá món + 20k phí ship)
                decimal subtotal = cart.Sum(x => x.Price * x.Quantity);
                order.TotalPrice = subtotal + 20000; 

                // 2. Lưu ngày đặt
                order.OrderDate = DateTime.Now;
                
                // 3. Chuyển giỏ hàng thành chuỗi văn bản để lưu vào cột OrderDetails
                order.OrderDetails = string.Join(", ", cart.Select(c => $"{c.ProductName} (x{c.Quantity})"));

                // 4. Lưu vào Database
                _context.Orders.Add(order);
                _context.SaveChanges();

                // 5. Xóa giỏ hàng sau khi đặt xong
                HttpContext.Session.Remove("Cart");

                return RedirectToAction("Success");
            }

            // Nếu có lỗi nhập liệu, quay lại trang thanh toán kèm dữ liệu giỏ hàng
            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(x => x.Price * x.Quantity);
            return View(order);
        }

        // ==========================================
        // 3. TRANG THÔNG BÁO THÀNH CÔNG
        // ==========================================
        public IActionResult Success()
        {
            return View();
        }
        

        // ==========================================
        // 4. XÓA ĐƠN HÀNG (Tùy chọn thêm để quản lý)
        // ==========================================
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("AdminAuth") != "true")
                return Redirect("/Admin/Login");

            var order = _context.Orders.Find(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

