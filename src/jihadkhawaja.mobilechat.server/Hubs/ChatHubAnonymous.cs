using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHubAnonymous : Hub
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
        public ChatHubAnonymous(IEntity<User> UserService, IEntity<UserFriend> UserFriendsService,
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
                    User? connectedUser = (await UserService.Read(x => x.Token == Token)).FirstOrDefault();
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
            HttpContext? hc = Context.GetHttpContext();

            if(hc != null)
            {
                string Token = hc.Request.Query["access_token"];

                //set user IsOnline false when he disconnects
                if (!string.IsNullOrWhiteSpace(Token))
                {
                    User? connectedUser = (await UserService.Read(x => x.Token == Token)).FirstOrDefault();
                    if (connectedUser != null)
                    {
                        connectedUser.IsOnline = false;

                        User[] connectedUsers = new User[1] { connectedUser };
                        await UserService.Update(connectedUsers);
                    }
                }

            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}