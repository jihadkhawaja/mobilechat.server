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
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<User>? Users { get; set; }
        public DbSet<UserFriend>? UsersFriends { get; set; }
        public DbSet<Channel>? Channels { get; set; }
        public DbSet<ChannelUser>? ChannelUsers { get; set; }
        public DbSet<Message>? Messages { get; set; }
    }
}
