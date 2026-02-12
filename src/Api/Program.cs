using HotelBooking.Domain.Repositories;
using HotelBooking.Infrastructure.Persistence;
using HotelBooking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
    
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Register the DbContext
// "Use PostgreSQL with this connection string"
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository Registration
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add MediatR - this scans for all handlers in Application assembly
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(HotelBooking.Application.AssemblyReference).Assembly));

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
