using jihadkhawaja.mobilechat.server.Database;
using jihadkhawaja.mobilechat.server.Hubs;
using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using jihadkhawaja.mobilechat.server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class ServiceCollectionEx
{
    public enum DatabaseEnum
    {
        Postgres,
        SqlServer
    }
    public static IConfiguration? Configuration { get; private set; }
    public static bool JWTEnabled { get; private set; } = true;
    public static DatabaseEnum SelectedDatabase { get; private set; }
    public static string CurrentExecutionAssemblyName { get; private set; }
    private static bool AutoMigrateDatabase { get; set; }
    /// <summary>
    /// Add MobileChat Server Services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="databaseEnum">Database type (Postgres, SqlServer..etc.)</param>
    /// <param name="executionClassType">Main execuation class (Program or Startup..etc.)</param>
    public static IServiceCollection AddMobileChatServices(this IServiceCollection services, IConfiguration config, Type executionClassType, DatabaseEnum databaseEnum, bool autoMigrateDatabase = true, bool jwtAuthentication = true)
    {
        Configuration = config;
        SelectedDatabase = databaseEnum;
        CurrentExecutionAssemblyName = System.Reflection.Assembly.GetAssembly(executionClassType).GetName().Name;
        JWTEnabled = jwtAuthentication;
        AutoMigrateDatabase = autoMigrateDatabase;

        string jwtKey = Configuration.GetSection("Secrets")["Jwt"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new NullReferenceException(nameof(jwtKey));
        }

        services.AddScoped<IMobileChatService, MobileChatService>();
        //signalr
        services.AddSignalR();
        //database
        services.AddDbContext<DataContext>();
        //auth
        if(JWTEnabled)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes());
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/chathub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization();
        }

        services.AddScoped<IEntity<User>, EntityService<User>>();
        services.AddScoped<IEntity<UserFriend>, EntityService<UserFriend>>();
        services.AddScoped<IEntity<Channel>, EntityService<Channel>>();
        services.AddScoped<IEntity<ChannelUser>, EntityService<ChannelUser>>();
        services.AddScoped<IEntity<Message>, EntityService<Message>>();

        return services;
    }
    /// <summary>
    /// Use MobileChat Server Services
    /// </summary>
    /// <param name="app"></param>
    /// <param name="jwtEnabled"></param>
    public static void UseMobileChatServices(this WebApplication app)
    {
        //auto-migrate database
        if(AutoMigrateDatabase)
        {
            using (IServiceScope scope = app.Services.CreateScope())
            {
                DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();
                db.Database.Migrate();
            }
        }

        if (JWTEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            //hubs
            app.MapHub<ChatHub>("/chathub");
        }
        else
        {
            app.MapHub<ChatHubAnonymous>("/chathub");
        }
    }
}