using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Hiper.Api.Helpers;
using Hiper.Api.Helpers.Services;
using Hiper.Api.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace Hiper.Api.Repositories
{
    public class UserRepository : IDisposable
    {
        private readonly AppContext _ctx;

        private readonly UserManager<UserModel> _userManager;

        public UserRepository(AppContext context)
        {
            _ctx = context;
            var provider = Startup.DataProtectionProvider;
            _userManager = new UserManager<UserModel>(new UserStore<UserModel>(_ctx));
            _userManager.EmailService = new EmailService();


            _userManager.UserTokenProvider = new DataProtectorTokenProvider<UserModel>(
                provider.Create("EmailConfirmation"));
        }

        public UserRepository()
        {
            _ctx = new AppContext();
            var provider = Startup.DataProtectionProvider;
            _userManager = new UserManager<UserModel>(new UserStore<UserModel>(_ctx));


            _userManager.UserTokenProvider = new DataProtectorTokenProvider<UserModel>(
                provider.Create("EmailConfirmation"));
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(userId, token, password);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(userId);
        }

        public async Task<List<UserModel>> GetAllAsync()
        {
            return await Task.FromResult(_ctx.Users.ToList());
        }

        public bool IsEmailConfirmed(string userId)
        {
            return _userManager.IsEmailConfirmed(userId);
        }


        public async Task<IdentityResult> RegisterUser(ProfileModel profileModel)
        {
            var user = Mapper.Map<ProfileModel, UserModel>(profileModel);
          
            user.RegDate = DateTime.UtcNow;
           
                var result = await _userManager.CreateAsync(user, profileModel.Password);
                user.Picture = profileModel.Picture == null ? "" : UploadHelper.SaveUploadedImage(profileModel.Picture, user.Id);
                await _userManager.UpdateAsync(user);

                return result;
            
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            
          
               var result = await _userManager.GenerateEmailConfirmationTokenAsync(userId);
         
            return result;
        }

        public async Task SendEmailAsync(string userId, string subject, string body)
        {
           
                await _userManager.SendEmailAsync(userId, subject, body);
           
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(userId, token);
            return result;
        }

        public async Task<IdentityResult> UpdateUser(ProfileModel profileModel)
        {
            var user = FindUserByUserName(profileModel.UserName);
            user.FirstName = profileModel.FirstName;
            user.LastName = profileModel.LastName;
            user.NickName = profileModel.NickName;
            user.Title = profileModel.Title;
            user.Company = profileModel.Company;
            if (!profileModel.Picture.Contains("api/Image"))
            {
                user.Picture = UploadHelper.SaveUploadedImage(profileModel.Picture, user.Id);
            }
            // var user = AutoMapper.Mapper.Map<ProfileModel, UserModel>(profileModel);


            var userUpdateResult = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(profileModel.Password))
            {
               

                var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                var result = await _userManager.ResetPasswordAsync(user.Id, token, profileModel.Password);
                return result;
            }


            return userUpdateResult;
        }


        public async Task<IdentityResult> UpdateUser(UserModel user)
        {
            var userUpdateResult = await _userManager.UpdateAsync(user);
            return userUpdateResult;
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public UserModel FindUserById(string userId)
        {
            var user = _userManager.FindById(userId);

            return user;
        }

        public UserModel FindUserByUserName(string userName)
        {
            var user = _userManager.FindByName(userName);

            return user;
        }

        public List<UserModel> FindUsersByTeamId(int teamId)
        {
            var userList = _ctx.Users.Where(u => u.Teams.Any(t => t.TeamId == teamId)).ToList();

            return userList;
        }

        public List<UserModel> FindApplicantsByTeamId(int teamId)
        {
            var userList = _ctx.Users.Where(u => u.TeamsApplicants.Any(t => t.TeamId == teamId)).ToList();

            return userList;
        }


        public IQueryable<UserModel> GetAll()
        {
            return _ctx.Users.Select(u => u);
        }

        public IQueryable<UserModel> FindBy(Expression<Func<UserModel, bool>> predicate)
        {
            var found = _ctx.Users.Where(predicate);
            return found;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();
        }
    }
}