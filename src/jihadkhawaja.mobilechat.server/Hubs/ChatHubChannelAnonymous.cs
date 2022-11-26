﻿using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHubAnonymous : IChatChannel
    {

        public async Task<Channel?> CreateChannel(params string[] usernames)
        {
            if (usernames.Length == 0)
            {
                return null;
            }

            Channel channel = new()
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
            };

            Channel[] channels = new Channel[1] { channel };
            await ChannelService.Create(channels);

            await AddChannelUsers(channel.Id, usernames);

            return channel;
        }
        
        public async Task<bool> AddChannelUsers(Guid channelid, params string[] usernames)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return false;
                }

                string Token = hc.Request.Query["access_token"];

                IEnumerable<User> users = await UserService.Read(x => x.Token == Token);
                User? user = users.FirstOrDefault();
                if (user == null)
                {
                    return false;
                }

                Guid ConnectorUserId = user.Id;

                ChannelUser[] channelUsers = new ChannelUser[usernames.Length];

                for (int i = 0; i < usernames.Length; i++)
                {
                    IEnumerable<User> usersToAdd = await UserService.Read(x => x.Username == usernames[i]);
                    User? userToAdd = usersToAdd.FirstOrDefault();
                    if (userToAdd is null)
                    {
                        return false;
                    }

                    Guid currentuserid = userToAdd.Id;

                    if (await ChannelContainUser(channelid, currentuserid))
                    {
                        continue;
                    }

                    channelUsers[i] = new ChannelUser()
                    {
                        Id = Guid.NewGuid(),
                        ChannelId = channelid,
                        UserId = currentuserid,
                        DateCreated = DateTime.UtcNow,
                    };

                    if (ConnectorUserId == currentuserid)
                    {
                        channelUsers[i].IsAdmin = true;
                    }
                }

                await ChannelUsersService.Create(channelUsers);

                return true;
            }
            catch { }

            return false;
        }
        
        public async Task<bool> ChannelContainUser(Guid channelid, Guid userid)
        {
            return (await ChannelUsersService.Read(x => x.ChannelId == channelid && x.UserId == userid)).FirstOrDefault() != null;
        }
        
        public async Task<User[]?> GetChannelUsers(Guid channelid)
        {
            HashSet<User> channelUsers = new();
            try
            {
                List<ChannelUser> currentChannelUsers = (await ChannelUsersService.Read(x => x.ChannelId == channelid)).ToList();
                foreach (ChannelUser user in currentChannelUsers)
                {
                    var userdata = (await UserService.Read(x => x.Id == user.UserId)).FirstOrDefault();

                    if(userdata != null)
                    {
                        channelUsers.Add(userdata);
                    }
                }
            }
            catch { }

            //only send users ids and display names
            List<User> users = new();
            foreach (User user in channelUsers)
            {
                users.Add(new User
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Username = user.Username,
                    ConnectionId = user.ConnectionId,
                });
            }

            return users.ToArray();
        }
        
        public async Task<Channel[]?> GetUserChannels()
        {
            HashSet<Channel> userChannels = new();
            try
            {
                HttpContext? hc = Context.GetHttpContext();

                if(hc == null)
                {
                    return null;
                }

                string Token = hc.Request.Query["access_token"];
                IEnumerable<User> users = await UserService.Read(x => x.Token == Token);
                User? user = users.FirstOrDefault();
                if(user == null)
                {
                    return null;
                }
                Guid ConnectorUserId = user.Id;

                List<ChannelUser> channelUsers = (await ChannelUsersService.Read(x => x.UserId == ConnectorUserId)).ToList();
                foreach (ChannelUser cu in channelUsers)
                {
                    IEnumerable<Channel> channels = await ChannelService.Read(x => x.Id == cu.ChannelId);
                    Channel? channel = channels.FirstOrDefault();
                    if (channel == null)
                    {
                        return null;
                    }

                    userChannels.Add(channel);
                }
            }
            catch { }

            return userChannels.ToArray();
        }
        
        public async Task<bool> IsChannelAdmin(Guid channelId, Guid userId)
        {
            IEnumerable<ChannelUser> channelUsers = await ChannelUsersService.Read(x => x.ChannelId == channelId && x.UserId == userId && x.IsAdmin);
            ChannelUser? channelAdmin = channelUsers.FirstOrDefault();

            if (channelAdmin is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        public async Task<bool> DeleteChannel(Guid channelId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if(hc == null)
            {
                return false;
            }

            string Token = hc.Request.Query["access_token"];

            IEnumerable<User> users = await UserService.Read(x => x.Token == Token);
            User? user = users.FirstOrDefault();
            if (user == null)
            {
                return false;
            }

            Guid ConnectorUserId = user.Id;

            if (!await IsChannelAdmin(channelId, ConnectorUserId))
            {
                return false;
            }

            if (!await ChannelUsersService.Delete(x => x.ChannelId == channelId))
            {
                return false;
            }

            if (!await MessageService.Delete(x => x.ChannelId == channelId))
            {
                return false;
            }

            return await ChannelService.Delete(x => x.Id == channelId);
        }
        
        public async Task<bool> LeaveChannel(Guid channelId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string Token = hc.Request.Query["access_token"];

            IEnumerable<User> users = await UserService.Read(x => x.Token == Token);
            User? user = users.FirstOrDefault();
            if (user == null)
            {
                return false;
            }
            Guid ConnectorUserId = user.Id;

            return await ChannelUsersService.Delete(x => x.UserId == ConnectorUserId && x.ChannelId == channelId);
        }
    }
}