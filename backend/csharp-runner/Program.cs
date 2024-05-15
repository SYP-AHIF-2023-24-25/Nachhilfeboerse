using Microsoft.AspNetCore.Mvc;
using System;
using System.IO; // Ergänzung
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Ergänzung
using Microsoft.Extensions.DependencyInjection; // Ergänzung
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text.Json.Nodes; // Ergänzung

namespace csharp_runner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseCors("AllowOrigin");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.MapPost("/api/execute/{exerciseName}", RunTests)
                    .WithName("RunTests");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        static async Task<IActionResult> RunTests(string exerciseName, [FromBody] JsonObject jsonContent)
        {
            try
            {
                Console.WriteLine("Unit Test für C# am ausführen");
                var currentDirectory = Directory.GetCurrentDirectory();
                Console.WriteLine("Current Directory: " + currentDirectory);
                Body body = JsonConvert.DeserializeObject<Body>(jsonContent.ToString());
                string templateFilePath = @$"{currentDirectory}/templates/{exerciseName}";
                Console.WriteLine($"Template File Path: {templateFilePath}");
                string filePathForRandomDirectory = @$"{currentDirectory}";
                string filePathForNuGetConfigFile = @$"{currentDirectory}/config";
                var result = await executeTests.runCSharp(exerciseName, templateFilePath, filePathForRandomDirectory, body.code, body.fileName);
                Console.WriteLine($"Result: {result}");
                //log succes
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                //log error
                Console.WriteLine($"Error: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }

    public class Body
    {
        public string code { get; set; }
        public string fileName { get; set; }
    }

    public class TemplateUploadModel
    {
        public string Content { get; set; }
        public IFormFile File { get; set; }
    }
}
