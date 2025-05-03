using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace IBanKing.Pages.Client
{
    public class FAQModel : PageModel
    {
        public List<FAQItem> FAQs { get; set; } = new();

        public void OnGet()
        {
            FAQs = new List<FAQItem>
            {
                new FAQItem { Id = 1, Question = "How do I create a bank account?", Answer = "Go to 'Add Bank Account' in your dashboard, select the currency and submit." },
                new FAQItem { Id = 2, Question = "How can I reset my password?", Answer = "Click 'Account' on the sidebar menu  and follow the instructions." },
                new FAQItem { Id = 3, Question = "How long does a transaction take?", Answer = "Transactions require approval and can take up until the bank employee accept." },
                new FAQItem { Id = 4, Question = "Can I have multiple accounts in the same currency?", Answer = "Yes. IBanKing allows you to open multiple accounts in any supported currency." },
                new FAQItem { Id = 5, Question = "What happens if I exceed my transaction limit?", Answer = "You will receive an error, and the transaction will not be submitted. Try again the next day or adjust your limits with support." }
            };
        }
        public class FAQItem
        {
            public int Id { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
        }
    }
}
