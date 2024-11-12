using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PostgreDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API списка контактов",
    });
});
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IStorage, InMemoryStorage>();

// Настройка CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy"); 

app.MapControllers();
app.MapHub<DownloadNotificationHub>("/downloadNotificationHub");

app.Run();