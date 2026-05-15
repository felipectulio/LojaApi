using LojaApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================== COLE ESTE BLOCO AQUI ==================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Ele lê suas Models e cria as tabelas direto no MySQL se elas não existirem!
    context.Database.EnsureCreated(); 
}
// ==========================================================

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();