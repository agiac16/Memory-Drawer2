using NetServer.Abstractions;
using NetServer.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NetServer.Repository;
using NetServer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// connect to mysql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
// scoped lives for one request
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IUserRepository, UserRepository>(); // register repo
builder.Services.AddProblemDetails(); // error message structure

// jwt configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
        options.TokenValidationParameters = new TokenValidationParameters 
        { 
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]
                    ?? throw new ArgumentNullException("JWT Secret missing"))
            )
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// different controllers for each api
app.MapControllers();

app.UseHttpsRedirection();

app.Run();
