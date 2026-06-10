using API.Context;
using APIBarbearia.Models;
using APIBarbearia.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is required in configuration.");
var runLegacyPasswordMigration = builder.Configuration.GetValue<bool>("Security:LegacyPasswordMigration:Enabled");
var legacyPasswordMigrationBatchSize = Math.Max(1, builder.Configuration.GetValue<int?>("Security:LegacyPasswordMigration:BatchSize") ?? 500);

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
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Muitas tentativas. Tente novamente em alguns minutos."
        }, cancellationToken: token);
    };

    options.AddFixedWindowLimiter("login", config =>
    {
        config.PermitLimit = 10;
        config.Window = TimeSpan.FromMinutes(15);
        config.QueueLimit = 0;
        config.AutoReplenishment = true;
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) &&
                    context.Request.Cookies.TryGetValue("auth_token", out var token) &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

var isRender =
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RENDER")) ||
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL"));

if (runLegacyPasswordMigration)
{
    try
    {
        await MigrateLegacyPasswordsAsync(app.Services, app.Logger, legacyPasswordMigrationBatchSize);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Legacy password migration failed during startup.");
    }
}

var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsRoot))
{
    Directory.CreateDirectory(uploadsRoot);
}

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
// Em plataformas com TLS terminado no proxy (Render), evita loop/avisos de redirecionamento.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsEnvironment("Docker") && !isRender)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";
    await next();
});

// Middleware para aplicar política CORS
app.UseCors("AllowFrontend");

app.UseRateLimiter();

// Expor arquivos enviados (ex.: fotos de profissionais) em /uploads/*
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

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

static async Task MigrateLegacyPasswordsAsync(IServiceProvider services, ILogger logger, int batchSize)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<APIDbContext>();

    int totalMigrated = 0;
    while (true)
    {
        var users = await db.Usuarios
            .Where(u => !string.IsNullOrWhiteSpace(u.Senha) && !u.Senha.StartsWith("PBKDF2$"))
            .OrderBy(u => u.UsuarioId)
            .Take(batchSize)
            .ToListAsync();

        if (users.Count == 0)
        {
            break;
        }

        foreach (var user in users)
        {
            user.Senha = PasswordService.HashPassword(user.Senha);
        }

        await db.SaveChangesAsync();
        totalMigrated += users.Count;
        db.ChangeTracker.Clear();
    }

    if (totalMigrated > 0)
    {
        logger.LogInformation("Legacy password migration completed. Users migrated: {Count}", totalMigrated);
    }
    else
    {
        logger.LogInformation("Legacy password migration enabled, but no legacy passwords were found.");
    }
}