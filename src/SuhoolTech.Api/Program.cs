using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SuhoolTech.Api.Data;
using SuhoolTech.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("Configure Jwt:Secret using a secure environment variable of at least 32 UTF-8 bytes.");

var issuer = builder.Configuration["Jwt:Issuer"] ?? "SuhoolTech.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "SuhoolTech.React";

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = issuer,
        ValidateAudience = true, ValidAudience = audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = key,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "SuhoolTech.Api" }));
app.MapGet("/api/health/database", async (AppDbContext db, CancellationToken ct) =>
{
    try
    {
        return await db.Database.CanConnectAsync(ct)
            ? Results.Ok(new { status = "connected" })
            : Results.Problem("SQL Server connection failed.", statusCode: 503);
    }
    catch
    {
        return Results.Problem("SQL Server connection failed.", statusCode: 503);
    }
});

app.MapPost("/api/auth/login", async (LoginRequest request, UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { message = "Email and password are required." });

    var user = await users.FindByEmailAsync(request.Email.Trim());
    if (user is null || !user.IsActive) return Results.Unauthorized();

    var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
    if (!result.Succeeded) return Results.Unauthorized();

    var roles = await users.GetRolesAsync(user);
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
    var signingCredentials = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
        SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddMinutes(20);
    var token = new JwtSecurityToken(config["Jwt:Issuer"] ?? "SuhoolTech.Api",
        config["Jwt:Audience"] ?? "SuhoolTech.React", claims, expires: expires,
        signingCredentials: signingCredentials);
    return Results.Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token),
        expires, user.Id, user.Email ?? "", roles));
}).AllowAnonymous();

app.MapGet("/api/auth/me", (ClaimsPrincipal user) => Results.Ok(new
{
    id = user.FindFirstValue(JwtRegisteredClaimNames.Sub),
    email = user.FindFirstValue(JwtRegisteredClaimNames.Email),
    roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
})).RequireAuthorization();

app.Run();
