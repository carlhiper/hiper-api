using System;
using System.Collections.Generic;
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
    [Authorize]
    [RoutePrefix("api/Team")]
    public class TeamController : ApiController
    {
        private readonly TeamRepository _repoTeam;
        private readonly UserRepository _repoUser;
        private readonly TeamFeedRepository _repoFeed;

        public TeamController()
        {
            var context = new AppContext();
            _repoTeam = new TeamRepository(context);
            _repoUser = new UserRepository(context);
            _repoFeed = new TeamFeedRepository(context);
        }


        [Route("CreateTeam")]
        public async Task<IHttpActionResult> CreateTeam(TeamCreationViewModel teamModel)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_repoTeam.FindBy(t => t.TeamName == teamModel.TeamName && t.AdministratorId == currentUser.Id).ToList().Any())
            {
                ModelState.AddModelError("", "Team with team name - " + teamModel.TeamName + " already exists");
                return BadRequest(ModelState);
            }

            var team = Mapper.Map<TeamCreationViewModel, TeamModel>(teamModel);
            team.AdministratorId = currentUser.Id;
            team.RegDate = DateTime.UtcNow;
            team.Active = true;
            team.TeamType = teamModel.TeamTypeCode == null ? null : _repoTeam.GetAllTeamTypes().FirstOrDefault(t => t.TeamTypeDescription == teamModel.TeamTypeCode);

            var resultTeam = _repoTeam.Add(team);
            currentUser.Teams.Add(resultTeam);

            _repoFeed.Add(new TeamFeedModel
            {
                UpdateTypeId = (int?) UpdateTypeEnum.IsNewMember,
                Team = resultTeam,
                User = currentUser,
                CreationDate = DateTime.UtcNow
            });
          

            var emailService = new EmailService();
            var adminMessage = new SendGridMessage();
            adminMessage.AddTo(currentUser.Email);
            adminMessage.Subject = "Team created";
            var adminMail =
                String.Format(
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailAdminTeamCreation"]),
                    team.TeamName, team.Password);
            adminMessage.Html = MailHelper.PrepareAutoReplyEmail(adminMail);
            await emailService.SendAsync(adminMessage);

            foreach (var email in teamModel.Emails)
            {
                if (email != "" && MailHelper.CheckIsEmail(email))
                {
                    var message = new SendGridMessage();
                    message.AddTo(email);
                    message.Subject = "Team invitation";
                    var invite = String.Format(UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailInviteToTeam"]), team.TeamName, team.Password, team.Administrator.Email);

                    message.Html = MailHelper.PrepareAutoReplyEmail(invite);
                    
                    await emailService.SendAsync(message);
                }
            }


            return Ok();
        }

        [Route("Team")]
        public IHttpActionResult Team([FromBody] int teamId)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var resultTeam = new List<ProfileModel>();
            var team = _repoTeam.FindBy(t => t.TeamId == teamId).FirstOrDefault();
            var isAdmin = team != null && team.AdministratorId == currentUser.Id;
            if (team != null && teamId != -1)
            {
                var teamUsers = _repoUser.FindUsersByTeamId(teamId);
                foreach (var teamMember in teamUsers)
                {
                    teamMember.Picture = UploadHelper.GetCurrentProfileImageUrl(teamMember.Id);
                }
                var teamApplicants = isAdmin ? _repoUser.FindApplicantsByTeamId(teamId) : new List<UserModel>();
                foreach (var teamMember in teamApplicants)
                {
                    teamMember.Picture = UploadHelper.GetCurrentProfileImageUrl(teamMember.Id);
                }
                var teamApplicatnsProfiles = Mapper.Map<List<UserModel>, List<ProfileModel>>(teamApplicants);
                foreach (var profile in teamApplicatnsProfiles)
                {
                    profile.IsApplicant = true;
                }

                resultTeam = Mapper.Map<List<UserModel>, List<ProfileModel>>(teamUsers);
                if (team.Administrator != null)
                {
                    foreach (var member in resultTeam.Where(member => team.Administrator.UserName == member.UserName))
                    {
                        member.IsAdmin = true;
                        break;
                    }
                }

                resultTeam.AddRange(teamApplicatnsProfiles);
                resultTeam = resultTeam.OrderBy(p => p.FirstName).ToList();
            }
            else
            {
                var currentProfile = Mapper.Map<UserModel, ProfileModel>(currentUser);
                currentProfile.Picture = UploadHelper.GetCurrentProfileImageUrl(currentUser.Id);
                resultTeam.Add(currentProfile);
            }
            var result = new
            {
                profileList = resultTeam,
                isAdmin,
                maxMemberCount = team != null && team.TeamType != null ? team.TeamType.TeamMembersCount : -1
            };

            return Ok(result);
        }

        [Route("JoinTeam")]
        public async Task<IHttpActionResult> JoinTeam(JoinTeamViewModel teamView)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(m => m.TeamName == teamView.TeamName && m.Password == teamView.Password && (teamView.AdminEmail == null || teamView.AdminEmail == m.Administrator.Email)).FirstOrDefault();
            if (team != null && currentUser != null)
            {
                if (currentUser.Teams == null)
                    currentUser.Teams = new List<TeamModel>();
                else if (currentUser.Teams.Any(t => t.TeamId == team.TeamId))
                {
                    ModelState.AddModelError("email", "You already joined to team:" + team.TeamName);
                    return BadRequest(ModelState);
                }
                currentUser.TeamsApplicants.Add(team);
                var result = await _repoUser.UpdateUser(currentUser);
                var errorResult = GetErrorResult(result);

                if (errorResult != null)
                {
                    return errorResult;
                }
            }
            else if (team == null)
            {
                ModelState.AddModelError("", "Team not finded for name:" + teamView.TeamName + "  and administrator`s email :" + teamView.AdminEmail);
                return BadRequest(ModelState);
            }
            else
            {
                return BadRequest(RequestContext.Principal.Identity.GetUserId());
            }
            return Ok(team.TeamId);
        }

        [Route("AddUsersToTeam")]
        public async Task<IHttpActionResult> AddUsersToTeam(ApprouveUsersViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
            var isAdmin = team != null && team.AdministratorId == currentUser.Id;
            var email = new EmailService();


            if (currentUser != null && isAdmin)
            {
                foreach (var userName in model.TeamUsers)
                {
                    if (MailHelper.CheckIsEmail(userName))
                    {
                        var message = new SendGridMessage();

                        message.AddTo(userName);

                        message.Subject = "Team invitation";


                        var invite = String.Format(UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailInviteToTeam"]), team.TeamName, team.Password, team.Administrator.Email);

                        message.Html = MailHelper.PrepareAutoReplyEmail(invite);

                        await email.SendAsync(message);
                    }
                }
                return Ok();
            }
            return BadRequest();
        }

        [Route("ApprouveUsers")]
        public async Task<IHttpActionResult> ApprouveUsers(ApprouveUsersViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
            var isAdmin = team != null && team.AdministratorId == currentUser.Id;
            if (currentUser != null && isAdmin)
            {
                foreach (var userName in model.TeamUsers)
                {
                    var user = _repoUser.FindUserByUserName(userName);
                    user.TeamsApplicants.Remove(team);
                    if (user.Teams.All(t => t.TeamId != team.TeamId))
                    {
                        user.Teams.Add(team);
                        await _repoUser.UpdateUser(user);
                        _repoFeed.Add(new TeamFeedModel
                        {
                            UpdateTypeId = (int?) UpdateTypeEnum.IsNewMember,
                            Team = team,
                            User = user,
                            CreationDate = DateTime.UtcNow
                        });
                    }
                }

                return Ok();
            }
            return BadRequest();
        }

        [Route("GetTeamsForUser")]
        public IHttpActionResult GetTeamsForUser()
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);

            if (currentUser != null)
            {
                var teams =
                    _repoTeam.FindBy(t => t.Users.Any(u => u.UserName == currentUser.UserName))
                        .Select(t => new {t.TeamId, t.TeamName, isAdmin = t.AdministratorId == currentUser.Id, active = t.Active});
                return Ok(teams);
            }
            return BadRequest();
        }

        [Route("RemoveUserFromTeam")]
        public async Task<IHttpActionResult> RemoveUserFromTeam(string userName, int teamId)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(m => m.TeamId == teamId).FirstOrDefault();
            if (currentUser != null && team != null && team.AdministratorId == currentUser.Id)
            {
                var user = _repoUser.FindUserByUserName(userName);
                user.Teams.Remove(user.Teams.FirstOrDefault(t => t.TeamId == teamId));
                var feed = new TeamFeedModel
                {
                    TeamId = teamId,
                    UserId = user.Id,
                    UpdateTypeId = (int?) UpdateTypeEnum.LeftTeam,
                    CreationDate = DateTime.UtcNow
                };
                _repoFeed.Add(feed);
                var result = await _repoUser.UpdateUser(user);
                var errorResult = GetErrorResult(result);

                if (errorResult != null)
                {
                    return errorResult;
                }
                return Ok();
            }
            return BadRequest();
        }

        [Route("RemoveUsersFromTeam")]
        public IHttpActionResult RemoveUsersFromTeam(RemoveUsersViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
            if (team != null && (currentUser != null && currentUser.Id == team.AdministratorId))
            {
                foreach (var teamUser in model.Users)
                {
                    var user = _repoUser.FindUserByUserName(teamUser);
                    team.Users.Remove(team.Users.FirstOrDefault(u => u.UserName == teamUser));
                    var feed = new TeamFeedModel
                    {
                        TeamId = model.TeamId,
                        UserId = user.Id,
                        UpdateTypeId = (int?) UpdateTypeEnum.LeftTeam,
                        CreationDate = DateTime.UtcNow
                    };
                    _repoFeed.Add(feed);
                }


                _repoTeam.Edit(team);

                return Ok();
            }
            return BadRequest();
        }


        [Route("RemoveTeamsForUser")]
        public async Task<IHttpActionResult> RemoveTeamsForUser(DeleteTeamsForUserViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            if (currentUser != null && currentUser.UserName == model.UserName)
            {
                var user = _repoUser.FindUserByUserName(model.UserName);
                foreach (var teamId in model.TeamIds)
                {
                    user.Teams.Remove(user.Teams.FirstOrDefault(t => model.TeamIds.Any(r => r == t.TeamId)));
                    var feed = new TeamFeedModel
                    {
                        TeamId = teamId,
                        UserId = user.Id,
                        UpdateTypeId = (int?) UpdateTypeEnum.LeftTeam,
                        CreationDate = DateTime.UtcNow
                    };
                    _repoFeed.Add(feed);
                }


                var result = await _repoUser.UpdateUser(user);
                var errorResult = GetErrorResult(result);

                if (errorResult != null)
                {
                    return errorResult;
                }
                return Ok();
            }
            return BadRequest();
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

        [Route("GetFilterUser")]
        public IHttpActionResult FilterUser([FromBody] int teamId)
        {
            var teamUsers = _repoUser.FindUsersByTeamId(teamId);

            var result = teamUsers.Select(r => new
            {
                name = r.FirstName + " " + r.LastName + " " + r.LastName,
                id = r.UserName
            });

            return Ok(result);
        }

        public IHttpActionResult GetTotalNumberOfTeams()
        {
            return Ok(_repoTeam.GetAll().Count());
        }
    }
}
