using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Services;
using minimal_api.infrastructure.Db;
using minimal_api.infrastructure.Interfaces;
using MinimalApi.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataBaseContext>(opt => 
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

builder.Services.AddScoped<IAdminService, AdminService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdminService adminService) => {
    if (adminService.Login(loginDTO) != null) 
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});

app.Run();
