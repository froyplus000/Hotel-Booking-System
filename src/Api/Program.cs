using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Register the DbContext
// "Use PostgreSQL with this connection string"
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// Keep Swagger always on for learning purpose. Will turn off later when working with production on AWS
app.UseSwagger();
app.UseSwaggerUI();

// Health check endpoint 
app.MapGet("/health", () => Results.Ok(new {status = "ok"}));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
