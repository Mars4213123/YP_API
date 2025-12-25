using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using YP_API.Data;
using YP_API.Interfaces;
using YP_API.Models;
using YP_API.Repositories;
using YP_API.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS настройки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Логгирование
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Recipe Planner API",
        Version = "v1",
        Description = "API for managing recipes, menus and shopping lists"
    });

    c.CustomSchemaIds(x => x.FullName);
});

var connectionString = "Server=MySQL-8.2;Port=3306;Database=recipe_planner;Uid=root;Pwd=;";

builder.Services.AddDbContext<RecipePlannerContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();

builder.Services.AddScoped<IRepository<ShoppingList>, Repository<ShoppingList>>();
builder.Services.AddScoped<IRepository<ShoppingListItem>, Repository<ShoppingListItem>>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

builder.Services.AddScoped<IRepository<UserInventory>, Repository<UserInventory>>();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe Planner API v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Recipe Planner API Documentation";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal server error",
            message = ex.Message,
            details = ex.StackTrace
        });
    }
});

app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RecipePlannerContext>();

    var canConnect = await context.Database.CanConnectAsync();

    if (canConnect)
    {
        var userCount = await context.Users.CountAsync();

        var recipeCount = await context.Recipes.CountAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database check failed: {ex.Message}");
    Console.WriteLine($"Please check:");
    Console.WriteLine($"1. MySQL server is running (net start mysql)");
    Console.WriteLine($"2. Database 'recipe_planner' exists");
    Console.WriteLine($"3. User 'root' has no password (or change connection string)");
    Console.WriteLine($"4. Port 3306 is not blocked by firewall");
}

Console.WriteLine("Application started successfully");
Console.WriteLine($"Swagger available at: {app.Urls.FirstOrDefault()}");

app.Run();