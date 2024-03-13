using System.Text.Json.Serialization;
using backend.Core.DBContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context and Connection String FROM appsettings.json
builder.Services.AddDbContext<WebApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// Converters.Add(new JsonStringEnumConverter()) is used to serialize and deserialize enum values as strings rather than integers, which can make your JSON more readable.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


// TODO: Dependency Injection


// TODO: Add Identity


// TODO: configure the identity options


// TODO: Add AuthenticationSchema and JWTBearer




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();

