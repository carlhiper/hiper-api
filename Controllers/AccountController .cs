using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Hiper.Api.Helpers;
using Hiper.Api.Helpers.Services;
using Hiper.Api.Models;
using Hiper.Api.Models.Enums;
using Hiper.Api.Repositories;
using Microsoft.AspNet.Identity;
using SendGrid;

namespace Hiper.Api.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly UserRepository _repoUser;
        private readonly GoalRepository _repoGoal;
        private readonly TeamRepository _repoTeam;

        public AccountController()
        {
            var context = new AppContext();
            _repoUser = new UserRepository(context);
            _repoGoal = new GoalRepository(context);
            _repoTeam = new TeamRepository(context);
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(ProfileModel profileModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            profileModel.UserName = profileModel.Email;

            var result = await _repoUser.RegisterUser(profileModel);
            if (result.Succeeded)
            {
                 // Send an email with this link
                var user = _repoUser.FindUserByUserName(profileModel.UserName);
                var code = await _repoUser.GenerateEmailConfirmationTokenAsync(user.Id);


                var callbackUrl = Url.Link("ConfirmEmail", new {userId = user.Id, code});


                var confirmMail =
                    String.Format(UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailConfirmation"]),
                        user.FirstName + " " + user.LastName, callbackUrl);

                await _repoUser.SendEmailAsync(user.Id,
                    "Confirm your account", MailHelper.PrepareAutoReplyEmail(confirmMail)
                    );
            }


            var errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }


            return Ok();
        }

        [Authorize]
        [Route("UpdateUserProfile")]
        public async Task<IHttpActionResult> UpdateUserProfile(ProfileModel profileModel)
        {

            var result = await _repoUser.UpdateUser(profileModel);

            var errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }


            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repoUser.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        [Authorize]
        [HttpPost]
        [Route("GetUserProfileEdit")]
        public IHttpActionResult GetUserProfileEdit([FromBody] string username)
        {
            var user = _repoUser.FindUserByUserName(username);
            var result = Mapper.Map<UserModel, ProfileModel>(user);
            result.Picture = UploadHelper.GetCurrentProfileImageUrl(user.Id);
            return Ok(result);
        }

        [Authorize]
        [Route("GetUserProfileLanguage")]
        public IHttpActionResult GetUserProfileLanguage()
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var result = new {currentUser.Language};
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("SaveUserProfileLanguage")]
        public async Task<IHttpActionResult> SaveUserProfileLanguage([FromBody] string language)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            currentUser.Language = language;
            var result = await _repoUser.UpdateUser(currentUser);
            var errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("GetUserProfile")]
        public IHttpActionResult GetUserProfile([FromBody] string username)
        {
            var user = _repoUser.FindUserByUserName(username);

            var profile = Mapper.Map<UserModel, ProfileModel>(user);
            profile.Picture = UploadHelper.GetCurrentProfileImageUrl(user.Id);


            var teams = _repoTeam.FindBy(r => r.Users.Any(u => u.UserName == profile.UserName)).Select(r => new {name = r.TeamName, id = r.TeamId});
            var goals = _repoGoal.FindBy(r => r.Participants.Any(u => u.UserName == profile.UserName) && teams.Any(t => t.id == r.TeamId) && r.StatusOfGoalId == (int) StatusGoalEnum.Active).Select(r => new {name = r.Title, teamName = r.Team != null ? r.Team.TeamName : user.FirstName + " " + user.LastName});
            var result = new
            {
                profile,
                goals,
                teams
            };
            return Ok(result);
        }


        [Route("GetTotalNumberOfUsers")]
        public IHttpActionResult GetTotalNumberOfUsers()
        {
            return Ok(_repoUser.GetAll().Count());
        }

        [Route("GetTotalNumberOfTeamUsers")]
        public IHttpActionResult GetTotalNumberOfTeamUsers()
        {
            return Ok(_repoUser.FindBy(u => u.Teams.Count > 0).Count());
        }

        [Route("GetTotalNumberOfNonTeamUsers")]
        public IHttpActionResult GetTotalNumberOfNonTeamUsers()
        {
            return Ok(_repoUser.FindBy(u => u.Teams.Count == 0).Count());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail", Name = "ConfirmEmail")]
        public async Task<IHttpActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                BadRequest();
            }
            IdentityResult result;
            try
            {
                result = await _repoUser.ConfirmEmailAsync(userId, code);
            }
            catch (InvalidOperationException ioe)
            {
                // ConfirmEmailAsync throws when the userId is not found.

                return BadRequest(ioe.ToString());
            }

            if (result.Succeeded)
            {
                return Redirect(ConfigurationManager.AppSettings["webClientAddress"] + "#/teamCreateJoin");
            }


            return BadRequest();
        }

        [Authorize]
        [HttpPost]
        [Route("ResendConfirmation")]
        public async Task<IHttpActionResult> ResendConfirmation([FromBody] string username)
        {
            var user = _repoUser.FindUserByUserName(username);
            var code = await _repoUser.GenerateEmailConfirmationTokenAsync(user.Id);
         
              var  callbackUrl = Url.Link("ConfirmEmail", new {userId = user.Id, code});
           
           
            var confirmMail =
                String.Format(UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailConfirmation"]),
                    user.FirstName + " " + user.LastName, callbackUrl);

            await _repoUser.SendEmailAsync(user.Id,
                "Confirm your account", MailHelper.PrepareAutoReplyEmail(confirmMail)
                );
            return Ok();
        }

        [AllowAnonymous]
        [Route("ForgotPassword", Name = "ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword([FromBody] string email)
        {
            
            var user = _repoUser.FindUserByUserName(email);
            if (user == null || !(_repoUser.IsEmailConfirmed(user.Id)))
            {
                ModelState.AddModelError("email", "This e-mail adress is unknown.");
                return BadRequest(ModelState);
            }

            var code = await _repoUser.GeneratePasswordResetTokenAsync(user.Id);
          

           
             var   callbackUrl = Url.Link("ResetPassword", new {userId = user.Id, code});
            
           
            var message = String.Format(UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailResetPassword"]), callbackUrl);
            await _repoUser.SendEmailAsync(user.Id,
                "Password reset", MailHelper.PrepareAutoReplyEmail(message));

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ResetPassword", Name = "ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(string userId, string code)
        {
            if (userId == null || code == null)
            {
                BadRequest();
            }
            IdentityResult result;
            var password = Guid.NewGuid().ToString().ToLower()
                .Replace("-", "").Replace("l", "").Replace("1", "").Replace("o", "").Replace("0", "")
                .Substring(0, 6);
            try
            {
                result = await _repoUser.ResetPasswordAsync(userId, code, password);
            }
            catch (InvalidOperationException ioe)
            {
                // ConfirmEmailAsync throws when the userId is not found.

                return BadRequest(ioe.ToString());
            }

            if (result.Succeeded)
            {
                var user = _repoUser.FindUserById(userId);
                var emailService = new EmailService();
                var message = new SendGridMessage();
                message.AddTo(user.Email);
                message.Subject = "Your password changed";
                var email =
                    String.Format(
                        UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailPasswordChanged"]),
                        password);
                message.Html = MailHelper.PrepareAutoReplyEmail(email);
                await emailService.SendAsync(message);

                return Redirect(ConfigurationManager.AppSettings["webClientAddress"] + "#/login");
            }

       return BadRequest();
        }
    }
}