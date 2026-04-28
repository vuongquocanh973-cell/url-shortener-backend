using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký các dịch vụ (Services)
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UrlShortenerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 2. Xây dựng ứng dụng (Build app)
var app = builder.Build();

// 3. Tự động tạo Database khi khởi động (Phải nằm SAU builder.Build())
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Lệnh này sẽ tạo Database và các bảng nếu chưa tồn tại
        context.Database.Migrate(); 
        Console.WriteLine("---- Database đã sẵn sàng! ----");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"---- Lỗi khi tạo DB: {ex.Message} ----");
    }
}

// 4. Cấu hình HTTP request pipeline (Middleware)
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 5. Chạy ứng dụng
app.Run();