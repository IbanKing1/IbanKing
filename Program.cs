using IBanKing.Data;
using IBanKing.Repositories;
using IBanKing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
// EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session (set to expire after 1 minute of inactivity)
builder.Services.AddSession(options =>
{
   // options.IdleTimeout = TimeSpan.FromMinutes(10); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DI for Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // IMPORTANT - must come before authorization
app.UseAuthorization();

app.MapRazorPages();

//Seed test BankEmployee user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var existingUser = context.Users.FirstOrDefault(u => u.Email == "nadolur152@gmail.com");
    if (existingUser != null)
    {
        context.Users.Remove(existingUser);
        context.SaveChanges();
    }

    var hashedPassword = IBanKing.Utils.PasswordHelper.HashPassword("Parola123");

    context.Users.Add(new IBanKing.Models.User
    {
        Name = "Radu",
        Email = "nadolur152@gmail.com",
        Password = hashedPassword,
        Role = "Client",
        DateBirth = new DateTime(2003, 06, 08),
        Address = "Str. Independentei 14",
        Gender = "M",
        PhoneNumber = "0726454521",
        IsBlocked = false,
        FailedLoginAttempts = 0
    });

    context.SaveChanges();
}

// Redirect root to /Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Login");
    return Task.CompletedTask;
});

app.Run();
