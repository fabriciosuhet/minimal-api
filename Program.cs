using Microsoft.EntityFrameworkCore;
using minimal_api.infrastructure.Db;
using MinimalApi.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<DataBaseContext>

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

builder.Services.AddDbContext<DataBaseContext>(opt => 
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/login", (LoginDTO loginDTO) => {
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "123456") 
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});

app.Run();
