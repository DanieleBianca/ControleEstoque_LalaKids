using Microsoft.EntityFrameworkCore; //biblioteca para trabalhar com entity do bd

var builder = WebApplication.CreateBuilder(args);

string connStr = "Server=ISABITTINELLI\\SQLEXPRESS;Database=BDLalaKids;Trusted_Connection=True;TrustServerCertificate=True";

builder.Services
    .AddDbContext<DatabaseContext>(opt => opt.UseSqlServer(connStr));

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapControllerRoute("default", "{controller=Produto}/{action=Index}/{id?}");

app.Run();