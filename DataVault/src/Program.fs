module Program

open System
open System.IO
open DataVault.db
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open DataVault.api.Route

let exitCode = 0

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureCors (builder: CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    app
        .UseCors(configureCors)
        .UseGiraffeErrorHandler(errorHandler)
        .UseStaticFiles()
        .UseAuthentication()
        .UseGiraffe
        webRouting

let configureServices (services: IServiceCollection) =
    let connectionString = Environment.GetEnvironmentVariable("ASP_AUTH_DB")

    services.AddDbContext<ApplicationDbContext>(fun options -> options.UseSqlite(connectionString) |> ignore)
    |> ignore

    services
        .BuildServiceProvider()
        .GetRequiredService<ApplicationDbContext>()
        .Database.EnsureCreated()
    |> ignore

    // Register Identity Dependencies
    services
        .AddIdentity<IdentityUser, IdentityRole>(fun options ->
            // Password settings
            options.Password.RequireDigit <- true
            options.Password.RequiredLength <- 8
            options.Password.RequireNonAlphanumeric <- false
            options.Password.RequireUppercase <- false
            options.Password.RequireLowercase <- true

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan <- TimeSpan.FromMinutes 30.0
            options.Lockout.MaxFailedAccessAttempts <- 10

            // User settings
            options.User.RequireUniqueEmail <- true)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
    |> ignore

    // Configure app cookie
    services.ConfigureApplicationCookie(fun options ->
        options.ExpireTimeSpan <- TimeSpan.FromDays 150.0
        options.LoginPath <- PathString "/login"
        options.LogoutPath <- PathString "/logout")
    |> ignore

    // Enable CORS
    services.AddCors() |> ignore

    // Configure Giraffe dependencies
    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    let filter (l: LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    exitCode
