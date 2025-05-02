using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IBanKing.Pages.BankEmployee
{
    public class ManageTransactionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageTransactionsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Transaction> PendingTransactions { get; set; } = new();
        public List<Transaction> AllTransactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            PendingTransactions = _context.Transactions
                .Where(t => t.Status == "Pending")
                .OrderByDescending(t => t.DateTime)
                .ToList();

            AllTransactions = _context.Transactions
                .OrderByDescending(t => t.DateTime)
                .ToList();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null) return NotFound();

            var sender = _context.Accounts.FirstOrDefault(a => a.IBAN == transaction.Sender);
            var receiver = _context.Accounts.FirstOrDefault(a => a.IBAN == transaction.Receiver);

            if (sender == null || receiver == null)
                return BadRequest("Invalid accounts.");

            decimal amount = (decimal)transaction.Amount;

            if (sender.Balance < amount)
            {
                transaction.Status = "Rejected";
                _context.Update(transaction);
                await _context.SaveChangesAsync();
                return RedirectToPage();
            }

            sender.Balance -= amount;
            receiver.Balance += amount;
            transaction.Status = "Completed";

            _context.Update(sender);
            _context.Update(receiver);
            _context.Update(transaction);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null) return NotFound();

            transaction.Status = "Rejected";
            _context.Update(transaction);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
