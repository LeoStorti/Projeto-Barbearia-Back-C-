using API.Context;
using APIBarbearia.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<APIDbContext>(options =>
    options.UseSqlServer(connection));

// Configuração dos serviços MVC
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Certifique-se de usar https:// se for o caso
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Configuração da autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Middleware para redirecionamento HTTPS
app.UseHttpsRedirection();

// Middleware para aplicar política CORS
app.UseCors("AllowFrontend");

// Middleware de autenticação
app.UseAuthentication();

// Middleware de autorização
app.UseAuthorization();

// Middleware de desenvolvimento para Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.InjectStylesheet("/swagger-ui/custom.css");
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
}

// Middleware para mapear controllers
app.MapControllers();

app.Run();
