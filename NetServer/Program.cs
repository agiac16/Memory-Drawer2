using NetServer.Abstractions;
using NetServer.Services;
using MongoDB.Driver;
using NetServer.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NetServer.Repository;

var builder = WebApplication.CreateBuilder(args);

// cors policy to accept from frontend
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// singleton is a single instance used for entire lifetime of app | used by everything in app
builder.Services.AddSingleton<IMongoClient>(s =>
{
var mongoUri = builder.Configuration.GetSection("MongoDB:ConnectionString").Value;
    if (string.IsNullOrEmpty(mongoUri))
    {
        throw new InvalidOperationException("MongoDB connection string is missing.");
    }
    return new MongoClient(mongoUri);
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<SearchService>(); // scoped bcs it changes each time a user calls it
builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
builder.Services.AddSingleton<IMusicRepository, MusicRepository>();
builder.Services.AddSingleton<IBookRepository, BookRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<MongoDBService>(); // ensure its registered
builder.Services.AddSingleton<IUserRepository, UserRepository>(); // register repo
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
app.UseCors(MyAllowSpecificOrigins); 
// different controllers for each api
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
