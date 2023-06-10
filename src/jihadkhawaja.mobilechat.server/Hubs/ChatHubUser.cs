using jihadkhawaja.mobilechat.server.Helpers;
using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHub : IChatUser
    {
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<string?> GetUserDisplayName(Guid userId)
        {
            User? user = await UserService.ReadFirst(x => x.Id == userId);

            if (user == null)
            {
                return null;
            }
            else if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                return null;
            }

            string displayname = user.DisplayName;

            return displayname;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<string?> GetUserDisplayNameByEmail(string email)
        {
            email = email.ToLower();
            User? user = await UserService.ReadFirst(x => x.Email == email);

            if (user == null)
            {
                return null;
            }
            else if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                return null;
            }

            string displayname = user.DisplayName;

            return displayname;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<string?> GetUserUsername(Guid userId)
        {
            User? user = await UserService.ReadFirst(x => x.Id == userId);

            if (user == null)
            {
                return null;
            }
            else if (string.IsNullOrWhiteSpace(user.Username))
            {
                return null;
            }

            string username = user.Username;

            return username;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> AddFriend(string friendEmailorusername)
        {
            if (string.IsNullOrEmpty(friendEmailorusername))
            {
                return false;
            }

            try
            {
                HttpContext? hc = Context.GetHttpContext();

                if (hc == null)
                {
                    return false;
                }

                string Token = hc.Request.Query["access_token"];
                User? cuser = await UserService.ReadFirst(x => x.Token == Token);
                if (cuser == null)
                {
                    return false;
                }

                Guid ConnectorUserId = cuser.Id;
                friendEmailorusername = friendEmailorusername.ToLower();

                if (PatternMatchHelper.IsValidEmail(friendEmailorusername))
                {
                    //get user id from email
                    User? user = await UserService.ReadFirst(x => x.Id == ConnectorUserId);
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from email
                    User? friendUser = await UserService.ReadFirst(x => x.Email == friendEmailorusername);
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if (await UserFriendsService.ReadFirst(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id) != null)
                    {
                        return false;
                    }

                    UserFriend entry = new() { UserId = user.Id, FriendUserId = friendUser.Id, DateCreated = DateTime.UtcNow };
                    UserFriend[] entries = new UserFriend[1] { entry };
                    await UserFriendsService.Create(entries);
                }
                else
                {
                    //get user id from username
                    User? user = await UserService.ReadFirst(x => x.Id == ConnectorUserId);
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from username
                    User? friendUser = await UserService.ReadFirst(x => x.Username == friendEmailorusername);
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if (await UserFriendsService.ReadFirst(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id) != null)
                    {
                        return false;
                    }

                    UserFriend entry = new() { Id = Guid.NewGuid(), UserId = user.Id, FriendUserId = friendUser.Id, DateCreated = DateTime.UtcNow };
                    UserFriend[] entries = new UserFriend[1] { entry };
                    await UserFriendsService.Create(entries);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> RemoveFriend(string friendEmailorusername)
        {
            if (string.IsNullOrEmpty(friendEmailorusername))
            {
                return false;
            }

            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return false;
                }

                string Token = hc.Request.Query["access_token"];

                User? dbuser = await UserService.ReadFirst(x => x.Token == Token);
                if (dbuser == null)
                {
                    return false;
                }
                Guid ConnectorUserId = dbuser.Id;
                friendEmailorusername = friendEmailorusername.ToLower();

                if (PatternMatchHelper.IsValidEmail(friendEmailorusername))
                {
                    //get user id from email
                    User? user = await UserService.ReadFirst(x => x.Id == ConnectorUserId);
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from email
                    User? friendUser = await UserService.ReadFirst(x => x.Email == friendEmailorusername);
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if (await UserFriendsService.ReadFirst(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id ||
                    x.FriendUserId == user.Id && x.UserId == friendUser.Id) != null)
                    {
                        return false;
                    }

                    UserFriend entry = new() { UserId = user.Id, FriendUserId = friendUser.Id, DateCreated = DateTime.UtcNow };

                    await UserFriendsService.Delete(x => x.Id == entry.Id);
                }
                else
                {
                    //get user id from username
                    User? user = await UserService.ReadFirst(x => x.Id == ConnectorUserId);
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from username
                    User? friendUser = await UserService.ReadFirst(x => x.Username == friendEmailorusername);
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if (await UserFriendsService.ReadFirst(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id ||
                    x.FriendUserId == user.Id && x.UserId == friendUser.Id) == null)
                    {
                        return false;
                    }

                    UserFriend entry = new() { UserId = user.Id, FriendUserId = friendUser.Id, DateCreated = DateTime.UtcNow };

                    await UserFriendsService.Delete(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<UserFriend[]?> GetUserFriends(Guid userId)
        {
            return (await UserFriendsService.Read(x => x.UserId == userId && x.IsAccepted || x.FriendUserId == userId && x.IsAccepted)).ToArray();
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<UserFriend[]?> GetUserFriendRequests(Guid userId)
        {
            return (await UserFriendsService.Read(x => x.FriendUserId == userId && !x.IsAccepted)).ToArray();
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> GetUserIsFriend(Guid userId, Guid friendId)
        {
            UserFriend? result = await UserFriendsService.ReadFirst(x => x.UserId == userId && x.FriendUserId == friendId && x.IsAccepted);

            if (result is null)
            {
                return false;
            }

            return result.IsAccepted;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> AcceptFriend(Guid friendId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string Token = hc.Request.Query["access_token"];

            User? user = await UserService.ReadFirst(x => x.Token == Token);

            if (user == null)
            {
                return false;
            }
            Guid ConnectorUserId = user.Id;

            UserFriend? friendRequest = await UserFriendsService.ReadFirst(x => x.UserId == friendId && x.FriendUserId == ConnectorUserId && !x.IsAccepted);

            if (friendRequest is null)
            {
                return false;
            }

            friendRequest.IsAccepted = true;

            UserFriend[] friendRequests = new UserFriend[1] { friendRequest };
            return await UserFriendsService.Update(friendRequests);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> DenyFriend(Guid friendId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string Token = hc.Request.Query["access_token"];

            User? user = await UserService.ReadFirst(x => x.Token == Token);

            if (user == null)
            {
                return false;
            }
            Guid ConnectorUserId = user.Id;

            return await UserFriendsService.Delete(x => x.UserId == friendId && x.FriendUserId == ConnectorUserId && !x.IsAccepted);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IEnumerable<User>?> SearchUser(string query)
        {
            IEnumerable<User>? users = await UserService.Read(x => 
            x.Username.Contains(query, StringComparison.InvariantCultureIgnoreCase) 
            || x.DisplayName.Contains(query, StringComparison.InvariantCultureIgnoreCase));

            if (users == null)
            {
                return null;
            }

            return users.Select(x => 
            new User
            { 
                DisplayName = x.DisplayName,
                Username = x.Username 
            });
        }
    }
}