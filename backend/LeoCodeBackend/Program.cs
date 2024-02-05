using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LeoCodeBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            InstallingNodeModules("Typescript", "PasswordChecker");
            BuildImage("typescript");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:4200/test-results", "http://localhost:4200")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowAnyOrigin();
                });
            });
            builder.Services.AddSignalR();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAngularFrontend");
            app.UseCors();
            app.UseHttpsRedirection();

            app.MapPost("/runtests", RunTestsApi)
                .WithName("RunTestsApi")
                .WithOpenApi();

            app.Run();
        }
        
        static async Task<IActionResult> RunTestsApi(string language, string ProgramName)
        {
            try
            {
                var cwd = Directory.GetCurrentDirectory();

                var path = $@"{cwd}\..\languages";

                cwd = $@"{path}\{language}\{ProgramName}";

                var command = $"run --rm -v {path}:/usr/src/project -w /usr/src/project pwdtest {language} {ProgramName}";
                var processInfo = new ProcessStartInfo("docker", command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var proc = new Process { StartInfo = processInfo, EnableRaisingEvents = true })
                {
                    proc.Start();
                    await proc.WaitForExitAsync();

                    var code = proc.ExitCode;
                    ResultFileHelperCSharp resultFileHelperCSharp = new ResultFileHelperCSharp();
                    var resultsFile = Directory.GetFiles($"{cwd}\\results", "*.json").FirstOrDefault();

                    if (resultsFile != null)
                    {
                        string jsonString = await File.ReadAllTextAsync(resultsFile);

                        var jsonDocument = JsonDocument.Parse(jsonString);
                        var rootElement = jsonDocument.RootElement;

                        var responseObject = new { data = rootElement };

                        return new OkObjectResult(responseObject);
                    }
                    else
                    {
                        var errorObject = new { error = "No results file found." };
                        return new BadRequestObjectResult(errorObject);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorObject = new { error = $"An error occurred: {ex.Message}" };
                return new BadRequestObjectResult(errorObject);
            }
        }

        static async void InstallingNodeModules(string language, string projectName) {
            var cwd = Directory.GetCurrentDirectory();

            var path = $@"{cwd}\..\languages\{language}\{projectName}";

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = path
            };

            process.StartInfo = startInfo;
            process.Start();

            process.StandardInput.WriteLine("npm install");
            process.StandardInput.Flush();
            process.StandardInput.Close();

            process.WaitForExit();
        }

        static async void BuildImage(string language) 
        {
            try 
            {
                var cwd = Directory.GetCurrentDirectory();
                var dockerFilePath = $@"{cwd}\..\languages\Dockerfile.{language}";
                var projectBuildPath = $@"{cwd}\..\languages";
                Console.WriteLine(dockerFilePath);
                Console.WriteLine(projectBuildPath);
                var command = $"build -f {dockerFilePath} -t passwordchecker {projectBuildPath}";
                var processInfo = new ProcessStartInfo("docker", command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var proc = new Process { StartInfo = processInfo, EnableRaisingEvents = true })
                {
                    proc.Start();
                    await proc.WaitForExitAsync();

                    var code = proc.ExitCode;
                

                    if (code == 0)
                    {
                        Console.WriteLine("Image builed successfully.");
                    }
                    else
                    {
                        //Console.WriteLine($"Image builed not successfully. Exit Code: {backendProcess.ExitCode}");
                    }
                }

            } 
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
