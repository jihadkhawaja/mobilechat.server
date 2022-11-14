using jihadkhawaja.mobilechat.server.Helpers;
using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHubAnonymous : IChatUser
    {

        public async Task<string?> GetUserDisplayName(Guid userId)
        {
            IEnumerable<User> users = await UserService.Read(x => x.Id == userId);
            if (users == null)
            {
                return null;
            }
            User? user = users.FirstOrDefault();

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
        public async Task<string?> GetUserDisplayNameByEmail(string email)
        {
            IEnumerable<User> users = await UserService.Read(x => x.Email == email);
            if (users == null)
            {
                return null;
            }
            User? user = users.FirstOrDefault();

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

        public async Task<string?> GetUserUsername(Guid userId)
        {
            IEnumerable<User> users = await UserService.Read(x => x.Id == userId);
            if (users == null)
            {
                return null;
            }
            User? user = users.FirstOrDefault();

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
                IEnumerable<User> users = await UserService.Read(x => x.Token == Token);
                User? cuser = users.FirstOrDefault();
                if (cuser == null)
                {
                    return false;
                }

                Guid ConnectorUserId = cuser.Id;

                if (PatternMatchHelper.IsEmail(friendEmailorusername))
                {
                    //get user id from email
                    User? user = (await UserService.Read(x => x.Id == ConnectorUserId)).FirstOrDefault();
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from email
                    User? friendUser = (await UserService.Read(x => x.Email == friendEmailorusername)).FirstOrDefault();
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if ((await UserFriendsService.Read(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id)).FirstOrDefault() != null)
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
                    User? user = (await UserService.Read(x => x.Id == ConnectorUserId)).FirstOrDefault();
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from username
                    User? friendUser = (await UserService.Read(x => x.Username == friendEmailorusername)).FirstOrDefault();
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if ((await UserFriendsService.Read(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id)).FirstOrDefault() != null)
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

                IEnumerable<User> dbusers = await UserService.Read(x => x.Token == Token);
                User? dbuser = dbusers.FirstOrDefault();
                if (dbuser == null)
                {
                    return false;
                }
                Guid ConnectorUserId = dbuser.Id;

                if (PatternMatchHelper.IsEmail(friendEmailorusername))
                {
                    //get user id from email
                    User? user = (await UserService.Read(x => x.Id == ConnectorUserId)).FirstOrDefault();
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from email
                    User? friendUser = (await UserService.Read(x => x.Email == friendEmailorusername)).FirstOrDefault();
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if ((await UserFriendsService.Read(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id)).FirstOrDefault() != null)
                    {
                        return false;
                    }

                    UserFriend entry = new() { UserId = user.Id, FriendUserId = friendUser.Id, DateCreated = DateTime.UtcNow };

                    await UserFriendsService.Delete(x => x.Id == entry.Id);
                }
                else
                {
                    //get user id from username
                    User? user = (await UserService.Read(x => x.Id == ConnectorUserId)).FirstOrDefault();
                    if (user == null)
                    {
                        return false;
                    }
                    //get friend id from username
                    User? friendUser = (await UserService.Read(x => x.Username == friendEmailorusername)).FirstOrDefault();
                    if (friendUser == null)
                    {
                        return false;
                    }

                    if ((await UserFriendsService.Read(x => x.UserId == user.Id && x.FriendUserId == friendUser.Id
                    || x.FriendUserId == user.Id && x.UserId == friendUser.Id)).FirstOrDefault() == null)
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

        public async Task<UserFriend[]?> GetUserFriends(Guid userId)
        {
            return (await UserFriendsService.Read(x => x.UserId == userId && x.IsAccepted || x.FriendUserId == userId && x.IsAccepted)).ToArray();
        }

        public async Task<UserFriend[]?> GetUserFriendRequests(Guid userId)
        {
            return (await UserFriendsService.Read(x => x.FriendUserId == userId && !x.IsAccepted)).ToArray();
        }

        public async Task<bool> GetUserIsFriend(Guid userId, Guid friendId)
        {
            UserFriend? result = (await UserFriendsService.Read(x => x.UserId == userId && x.FriendUserId == friendId && x.IsAccepted)).FirstOrDefault();

            if (result is null)
            {
                return false;
            }

            return result.IsAccepted;
        }

        public async Task<bool> AcceptFriend(Guid friendId)
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

            UserFriend? friendRequest = (await UserFriendsService.Read(x => x.UserId == friendId && x.FriendUserId == ConnectorUserId && !x.IsAccepted)).FirstOrDefault();

            if (friendRequest is null)
            {
                return false;
            }

            friendRequest.IsAccepted = true;

            UserFriend[] friendRequests = new UserFriend[1] { friendRequest };
            return await UserFriendsService.Update(friendRequests);
        }

        public async Task<bool> DenyFriend(Guid friendId)
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

            return await UserFriendsService.Delete(x => x.UserId == friendId && x.FriendUserId == ConnectorUserId && !x.IsAccepted);
        }
    }
}