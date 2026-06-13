using LojaApi.Data;
using LojaApi.Models; // Adicionado para reconhecer os modelos do banco
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ================== CONFIGURAÇÃO DO CORS ==================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy => 
        policy.WithOrigins("http://localhost:3000") // Permite o React acessar a API
              .AllowAnyMethod()                     // Permite GET, POST, PUT, DELETE
              .AllowAnyHeader());                   // Permite cabeçalhos como Authorization
});

// CONFIGURAÇÃO DO BANCO DE DADOS (Mantendo seu MySQL original)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Chave secreta do token JWT
var chaveJwt = "MINHA_CHAVE_SUPER_SECRETA_123456";
var key = Encoding.UTF8.GetBytes(chaveJwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// =========================================================================
// FORÇAR RECRAÇÃO DO BANCO DE DADOS (ALTERAÇÃO REALIZADA AQUI)
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Deleta o banco antigo para limpar inconsistências de tabelas faltantes
    context.Database.EnsureDeleted(); 
    
    // Cria o banco novamente com todas as tabelas mapeadas no AppDbContext
    context.Database.EnsureCreated(); 
}
// =========================================================================

app.UseSwagger();
app.UseSwaggerUI();

// 🚨 ORDEM OBRIGATÓRIA DOS MIDDLEWARES
app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();

// -------------------------------------------------------------------------
// ROTA: POST /login (Mapeada diretamente aqui para responder ao seu React)
// -------------------------------------------------------------------------
app.MapPost("/login", (LoginDTO login) =>
{
    // Validação aceitando as credenciais com letras maiúsculas ou minúsculas
    if ((login.Email?.ToLower() == "admin@email.com" && login.Senha == "123456") || 
        (login.Email?.ToLower() == "admin" && login.Senha == "admin"))
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login.Email) }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Results.Ok(new { token = tokenHandler.WriteToken(token) });
    }

    return Results.Unauthorized();
});

// Rota extra para o caso do seu React ainda chamar o caminho antigo
app.MapPost("/Auth/login", (LoginDTO login) => Results.Redirect("/login", permanent: false));

// -------------------------------------------------------------------------
// ROTAS DO CRUD DE SALAS (Usando o modelo do namespace LojaApi.Models)
// -------------------------------------------------------------------------
app.MapGet("/salas", [Authorize] async (AppDbContext db) => 
    Results.Ok(await db.SalasReuniao.ToListAsync()));

app.MapPost("/salas", [Authorize] async (SalaReuniao sala, AppDbContext db) =>
{
    db.SalasReuniao.Add(sala);
    await db.SaveChangesAsync();
    return Results.Created($"/salas/{sala.Id}", sala);
});

app.MapPut("/salas/{id:int}", [Authorize] async (int id, SalaReuniao salaAtualizada, AppDbContext db) =>
{
    var sala = await db.SalasReuniao.FindAsync(id);
    if (sala is null) return Results.NotFound();

    sala.Nome = salaAtualizada.Nome;
    sala.Capacidade = salaAtualizada.Capacidade;
    sala.PossuiProjetor = salaAtualizada.PossuiProjetor;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/salas/{id:int}", [Authorize] async (int id, AppDbContext db) =>
{
    var sala = await db.SalasReuniao.FindAsync(id);
    if (sala is null) return Results.NotFound();

    db.SalasReuniao.Remove(sala);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// -------------------------------------------------------------------------
// DTO DO LOGIN (Mapeia o Email e Senha enviados pelo Axios do React)
// -------------------------------------------------------------------------
public record LoginDTO(string Email, string Senha);