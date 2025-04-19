using IBanKing.Data;
using IBanKing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace IBanKing.Pages.UserFeedback
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public FeedbackInputModel Input { get; set; }

        public bool Success { get; set; } = false;

        public class FeedbackInputModel
        {
           
            public string Message { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                // Optional: Redirect to login or show an error if session is missing
                return RedirectToPage("/Login/Index");
            }

            var feedback = new IBanKing.Models.Feedback
            {
                Message = Input.Message,
                DateTime = DateTime.Now,
                UserId = userId
            };

            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            Success = true;

            ModelState.Clear(); // Clear input field
            return Page();
        }

    }
}
