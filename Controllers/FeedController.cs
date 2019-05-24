using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using Hiper.Api.Helpers;
using Hiper.Api.Models;
using Hiper.Api.Models.Enums;
using Hiper.Api.Repositories;

namespace Hiper.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Feed")]
    public class FeedController : ApiController
    {
        private readonly UserRepository _repoUser;
        private readonly TeamFeedRepository _repoFeed;
        private readonly GoalRepository _repoGoal;
        private readonly HipeRepository _repoHipe;
        private readonly TeamRepository _repoTeam;

        public FeedController()
        {
            var context = new AppContext();
            _repoFeed = new TeamFeedRepository(context);
            _repoUser = new UserRepository(context);
            _repoGoal = new GoalRepository(context);
            _repoHipe = new HipeRepository(context);
            _repoTeam = new TeamRepository(context);
        }

        [HttpPost]
        [Route("GetFeedsByFilter")]
        public IHttpActionResult GetFeedsByFilter(FilterViewModel filter)
        {
            List<TeamFeedModel> feeds;
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            if (filter != null)
            {
                filter.LastDate = filter.LastDate == filter.TimeTo ? filter.LastDate = DateTime.UtcNow : filter.LastDate;


                feeds =
                    _repoFeed.FindBy(
                        t =>
                            (filter.TypeOfUpdate == -1 || t.UpdateTypeId == filter.TypeOfUpdate)
                            && (filter.TypeOfGoal == -1 || t.Goal.GoalTypeId == filter.TypeOfGoal)
                            && (filter.TeamId == -1 ? t.User != null && t.UserId == currentUser.Id && t.Team == null :
                                (filter.TeamMember == "-1" ? t.TeamId == filter.TeamId : t.TeamId == filter.TeamId && (t.User.UserName == filter.TeamMember || (t.Goal != null && t.Goal.Participants.Any(p => p.UserName == filter.TeamMember)))))
                            && DbFunctions.TruncateTime(t.CreationDate) >= DbFunctions.TruncateTime(filter.TimeFrom)
                            && DbFunctions.TruncateTime(t.CreationDate) <= DbFunctions.TruncateTime(filter.TimeTo)
                            && (t.CreationDate < filter.LastDate)
                        ).OrderByDescending(x => x.CreationDate).Take(filter.ScrollParam).ToList();
            }
            else
            {
                feeds = _repoFeed.GetAll().ToList();
            }

            var team = filter != null && filter.TeamId == -1 ? null : _repoTeam.FindBy(t => t.TeamId == filter.TeamId).FirstOrDefault();

            var result = feeds.Select(f => new
            {
                FeedId = f.TeamFeedId,
                UserName = f.User == null ? f.Team.Administrator.UserName : f.User.UserName,
                IsDeleted = f.User != null && team != null && team.Users.All(u => u.Id != f.User.Id),
                FirstName = f.User == null ? "" : f.User.FirstName,
                LastName = f.User == null ? "" : f.User.LastName,
                Users = f.User == null ? f.UpdateTypeId == (int) UpdateTypeEnum.ReceivedNewGoal
                                         || f.UpdateTypeId == (int) UpdateTypeEnum.MissedDeadline
                                         || f.UpdateTypeId == (int) UpdateTypeEnum.OneDayLeft
                                         || f.UpdateTypeId == (int) UpdateTypeEnum.OneWeekLeft
                                         || f.UpdateTypeId == (int) UpdateTypeEnum.ThreeDaysLeft ?
                    f.Goal.Participants.Select(p => new {p.FirstName, p.LastName, p.UserName, isDeleted = f.Goal.Team.Users.All(u => u.Id != p.Id)}).ToList() : null :
                    (f.Goal != null && f.Goal.IsTeamGoal) ?
                        f.Goal.Participants.Select(p => new {p.FirstName, p.LastName, p.UserName, isDeleted = f.Goal.Team.Users.All(u => u.Id != p.Id)}).ToList() :
                        new[] {new {f.User.FirstName, f.User.LastName, f.User.UserName, isDeleted = team != null && team.Users.All(u => u.Id != f.User.Id)}}.ToList(),
                NickName = f.User == null ? "" : f.User.NickName,
                Title = f.User == null ? "" : f.User.Title,
                picture = f.User == null ? UploadHelper.GetTeamProfileImageurl() : (f.Goal != null && f.Goal.IsTeamGoal && (
                    f.UpdateTypeId == (int) UpdateTypeEnum.AchievedGoal
                    || f.UpdateTypeId == (int) UpdateTypeEnum.DidntAchievedGoal
                    || f.UpdateTypeId == (int) UpdateTypeEnum.CancelledGoal)) ? UploadHelper.GetTeamProfileImageurl() : UploadHelper.GetCurrentProfileImageUrl(f.User.Id),
                f.UpdateType.UpdateTypeDescription,
                GoalDescription = f.Goal == null ? "" : f.Goal.Title,
                GoalId = f.Goal == null ? -1 : f.GoalId,
                Hipes = f.Goal == null ? _repoHipe.FindBy(h => h.FeedId == f.TeamFeedId).ToList().Count() : _repoHipe.FindBy(h => h.GoalId == f.GoalId).ToList().Count(),
                f.CreationDate, f.TeamId,
                IsHiped = f.Goal == null ? _repoHipe.FindBy(h => h.UserId == currentUser.Id && h.FeedId == f.TeamFeedId).ToList().Any() : _repoHipe.FindBy(h => h.UserId == currentUser.Id && h.GoalId == f.GoalId).ToList().Any() || (!f.Goal.IsTeamGoal && f.Goal.Participants.Any(p => p.Id == currentUser.Id)),
                DateFrom = "",
                TargetAmount = f.Goal == null ? "" : f.Goal.GoalTypeId == (int) GoalTypeEnum.SucceedFail ? "" : f.Goal.TargetAmount.ToString(),
                ReachedAmount = f.Goal == null ? "" : f.Goal.GoalTypeId == (int) GoalTypeEnum.SucceedFail ? "" : f.CurrentAmount == null ? f.Goal.ReachedAmount.ToString() : f.CurrentAmount.ToString()
            }).ToList();
            return Ok(result);
        }

        [AllowAnonymous]
        [Route("GetUpdateTypes")]
        public IHttpActionResult GetUpdateTypes()
        {
            return Ok(_repoFeed.GetUpdateTypes().Select(r => new {id = r.UpdateTypeId, name = r.UpdateTypeDescription}));
        }

        [Route("Hipe")]
        public IHttpActionResult Hipe([FromBody] int id)
        {
            var feeds = _repoFeed.FindBy(f => f.TeamFeedId == id).FirstOrDefault();
            var identity = (ClaimsIdentity) User.Identity;
            var user = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            if (feeds != null && user != null)
            {
                _repoHipe.Add(new HipeModel {Feed = feeds, User = user, CreationDate = DateTime.UtcNow});
                return Ok();
            }
            return BadRequest();
        }

        [Route("GetFeedFilterData")]
        public IHttpActionResult FeedFilterData([FromBody] int teamId)
        {
            var teamUsers = _repoUser.FindUsersByTeamId(teamId);

            var users = teamUsers.Select(r => new
            {
                name = r.FirstName + " " + r.LastName,
                id = r.UserName
            });

            var goalTypes = _repoGoal.GetGoalTypes().Select(r => new {id = r.GoalTypeId, name = r.GoalTypeDescription});
            var updateTypes =
                _repoFeed.GetUpdateTypes().Select(r => new {id = r.UpdateTypeId, name = r.UpdateTypeDescription});

            var feedbacks = _repoGoal.GetSurveys().Select(r => new {id = r.SurveyId, name = r.SurveyDescription});
            var statuses =
                _repoGoal.GetGoalStatuses().Where(r => r.StatusGoalId != (int) StatusGoalEnum.Active).Select(r => new {id = r.StatusGoalId, name = r.StatusGoalDescription});
            var result = new
            {
                users,
                goalTypes,
                updateTypes,
                feedbacks,
                statuses
            };
            return Ok(result);
        }
    }
}
