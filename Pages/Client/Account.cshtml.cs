using IBanKing.Data;
using IBanKing.Models;
using IBanKing.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using IBanKing.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace IBanKing.Pages.Client
{
    public class AccountModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountModel(ApplicationDbContext context, IEmailService emailService, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _emailService = emailService;
            _hostingEnvironment = hostingEnvironment;
        }

        [BindProperty]
        public EditInputModel Input { get; set; }

        [BindProperty]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".gif", ".jpeg" })]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ProfilePicture { get; set; }

        public string ProfilePath { get; set; }
        public bool Success { get; set; }

        public class EditInputModel
        {
            [EmailAddress]
            public string? Email { get; set; }
            public string? Address { get; set; }
            [RegularExpression(@"^\d{7,15}$", ErrorMessage = "Phone number must contain only digits (7-15 digits).")]
            public string? PhoneNumber { get; set; }
            [DataType(DataType.Password)]
            public string? CurrentPassword { get; set; }
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 8)]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}:;'<>,.?\/~`-]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one digit, and one special character.")]
            public string? NewPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            ProfilePath = user.ProfilePicturePath;
            Input = new EditInputModel
            {
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ProfilePath = user.ProfilePicturePath;
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.Email) && Input.Email != user.Email)
            {
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == Input.Email && u.UserId != userId);
                if (emailExists)
                {
                    ModelState.AddModelError("Input.Email", "This email is already in use.");
                    ProfilePath = user.ProfilePicturePath;
                    return Page();
                }
                user.Email = Input.Email;
            }

            if (!string.IsNullOrWhiteSpace(Input.Address))
            {
                user.Address = Input.Address;
            }

            if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                user.PhoneNumber = Input.PhoneNumber;
            }

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ProfilePicturePath) && user.ProfilePicturePath != "user.png")
                {
                    var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "profile-pictures", user.ProfilePicturePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var fileName = GetUniqueFileName(ProfilePicture.FileName);
                var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "profile-pictures");
                Directory.CreateDirectory(uploads);
                var filePath = Path.Combine(uploads, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(fileStream);
                }

                user.ProfilePicturePath = fileName;
                HttpContext.Session.SetString("ProfilePicturePath", user.ProfilePicturePath);
            }

            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
                {
                    ModelState.AddModelError("Input.CurrentPassword", "You must enter your current password to set a new one.");
                    ProfilePath = user.ProfilePicturePath;
                    return Page();
                }

                if (!PasswordHelper.VerifyPassword(Input.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("Input.CurrentPassword", "Current password is incorrect.");
                    ProfilePath = user.ProfilePicturePath;
                    return Page();
                }

                user.Password = PasswordHelper.HashPassword(Input.NewPassword);
                await _emailService.SendPasswordChangeEmailAsync(user.Email, user.Name, DateTime.Now);
            }

            await _context.SaveChangesAsync();
            Success = true;
            ProfilePath = user.ProfilePicturePath;

            return Page();
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName) + "_" + Guid.NewGuid().ToString().Substring(0, 4) + Path.GetExtension(fileName);
        }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions) { _extensions = extensions; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult($"Only {string.Join(", ", _extensions)} files are allowed!");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize) { _maxFileSize = maxFileSize; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"Maximum allowed file size is {_maxFileSize / 1024 / 1024}MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}