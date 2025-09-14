using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure environment variables support
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy to allow all websites
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Note: AllowCredentials() cannot be used with AllowAnyOrigin()
        // If you need credentials, you must specify specific origins
    });
});

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS - must be called before UseRouting and UseEndpoints
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
