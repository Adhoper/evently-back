using Evently.Api.Data;
using Evently.Api.Services;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EventlyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingrese el token JWT."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] =
                new List<string>()
        });
});

builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IEventService,
    EventService>();

builder.Services.AddScoped<
    ITokenService,
    TokenService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IUserService,
    UserService>();

builder.Services.AddScoped<
    ITicketService,
    TicketService>();

builder.Services.AddScoped<
    IOrganizerService,
    OrganizerService>();

builder.Services.AddScoped<
    IEventImageService,
    EventImageService>();

builder.Services.AddScoped<
    IAdminService,
    AdminService>();

builder.Services.AddHttpClient<
    IEmailService,
    BrevoEmailService>(
        client =>
        {
            client.BaseAddress =
                new Uri(
                    "https://api.brevo.com/v3/");
        });

var jwtSettings =
    builder.Configuration.GetSection("Jwt");

var jwtKey =
    jwtSettings["Key"]
    ?? throw new InvalidOperationException(
        "JWT Key no está configurada.");

var frontendUrl =
    builder.Configuration["Frontend:BaseUrl"]
        ?.TrimEnd('/');

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings["Issuer"],

                ValidAudience =
                    jwtSettings["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey)),

                ClockSkew =
                    TimeSpan.Zero
            };

        options.Events =
            new JwtBearerEvents
            {
                OnTokenValidated =
                    async context =>
                    {
                        var userIdValue =
                            context.Principal?
                                .FindFirst(
                                    ClaimTypes
                                        .NameIdentifier)?
                                .Value;

                        var tokenRole =
                            context.Principal?
                                .FindFirst(
                                    ClaimTypes.Role)?
                                .Value;

                        if (!int.TryParse(
                                userIdValue,
                                out var userId))
                        {
                            context.Fail(
                                "Token inválido.");

                            return;
                        }

                        var dbContext =
                            context.HttpContext
                                .RequestServices
                                .GetRequiredService<
                                    EventlyDbContext>();

                        var user =
                            await dbContext.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(
                                    u =>
                                        u.Id ==
                                        userId);

                        if (
                            user is null ||
                            !user.IsActive ||
                            !string.Equals(
                                user.Role.ToString(),
                                tokenRole,
                                StringComparison.Ordinal))
                        {
                            context.Fail(
                                "La sesión ya no es válida.");
                        }
                    }
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            var allowedOrigins =
                new List<string>
                {
                    "http://localhost:5173"
                };

            if (!string.IsNullOrWhiteSpace(
                    frontendUrl))
            {
                allowedOrigins.Add(
                    frontendUrl);
            }

            policy
                .WithOrigins(
                    allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app =
    builder.Build();

var webRootPath =
    Path.Combine(
        app.Environment.ContentRootPath,
        "wwwroot");

var eventUploadsPath =
    Path.Combine(
        webRootPath,
        "uploads",
        "events");

Directory.CreateDirectory(
    eventUploadsPath);

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Evently API v1");
});

app.UseHttpsRedirection();

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider =
            new PhysicalFileProvider(
                webRootPath),

        RequestPath = ""
    });

app.UseCors(
    "Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/",
    () =>
        Results.Redirect(
            "/swagger"));

app.Run();