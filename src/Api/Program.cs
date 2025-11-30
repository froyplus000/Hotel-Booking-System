using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
