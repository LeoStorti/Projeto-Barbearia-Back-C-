using API.Context;
using APIBarbearia.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<APIDbContext>(options =>
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

// Configuração dos serviços MVC
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// (NOVO) ProblemDetails - ajuda a não devolver 500 "mudo" em DEV
builder.Services.AddProblemDetails(options =>
{
    // Em produção, não expor detalhes internos
    options.CustomizeProblemDetails = ctx =>
    {
        // Inclui o traceId sempre
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;

        // Inclui o correlationId se existir
        var cid = ctx.HttpContext.Request.Headers["X-Correlation-Id"].ToString();
        if (!string.IsNullOrWhiteSpace(cid))
            ctx.ProblemDetails.Extensions["correlationId"] = cid;
    };
});

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:4200" };

            policyBuilder.WithOrigins(allowedOrigins)
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

// (NOVO) Middleware de CorrelationId: propaga X-Correlation-Id e devolve no response
app.Use(async (context, next) =>
{
    var cid = context.Request.Headers["X-Correlation-Id"].ToString();
    if (!string.IsNullOrWhiteSpace(cid))
    {
        context.Response.Headers["X-Correlation-Id"] = cid;
    }

    await next();
});

// (NOVO) Exception handler + ProblemDetails
// Em DEV: retorna body com problem+json (com detalhe), em vez de 500 vazio.
// Em PROD: retorna ProblemDetails sem detalhes internos.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

        var pd = new ProblemDetails
        {
            Title = "Erro interno no servidor",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path
        };

        // sempre inclui ids
        pd.Extensions["traceId"] = context.TraceIdentifier;
        var cid = context.Request.Headers["X-Correlation-Id"].ToString();
        if (!string.IsNullOrWhiteSpace(cid))
            pd.Extensions["correlationId"] = cid;

        if (env.IsDevelopment())
        {
            // detalhe genérico (a exception detalhada já fica no log)
            pd.Detail = "Ocorreu uma exceção no servidor. Verifique os logs do backend usando o correlationId/traceId.";
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(pd);
    });
});

// Middleware para redirecionamento HTTPS
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

// Middleware para aplicar política CORS
app.UseCors("AllowFrontend");

// Middleware de autenticação
app.UseAuthentication();

// Middleware de autorização
app.UseAuthorization();

// Middleware de desenvolvimento para Swagger
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
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