using Microsoft.OpenApi.Models;
using Serilog;
using UPB.BusinessLogic.Managers;
using UPB.BusinessLogic.Managers.Exceptions;
using UPB.Practica_2.Middlewares;

var builder = WebApplication.CreateBuilder(args);

//Configuraciones de entorno
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings." + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + ".json"
    )
    .Build();


string titulo = builder.Configuration.GetSection("ConnectionStrings").GetSection("titulo").Value;
if (titulo == null) {
    titulo = "WebAPI de Adrian";
    throw new PracticeException("No se pudo obtener el titulo del archivo de configuracion y devolvio nulo");
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<PatientManager>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo() {
        Title = titulo
    });
});

var app = builder.Build();

//app.UseExceptionHandlerMiddleware();

string direccionLog = builder.Configuration.GetSection("Logging").GetSection("FileLocation").Value;
if (direccionLog == null)
{
    direccionLog = "../logs";
    throw new PracticeException("No se pudo obtener la direccion para guardar los logs y devolvio null");
}

app.UseSwaggerUI(c =>
{
    c.DocumentTitle = titulo;
});

if (app.Environment.IsDevelopment())
{

    Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(direccionLog, rollingInterval: RollingInterval.Hour) 
    .CreateLogger();
    Log.Information("Se inicio el servidor en Development (Logs en consola y archivos)");
}
else {
    Log.Logger = new LoggerConfiguration()
    .WriteTo.File(direccionLog, rollingInterval: RollingInterval.Hour) 
    .CreateLogger();
    Log.Information("Se inicio el servidor en QA o UAT (Logs solo en archivo)");
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "QA" || app.Environment.EnvironmentName == "UAT")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandlerMiddleware();

app.Run();
