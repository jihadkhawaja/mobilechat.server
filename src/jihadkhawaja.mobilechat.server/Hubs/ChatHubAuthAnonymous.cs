using jihadkhawaja.mobilechat.server.Authorization;
using jihadkhawaja.mobilechat.server.Helpers;
using jihadkhawaja.mobilechat.server.Interfaces;
using jihadkhawaja.mobilechat.server.Models;

namespace jihadkhawaja.mobilechat.server.Hubs
{
    public partial class ChatHubAnonymous : IChatAuth
    {
        public async Task<dynamic?> SignUp(string displayname, string username, string email, string password)
        {
            username = username.ToLower();

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.ToLower();
            }

            if (await UserService.ReadFirst(x => x.Username == username) != null)
            {
                return null;
            }

            string encryptedPassword = CryptographyHelper.SecurePassword(password);

            User user = new()
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                Password = encryptedPassword,
                DisplayName = displayname,
                ConnectionId = Context.ConnectionId,
                DateCreated = DateTime.UtcNow,
                IsOnline = true,
                Permission = 0
            };

            if (ServiceCollectionEx.Configuration == null)
            {
                return null;
            }

            var generatedToken = await TokenGenerator.GenerateJwtToken(user, ServiceCollectionEx.Configuration.GetSection("Secrets")["Jwt"]);
            user.Token = generatedToken.Access_Token;

            User[] users = new User[1] { user };
            if (await UserService.Create(users))
            {
                var result = new
                {
                    user.Id,
                    user.Token,
                };

                return result;
            }

            return null;
        }
        public async Task<dynamic?> SignIn(string emailorusername, string password)
        {
            emailorusername = emailorusername.ToLower();

            if (PatternMatchHelper.IsEmail(emailorusername))
            {
                User? user = await UserService.ReadFirst(x => x.Email == emailorusername);

                if (user == null)
                {
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(user.Password))
                {
                    return null;
                }
                else if (!CryptographyHelper.ComparePassword(password, user.Password))
                {
                    return null;
                }
                else if (ServiceCollectionEx.Configuration == null)
                {
                    return null;
                }

                User registeredUser = user;

                registeredUser.ConnectionId = Context.ConnectionId;
                registeredUser.IsOnline = true;

                var generatedToken = await TokenGenerator.GenerateJwtToken(registeredUser, ServiceCollectionEx.Configuration.GetSection("Secrets")["Jwt"]);
                registeredUser.Token = generatedToken.Access_Token;

                User[] users = new User[1] { registeredUser };
                await UserService.Update(users);

                var result = new
                {
                    registeredUser.Id,
                    registeredUser.Token,
                };

                return result;
            }
            else
            {
                User? user = await UserService.ReadFirst(x => x.Username == emailorusername);
                if (user == null)
                {
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(user.Password))
                {
                    return null;
                }
                else if (!CryptographyHelper.ComparePassword(password, user.Password))
                {
                    return null;
                }
                else if (ServiceCollectionEx.Configuration == null)
                {
                    return null;
                }

                User registeredUser = user;

                registeredUser.ConnectionId = Context.ConnectionId;
                registeredUser.IsOnline = true;

                var generatedToken = await TokenGenerator.GenerateJwtToken(registeredUser, ServiceCollectionEx.Configuration.GetSection("Secrets")["Jwt"]);
                registeredUser.Token = generatedToken.Access_Token;

                User[] users = new User[1] { registeredUser };
                await UserService.Update(users);

                var result = new
                {
                    registeredUser.Id,
                    registeredUser.Token,
                };

                return result;
            }
        }
        public async Task<string> RefreshSession(string token)
        {
            User? user = await UserService.ReadFirst(x => x.Token == token);
            if (user == null)
            {
                return null;
            }

            var generatedToken = await TokenGenerator.GenerateJwtToken(user, ServiceCollectionEx.Configuration.GetSection("Secrets")["Jwt"]);
            user.Token = generatedToken.Access_Token;

            User[] users = new User[1] { user };
            await UserService.Update(users);

            return user.Token;
        }
        public async Task<bool> ChangePassword(string emailorusername, string oldpassword, string newpassword)
        {
            if (PatternMatchHelper.IsEmail(emailorusername))
            {
                User? registeredUser = await UserService.ReadFirst(x => x.Email == emailorusername);

                if (registeredUser is null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(registeredUser.Password))
                {
                    return false;
                }
                else if (!CryptographyHelper.ComparePassword(oldpassword, registeredUser.Password))
                {
                    return false;
                }

                string encryptedPassword = CryptographyHelper.SecurePassword(newpassword);
                registeredUser.Password = encryptedPassword;

                User[] users = new User[1] { registeredUser };
                await UserService.Update(users);

                return true;
            }
            else
            {
                User? registeredUser = await UserService.ReadFirst(x => x.Username == emailorusername);

                if (registeredUser is null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(registeredUser.Password))
                {
                    return false;
                }
                else if (!CryptographyHelper.ComparePassword(oldpassword, registeredUser.Password))
                {
                    return false;
                }

                string encryptedPassword = CryptographyHelper.SecurePassword(newpassword);
                registeredUser.Password = encryptedPassword;

                User[] users = new User[1] { registeredUser };
                await UserService.Update(users);

                return true;
            }
        }
    }
}