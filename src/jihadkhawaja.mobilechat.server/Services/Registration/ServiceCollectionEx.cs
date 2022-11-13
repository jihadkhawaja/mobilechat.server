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
    public static IConfiguration? Configuration { get; private set; }
    public static bool JWTEnabled { get; private set; } = true;
    public static IServiceCollection AddMobileChatServices(this IServiceCollection services, IConfiguration config)
    {
        Configuration = config;

        services.AddScoped<IMobileChatService, MobileChatService>();
        //signalr
        services.AddSignalR();
        //database
        services.AddDbContext<DataContext>();
        //auth
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("Secrets")["Jwt"]));
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

        services.AddScoped<IEntity<User>, EntityService<User>>();
        services.AddScoped<IEntity<UserFriend>, EntityService<UserFriend>>();
        services.AddScoped<IEntity<Channel>, EntityService<Channel>>();
        services.AddScoped<IEntity<ChannelUser>, EntityService<ChannelUser>>();
        services.AddScoped<IEntity<Message>, EntityService<Message>>();

        return services;
    }

    public static void UseMobileChatServices(this WebApplication app, bool _JWTEnabled = true)
    {
        JWTEnabled = _JWTEnabled;

        //auto-migrate database
        using (IServiceScope scope = app.Services.CreateScope())
        {
            DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();
            db.Database.Migrate();
        }

        if (_JWTEnabled)
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