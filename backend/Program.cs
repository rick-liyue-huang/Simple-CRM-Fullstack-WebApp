using System.Text.Json.Serialization;
using backend.Core.DBContext;
using Microsoft.EntityFrameworkCore;


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

// TODO: DEPENDENCY INJECTION

// TODO: ADD IDENTITY

// TODO: CONFIGURE IDENTITY

// TODO: ADD JWT AUTHENTICATION


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

// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();



// `dotnet ef migrations add init`

// `dotnet ef database update`
