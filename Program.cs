using Alunos.Services;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Backend_Vestetec_App.Services;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = ServerVersion.Parse("10.4.32-mariadb");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AloowAll", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });

});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<IEnviarEmail, MailjetEmailService>();
builder.Services.AddScoped<IVerificacaoService, VerificacaoService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<ILoginInterface, LoginService>();
builder.Services.AddScoped<IAlunoInterface, AlunoService>();
builder.Services.AddScoped<IEscolaService, EscolaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IModeloService, ModeloService>();
builder.Services.AddScoped<ITecidoService, TecidoService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Converte PascalCase (C#) para camelCase (JavaScript)
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    });

    builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5260); // Aceita em qualquer IP
});



// NOVA CONFIGURA��O: Configura o tamanho m�ximo para upload de arquivos
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 3 * 1024 * 1024;
});

// NOVA CONFIGURA��O: Para servidores Kestrel
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 3 * 1024 * 1024; 
});

// NOVA CONFIGURA��O: Configura as op��es de formul�rio para aceitar arquivos maiores
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 3 * 1024 * 1024; 
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});



var app = builder.Build();


app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Adicionar headers CORS para imagens
        ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Add("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
    }
});

// IMPORTANTE: Certificar-se de que a pasta wwwroot/uploads/produtos existe
// Voc� pode adicionar esta verifica��o no startup da aplica��o:
var uploadsPath = Path.Combine(app.Environment.WebRootPath ??
    Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads", "produtos");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Console.WriteLine($"Diret�rio criado: {uploadsPath}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AloowAll");

app.MapControllers();

app.UseAuthentication();

app.UseAuthorization();

app.Run();


