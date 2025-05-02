using IBanKing.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IBanKing.Pages.Client
{
    public class TermsAndConditionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TermsAndConditionsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.TermsAndConditions Terms { get; set; }

        public async Task OnGetAsync()
        {
            Terms = await _context.TermsAndConditions.FirstOrDefaultAsync();
        }
    }
}