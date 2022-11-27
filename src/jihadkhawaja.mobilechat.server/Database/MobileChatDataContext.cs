using jihadkhawaja.mobilechat.server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace jihadkhawaja.mobilechat.server.Database
{
    public abstract class MobileChatDataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public MobileChatDataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            switch (ServiceCollectionEx.SelectedDatabase)
            {
                case ServiceCollectionEx.DatabaseEnum.Postgres:
                    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b =>
                    b.MigrationsAssembly(ServiceCollectionEx.CurrentExecutionAssemblyName));
                    break;
                case ServiceCollectionEx.DatabaseEnum.SqlServer:
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b =>
                    b.MigrationsAssembly(ServiceCollectionEx.CurrentExecutionAssemblyName));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public DbSet<User>? Users { get; set; }
        public DbSet<UserFriend>? UsersFriends { get; set; }
        public DbSet<Channel>? Channels { get; set; }
        public DbSet<ChannelUser>? ChannelUsers { get; set; }
        public DbSet<Message>? Messages { get; set; }
    }
}
