using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Notaion.Configurations;
using Notaion.Context;
using Notaion.Hubs;
using Notaion.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
            builder => builder
                .SetIsOriginAllowed(_ => true) // cho phép tất cả các port
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
});

//// Cấu hình tài khoản Cloudinary
var configuration = builder.Configuration;

var cloudName = configuration["Cloudinary:CloudName"];
var apiKey = configuration["Cloudinary:ApiKey"];
var apiSecret = configuration["Cloudinary:ApiSecret"];

var cloudinaryAccount = new Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.Configure<CloudinaryService>(builder.Configuration.GetSection("Cloudinary"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<ChatHub>("/chatHub");

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.UseCors("AllowAllOrigins");

app.Run();

