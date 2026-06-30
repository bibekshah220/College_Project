using CafeManagementSystem.Data;
using CafeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagementSystem.Controllers
{
    [Authorize]
    public class TableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TableController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tables = await _context.Tables
                .Include(t => t.Orders.Where(o => o.Status != OrderStatus.Cancelled && o.Bill == null))
                .OrderBy(t => t.TableNumber)
                .ToListAsync();
            return View(tables);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TableNumber,Capacity,Status")] TableInfo tableInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tableInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tableInfo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, TableStatus status)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table != null)
            {
                table.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
