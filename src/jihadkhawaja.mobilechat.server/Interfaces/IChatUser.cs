﻿using jihadkhawaja.mobilechat.server.Models;

namespace jihadkhawaja.mobilechat.server.Interfaces
{
    public interface IChatUser
    {
        Task<string?> GetUserDisplayName(Guid userId);
        Task<string?> GetUserDisplayNameByEmail(string email);
        Task<string?> GetUserUsername(Guid userId);
        Task<bool> AddFriend(string friendEmailorusername);
        Task<bool> RemoveFriend(string friendEmailorusername);
        Task<UserFriend[]?> GetUserFriends(Guid userId);
        Task<UserFriend[]?> GetUserFriendRequests(Guid userId);
        Task<bool> GetUserIsFriend(Guid userId, Guid friendId);
        Task<bool> AcceptFriend(Guid friendId);
        Task<bool> DenyFriend(Guid friendId);
    }
}
