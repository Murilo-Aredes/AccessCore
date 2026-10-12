using System.Text;
using AuthService.Api.Seguranca;
using AuthService.Application;
using AuthService.Application.Common;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContextoCorrelacao, ContextoCorrelacao>();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtualHttpContext>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtConfiguracao = builder.Configuration.GetSection(JwtConfiguracao.Secao).Get<JwtConfiguracao>() ?? new JwtConfiguracao();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfiguracao.Emissao,
            ValidateAudience = true,
            ValidAudience = jwtConfiguracao.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguracao.ChaveSecreta)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SessaoPendente", policy => policy.RequireClaim("token_type", "sessao_pendente"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsConfiguravel", policy =>
    {
        var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
        if (origensPermitidas.Length > 0)
        {
            policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Informe o JWT no formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:AplicarMigrationsAoIniciar"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();
}

var executandoEmDocker = app.Environment.IsEnvironment("Docker");

if (!app.Environment.IsDevelopment() && !executandoEmDocker)
{
    app.UseHsts();
}

if (!executandoEmDocker)
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("CorsConfiguravel");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
