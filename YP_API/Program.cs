using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Repositories;
using YP_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавьте логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger конфигурация
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Recipe Planner API",
        Version = "v1",
        Description = "API for managing recipes, menus and shopping lists",
        Contact = new OpenApiContact
        {
            Name = "Recipe Planner Team",
            Email = "support@recipeplanner.com"
        }
    });

    // JWT Authentication в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Игнорировать циклические ссылки для Swagger
    c.CustomSchemaIds(x => x.FullName);
});

// База данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=recipe_planner;Uid=root;Pwd=;";

builder.Services.AddDbContext<RecipePlannerContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

// Репозитории
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();

// Сервисы
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

// JWT Authentication
var jwtKey = builder.Configuration["JWT:Key"] ?? "fallback-super-secret-key-for-development-1234567890";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe Planner API v1");
        c.RoutePrefix = string.Empty; // Чтобы Swagger открывался на корневом URL
        c.DocumentTitle = "Recipe Planner API Documentation";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Глобальный обработчик ошибок
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal server error",
            message = ex.Message
        });
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Проверка базы данных при запуске
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RecipePlannerContext>();

    Console.WriteLine("Testing database connection...");
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"Database connected: {canConnect}");

    if (canConnect)
    {
        var userCount = await context.Users.CountAsync();
        Console.WriteLine($"Users in database: {userCount}");

        var recipeCount = await context.Recipes.CountAsync();
        Console.WriteLine($"Recipes in database: {recipeCount}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database check failed: {ex.Message}");
}

Console.WriteLine("Application started successfully");
Console.WriteLine($"Swagger available at: {app.Urls.FirstOrDefault()}");
app.Run();