using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Notaion.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Notaion.Services;
using Notaion.Domain.Models;

namespace Notaion.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IConfiguration configuration;


        public AccountRepository(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        public async Task<string> SignInAsync(SignInModel model) // đăng nhập
        {
            User user = null;

            if (model.Email.Contains("@"))
            {
                user = await userManager.FindByEmailAsync(model.Email);
            }
            else
            {
                user = await userManager.FindByNameAsync(model.Email);
            }

            if (user == null)
            {
                return "UserNotFound";
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (!result.Succeeded)
            {
                return "InvalidPassword";
            }

            var userId = user.Id;

            var authClaims = new List<Claim> // jwt.io
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Country, user.Avatar),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
   
            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(20),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<IdentityResult> SignUpAsync(SignUpModel model) // đăng kí
        {
            //if (model.Password != model.ConfirmPassword)
            //{
            //    return IdentityResult.Failed(new IdentityError { Description = "Password and confirmation password do not match." });
            //}

            User existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            User existingUserByUsername = await userManager.FindByNameAsync(model.Username);

            if (existingUserByEmail != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email exists" });
            }

            if (existingUserByUsername != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Username exists" });
            }


            var newUser = new User
            {
                Email = model.Email,
                UserName = model.Username,
                EmailConfirmed = true,
                Avatar = model.Avatar,
            };

            var result = await userManager.CreateAsync(newUser, model.Password);

            return result;
        }
    }
}