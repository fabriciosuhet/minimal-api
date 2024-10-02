using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Entities.ModelViews;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Services;
using minimal_api.infrastructure.Db;
using minimal_api.infrastructure.Interfaces;
using MinimalApi.DTOs;

#region Builder

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataBaseContext>(opt => 
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#endregion 

#region  App

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Admin
app.MapPost("/admin/login", ([FromBody] LoginDTO loginDTO, IAdminService adminService) => {
    if (adminService.Login(loginDTO) != null) 
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
}).WithTags("Admin");

app.MapGet("/admins", ([FromQuery] int? pagina, IAdminService adminService) => {
    var adms = new List<AdminModelViewer>();
    var admins = adminService.Todos(pagina);
    foreach (var adm in admins)
    {
        adms.Add(new AdminModelViewer
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).WithTags("Admin");

app.MapGet("/Admin/{id}", ([FromRoute] int id, IAdminService adminService) => {

    var admin = adminService.GetById(id);
    if (admin == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new AdminModelViewer
        {
            Id = admin.Id,
            Email = admin.Email,
            Perfil = admin.Perfil
        });
    
}).WithTags("Admin");

app.MapPost("/admin", ([FromBody] AdminDTO adminDTO, IAdminService adminService) => {
    var validation = new ErrorsValidation
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDTO.Email))
    {
        validation.Messages.Add("E-mail não pode ser vázio!");
    }
    if (string.IsNullOrEmpty(adminDTO.Senha))
    {
        validation.Messages.Add("Senha não pode ser vázia!");
    }
    if (adminDTO.Perfil == null)
    {
        validation.Messages.Add("Perfil não pode ser vázio!");
    }

    if (validation.Messages.Count > 0)
    {
        return Results.BadRequest(validation);
    }

    var admin = new Admin
    {
        Email = adminDTO.Email,
        Senha = adminDTO.Senha,
        Perfil = adminDTO.Perfil.ToString(),
    };
    adminService.Include(admin);

    return Results.Created($"/admin/{admin.Id}", new AdminModelViewer
        {
            Id = admin.Id,
            Email = admin.Email,
            Perfil = admin.Perfil
        });
}).WithTags("Admin");
#endregion

#region  Vehicle

ErrorsValidation validaDTO(VehicleDTO vehicleDTO)
{
    var validation = new ErrorsValidation
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(vehicleDTO.Nome))
    {
        validation.Messages.Add("O NOME não pode ser vázio!");
    }

    if (string.IsNullOrEmpty(vehicleDTO.Marca))
    {
        validation.Messages.Add("A MARCA não pode ser vázia!");
    }

    if (vehicleDTO.Ano < 1950)
    {
        validation.Messages.Add("Veículo muito antigo, só aceita veiculos superiores a 1950!");
    }

    return validation;
    
}

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) => {

    var validation = validaDTO(vehicleDTO);
    if (validation.Messages.Count > 0)
    {
        return Results.BadRequest(validation);
    }

    var vehicle = new Vehicle
    {
        Nome = vehicleDTO.Nome,
        Marca = vehicleDTO.Marca,
        Ano = vehicleDTO.Ano,
    };
    vehicleService.Include(vehicle);

    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
    
}).WithTags("Vehicles");

app.MapGet("/vehicles", ([FromQuery] int pagina, IVehicleService vehicleService) => {

    var vehicles = vehicleService.Todos(pagina);
    return Results.Ok(vehicles);
    
}).WithTags("Vehicles");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) => {

    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(vehicle);
    
}).WithTags("Vehicles");

app.MapPut("/vehicles", ([FromQuery] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) => {

    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
    {
        return Results.NotFound();
    }

    var validation = validaDTO(vehicleDTO);
    if (validation.Messages.Count > 0)
    {
        return Results.BadRequest(validation);
    }
   
    vehicle.Nome = vehicleDTO.Nome;
    vehicle.Marca = vehicleDTO.Marca;
    vehicle.Ano = vehicleDTO.Ano; 

    vehicleService.Update(vehicle);

    return Results.Ok(vehicle);
    
}).WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) => {

   var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
    {
        return Results.NotFound();
    }

    vehicleService.Update(vehicle);

    return Results.NoContent();
}).WithTags("Vehicles");
#endregion

app.Run();
