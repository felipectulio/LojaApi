using LojaApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== CONFIGURAÇÃO DO CORS ADICIONADA AQUI ==================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy => policy.WithOrigins("http://localhost:3000") // Permite o seu React acessar a API
                        .AllowAnyMethod()                     // Permite GET, POST, PUT, DELETE
                        .AllowAnyHeader());                    // Permite cabeçalhos como Authorization
});
// =========================================================================

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
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
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

// A criação automática do banco deve rodar logo no início do app
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated(); 
}

// Configurações globais do servidor de requisições
app.UseSwagger();
app.UseSwaggerUI();

// 🚨 IMPORTANTE: A ORDEM DOS PROXIMOS 3 COMANDOS NAO PODE SER ALTERADA!
app.UseCors("AllowReact");       // 1º Libera o acesso para o navegador
app.UseAuthentication();        // 2º Verifica quem é o usuário (Token)
app.UseAuthorization();         // 3º Verifica as permissões dele

app.MapControllers();
app.Run();