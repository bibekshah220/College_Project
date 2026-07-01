using CafeManagementSystem.Data;
using CafeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagementSystem.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bills = await _context.Bills
                .Include(b => b.Order)
                    .ThenInclude(o => o!.TableInfo)
                .OrderByDescending(b => b.GeneratedAt)
                .ToListAsync();
            return View(bills);
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.TableInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            // Check if bill already exists
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.OrderId == orderId);
            if (existingBill != null)
            {
                return RedirectToAction(nameof(Details), new { id = existingBill.Id });
            }

            decimal subTotal = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
            decimal tax = subTotal * 0.05m; // 5% tax

            var bill = new Bill
            {
                OrderId = orderId,
                Order = order,
                SubTotal = subTotal,
                Tax = tax,
                Discount = 0,
                TotalAmount = subTotal + tax
            };

            return View(bill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(Bill bill)
        {
            bill.TotalAmount = bill.SubTotal + bill.Tax - bill.Discount;
            bill.GeneratedAt = DateTime.UtcNow;

            _context.Bills.Add(bill);
            
            // Mark order as served or similar if needed
            var order = await _context.Orders.Include(o => o.TableInfo).FirstOrDefaultAsync(o => o.Id == bill.OrderId);
            if (order != null)
            {
                if (bill.PaymentStatus == PaymentStatus.Paid)
                {
                    if (order.TableInfo != null)
                    {
                        order.TableInfo.Status = TableStatus.Free;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = bill.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Order)
                    .ThenInclude(o => o!.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                .Include(b => b.Order)
                    .ThenInclude(o => o!.TableInfo)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null) return NotFound();

            return View(bill);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Order)
                    .ThenInclude(o => o!.TableInfo)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (bill != null && bill.PaymentStatus == PaymentStatus.Unpaid)
            {
                bill.PaymentStatus = PaymentStatus.Paid;
                if (bill.Order?.TableInfo != null)
                {
                    bill.Order.TableInfo.Status = TableStatus.Free;
                }
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
