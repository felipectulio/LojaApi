using LojaApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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

// Configuração do Swagger corrigida para exibir o botão Authorize (Cadeado)
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Digite 'Bearer' [espaço] e o seu token.\n\nExemplo: Bearer eyJhbGciOi..."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, // <--- Correção aqui!
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// Chave secreta do token (Mantenha a mesma no Controller)
var chaveJwt = "MINHA_CHAVE_SUPER_SECRETA_123456";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication(); // OBRIGATÓRIO: Verifica quem é o usuário
app.UseAuthorization();  // OBRIGATÓRIO: Verifica o que ele pode fazer
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