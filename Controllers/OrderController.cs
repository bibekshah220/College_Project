using CafeManagementSystem.Data;
using CafeManagementSystem.Hubs;
using CafeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CafeManagementSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<KitchenHub> _hubContext;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<KitchenHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.TableInfo)
                .Include(o => o.Waiter)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        public IActionResult Create()
        {
            var freeTables = _context.Tables.Where(t => t.Status == TableStatus.Free).ToList();
            ViewData["TableInfoId"] = new SelectList(freeTables, "Id", "TableNumber");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TableInfoId")] Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            order.WaiterId = user.Id;
            order.Status = OrderStatus.Pending;
            order.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                var table = await _context.Tables.FindAsync(order.TableInfoId);
                if (table != null) table.Status = TableStatus.Occupied;

                _context.Add(order);
                await _context.SaveChangesAsync();

                // Notify kitchen of new order
                await _hubContext.Clients.All.SendAsync("ReceiveOrder", order.Id, table?.TableNumber ?? "Unknown");

                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            ViewData["TableInfoId"] = new SelectList(_context.Tables.Where(t => t.Status == TableStatus.Free), "Id", "TableNumber", order.TableInfoId);
            return View(order);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.TableInfo)
                .Include(o => o.Waiter)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            ViewData["MenuItems"] = new SelectList(_context.MenuItems.Where(m => m.IsAvailable), "Id", "Name");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int orderId, int menuItemId, int quantity)
        {
            var order = await _context.Orders.FindAsync(orderId);
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);

            if (order != null && menuItem != null && quantity > 0)
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    MenuItemId = menuItemId,
                    Quantity = quantity,
                    UnitPrice = menuItem.Price
                };
                
                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                // Notify Kitchen of new item
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", orderId);
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                
                // Real-time update
                await _hubContext.Clients.All.SendAsync("OrderStatusChanged", id, status.ToString());
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
