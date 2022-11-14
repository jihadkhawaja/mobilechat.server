using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;
using Microsoft.AspNetCore.SignalR;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHubAnonymous : IChatMessage
    {

        public async Task<bool> SendMessage(Message message)
        {
            if (message == null)
            {
                return false;
            }

            if (message.SenderId == Guid.Empty)
            {
                return false;
            }

            if (string.IsNullOrEmpty(message.Content) || string.IsNullOrWhiteSpace(message.Content))
            {
                return false;
            }

            //save msg to db
            message.Sent = true;
            message.DateSent = DateTime.UtcNow;

            Message[] messages = new Message[1] { message };
            if (await MessageService.Create(messages))
            {
                User[]? users = await GetChannelUsers(message.ChannelId);
                if (users is null)
                {
                    return false;
                }
                foreach (User user in users)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(user.ConnectionId))
                        {
                            continue;
                        }

                        await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessage", message);
                    }
                    catch { }
                }

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateMessage(Message message)
        {
            if (message == null)
            {
                return false;
            }

            if (message.SenderId == Guid.Empty)
            {
                return false;
            }

            if (string.IsNullOrEmpty(message.Content) || string.IsNullOrWhiteSpace(message.Content))
            {
                return false;
            }

            Message[] messages = new Message[1] { message };
            //save msg to db
            if (await MessageService.Update(messages))
            {
                User[]? users = await GetChannelUsers(message.ChannelId);
                if (users is null)
                {
                    return false;
                }
                foreach (User user in users)
                {
                    if (string.IsNullOrEmpty(user.ConnectionId))
                    {
                        continue;
                    }

                    await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessage", message);
                }

                return true;
            }

            return false;
        }

        public async Task<Message[]?> ReceiveMessageHistory(Guid channelId)
        {
            HashSet<Message> msgs = (await MessageService.Read(x => x.ChannelId == channelId)).ToHashSet();
            return msgs.ToArray();
        }

        public async Task<Message[]?> ReceiveMessageHistoryRange(Guid channelId, int index, int range)
        {
            Message[]? messages = await ReceiveMessageHistory(channelId);
            if (messages is null)
            {
                return null;
            }
            HashSet<Message> msgs = messages.Skip(index).Take(range).ToHashSet();
            return msgs.ToArray();
        }
    }
}