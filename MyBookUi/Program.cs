using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBookUi.Data;
using MyDataAccess;
using MyUtility.BookRepository;
using MyUtility.CartRepository;
using MyUtility.FileService;
using MyUtility.GenreRepository;
using MyUtility.HomeRepository;
using MyUtility.ReportRepository;
using MyUtility.StockRepository;
using MyUtility.UserOrderRepository;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("MyCon") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddTransient<IHomeRepository, HomeRepository>();
builder.Services.AddTransient<ICartRepository, CartRepository>();
builder.Services.AddTransient<IUserOrderRepository, UserOrderRepository>();
builder.Services.AddTransient<IStockRepository, StockRepository>();
builder.Services.AddTransient<IGenreRepository, GenreRepository>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IReportRepository, ReportRepository>();
var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    await DbSeeder.SeedDefaultData(scope.ServiceProvider);
//}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
