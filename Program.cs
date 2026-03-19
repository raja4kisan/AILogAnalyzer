using System.Text.Json.Serialization;
using LogAnalyzer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add HttpClient
builder.Services.AddHttpClient();

// Register services - Using Azure OpenAI instead of Gemini
builder.Services.AddScoped<ILogAnalyzerService, AzureOpenAILogAnalyzerService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
