using System.Text;
using System.Text.Json.Serialization;
using backend.Core.DBContext;
using backend.Core.Entities;
using backend.Interfaces;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);


// Within the JSON options configuration, this line adds a JsonStringEnumConverter to the list of JSON converters. The JsonStringEnumConverter is a special converter that serializes and deserializes enum values as strings instead of integers, which is the default behavior. This makes the JSON output more readable and understandable, as it uses the enum member names instead of their numeric values. The overall effect of this configuration is that when your controllers return data that includes enum types, those enums will be serialized as strings in the JSON response. This can be particularly useful for APIs where you want the client to easily understand the meaning of the enum values without having to map them from numbers to their corresponding names.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


//  config the database
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    // confirmed from appsettings.json that the connection string is named "DefaultConnection", and then create the tables in the database from the models, also notes: change the <InvariantGlobalization>crm_db</InvariantGlobalization> in backend.csproj
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

//  DEPENDENCY INJECTION
builder.Services.AddScoped<ILogService, LogService>();

// ADD IDENTITY
builder.Services
.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

//  CONFIGURE IDENTITY
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    // options.Password.RequiredUniqueChars = 1;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Lockout settings.
    // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    // options.Lockout.MaxFailedAccessAttempts = 5;
    // options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

//  ADD AUTHENTICATION Scheme and JWT Bearer Configuration

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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



// `dotnet ef migrations add init`

// `dotnet ef database update`
