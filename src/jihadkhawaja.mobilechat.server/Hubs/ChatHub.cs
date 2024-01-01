using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHub : Hub
    {
        [Inject]
        private IEntity<User> UserService { get; set; }
        [Inject]
        private IEntity<UserFriend> UserFriendsService { get; set; }
        [Inject]
        private IEntity<Channel> ChannelService { get; set; }
        [Inject]
        private IEntity<ChannelUser> ChannelUsersService { get; set; }
        [Inject]
        private IEntity<Message> MessageService { get; set; }
        public ChatHub(IEntity<User> UserService, IEntity<UserFriend> UserFriendsService,
            IEntity<Channel> ChannelService, IEntity<ChannelUser> ChannelUsersService,
            IEntity<Message> MessageService)
        {
            this.UserService = UserService;
            this.UserFriendsService = UserFriendsService;
            this.ChannelService = ChannelService;
            this.ChannelUsersService = ChannelUsersService;
            this.MessageService = MessageService;
        }
        public override async Task OnConnectedAsync()
        {
            HttpContext? hc = Context.GetHttpContext();

            if (hc != null)
            {
                string Token = hc.Request.Query["access_token"];

                //set user IsOnline true when he connects or reconnects
                if (!string.IsNullOrWhiteSpace(Token))
                {
                    //get user id from token
                    JwtSecurityTokenHandler tokenHandler = new();
                    JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(Token);
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                    Guid ConnectorUserId = Guid.Parse(userIdClaim.Value);

                    User? connectedUser = await UserService.ReadFirst(x => x.Id == ConnectorUserId);
                    if (connectedUser != null)
                    {
                        connectedUser.ConnectionId = Context.ConnectionId;
                        connectedUser.IsOnline = true;

                        User[] connectedUsers = new User[1] { connectedUser };
                        await UserService.Update(connectedUsers);
                    }
                }
            }

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            User? connectedUser = await UserService.ReadFirst(x => x.ConnectionId == Context.ConnectionId);
            if (connectedUser != null)
            {
                connectedUser.ConnectionId = null;
                connectedUser.IsOnline = false;

                User[] connectedUsers = new User[1] { connectedUser };
                await UserService.Update(connectedUsers);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}