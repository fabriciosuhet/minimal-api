using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Entities.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.infrastructure.Db;
using minimal_api.infrastructure.Interfaces;
using MinimalApi.DTOs;

#region Builder

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters= new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddDbContext<DataBaseContext>(opt => 
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"));
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }

    });
});


var app = builder.Build();

#endregion 

#region  App

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Admin

string GerarTokenJwt(Admin? admin)
{   
    if (string.IsNullOrEmpty(key)) return string.Empty;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new("Email", admin.Email),
        new("Perfil", admin.Perfil),
        new(ClaimTypes.Role, admin.Perfil),
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/admin/login", ([FromBody] LoginDTO loginDTO, IAdminService adminService) => {
    var adm = adminService.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdminLogadoModelView 
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Admin");

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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Admin");

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
    
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Admin");

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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Admin");
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
    
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
.WithTags("Vehicles");

app.MapGet("/vehicles", ([FromQuery] int pagina, IVehicleService vehicleService) => {

    var vehicles = vehicleService.Todos(pagina);
    return Results.Ok(vehicles);
    
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
.WithTags("Vehicles");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) => {

    var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(vehicle);
    
}).RequireAuthorization().WithTags("Vehicles");

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
    
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
.WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) => {

   var vehicle = vehicleService.GetById(id);
    if (vehicle == null)
    {
        return Results.NotFound();
    }

    vehicleService.Update(vehicle);

    return Results.NoContent();
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Vehicles");
#endregion

app.Run();
