using FormulaOneApp.Configurations;
using FormulaOneApp.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Add Authentication to the middleware
// In case the main one did not work => It's going to be falling back on DefaultScheme by DefaultChallengeScheme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}) 
// Add Lambda function to build the configuration for the bearer
.AddJwtBearer(jwt =>
{
    var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);

    // Store token in Headers
    jwt.SaveToken = true;

    // Allow us to verify the token that we have generated through the application not any random token
    jwt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        // check the credential of the key to sign it or encrypt and decrypt (To make sure it is a valid key)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // (for dev) to avoid run it as https causing an issue since ssl credentials
        ValidateAudience = false, // for dev
        RequireExpirationTime = false, // for dev -- needs to be updated when refresh token is added
        ValidateLifetime = true, // token > 1 minute is valid, otherwise we reject it
    };
}); // Add Jwt Token and define the authentication mechanism

//Dependency Injection
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
options.SignIn.RequireConfirmedEmail = false).AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
