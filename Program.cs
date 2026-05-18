using Microsoft.EntityFrameworkCore; //biblioteca para trabalhar com entity do bd

var builder = WebApplication.CreateBuilder(args);

string connStr = "Server=ISABITTINELLI\\SQLEXPRESS;Database=BDLalaKids;Trusted_Connection=True;TrustServerCertificate=True";

builder.Services
    .AddDbContext<DatabaseContext>(opt => opt.UseSqlServer(connStr));

builder.Services.AddControllersWithViews();

builder.Services.AddSession(); //parte de usuarios

var app = builder.Build();

app.UseSession();//parte de usuarios

app.MapControllerRoute("default", "{controller=Produto}/{action=Index}/{id?}");

app.Run();
