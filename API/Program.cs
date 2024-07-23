using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Voeg services toe aan de container.
builder.Services.AddControllers();

//Response community ont-loopen
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});


builder.Services.AddDbContext<StartspelerContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("LocalStartspelerConnection")));

// Voeg Identity services toe met geavanceerde configuratie inclusief wachtwoordvereisten
builder.Services.AddDefaultIdentity<Gebruiker>(options =>
{
    // Wachtwoordvereisten
    options.Password.RequiredLength = 6; // Minimale lengte
    options.Password.RequireDigit = false; // Minimaal één cijfer
    options.Password.RequireLowercase = false; // Minimaal één kleine letter
    options.Password.RequireNonAlphanumeric = false; // Minimaal één niet-alfanumeriek teken
    options.Password.RequireUppercase = false; // Minimaal één hoofdletter
})
.AddEntityFrameworkStores<StartspelerContext>();

// CORS-configuratie
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builderPolicy =>
    {
        builderPolicy.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

// JWT-configuratie
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StartspelerContext>();
    DbInitializer.SeedRechtenData(context);
}

// Configureer de HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection(); Dit gaan we niet gebruiken tijdens development.
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();