using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using Hiper.Api.Models;
using Hiper.Api.Models.Enums;
using Hiper.Api.Repositories;

namespace Hiper.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Goal")]
    public class GoalController : ApiController
    {
        private readonly UserRepository _repoUser;
        private readonly TeamFeedRepository _repoFeed;
        private readonly GoalRepository _repoGoal;
        private readonly TeamRepository _repoTeam;
        private readonly HipeRepository _repoHipe;

        public GoalController()
        {
            var context = new AppContext();
            _repoTeam = new TeamRepository(context);
            _repoUser = new UserRepository(context);
            _repoFeed = new TeamFeedRepository(context);
            _repoGoal = new GoalRepository(context);
            _repoHipe = new HipeRepository(context);
        }

        [Route("CreateGoal")]
        public IHttpActionResult CreateGoal(GoalCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
           
            if (currentUser != null)
            {
                var goal = Mapper.Map<GoalCreateViewModel, GoalModel>(model);
                goal.CreatedDate = DateTime.UtcNow;
                goal.TeamId = model.TeamId == -1 ? null : model.TeamId;
                goal.IsTeamGoal = false;
                goal.Participants = new List<UserModel> {currentUser};
              
                _repoFeed.Add(new TeamFeedModel
                {
                    Goal = goal,
                    User = currentUser,
                    Team = team,
                    UpdateTypeId = (int) UpdateTypeEnum.AddedGoal,
                    CreationDate = DateTime.UtcNow,
                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                });
            }

          


            return Ok();
        }

        [Route("CreateTeamGoal")]
        public IHttpActionResult CreateTeamGoal(GoalCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
            var isAdmin = team != null && team.AdministratorId == currentUser.Id;
            if (currentUser != null && isAdmin)
            {
                var goal = Mapper.Map<GoalCreateViewModel, GoalModel>(model);
                goal.CreatedDate = DateTime.UtcNow;
                var participants = new List<UserModel>();
                if (model.TeamUsers.Length > 0)
                    participants.AddRange(model.TeamUsers.Select(user => _repoUser.FindUserByUserName(user)));

                goal.Participants = participants;
                goal.IsTeamGoal = true;
               
                _repoFeed.Add(new TeamFeedModel
                {
                    Goal = goal,
                    User = null,
                    Team = team,
                    UpdateTypeId = (int?) UpdateTypeEnum.ReceivedNewGoal,
                    CreationDate = DateTime.UtcNow,
                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                });
            }

          


            return Ok();
        }

        [Route("UserGoalPage")]
        public IHttpActionResult UserGoalPage([FromBody] string userName)
        {
            var goals = _repoGoal.FindBy(g => g.Participants.Any(u => u.UserName == userName)).ToList();
            return Ok(goals);
        }

        [Route("GoalById")]
        public IHttpActionResult GoalById([FromBody] int goalId)
        {
            var goal = _repoGoal.FindBy(r => r.GoalId == goalId).FirstOrDefault();
            var result = Mapper.Map<GoalModel, GoalViewModel>(goal);
            if (goal != null)
            {
                result.TeamUsers = goal.Participants.Select(p => p.UserName).ToArray();
                result.Hipes = _repoHipe.FindBy(h => h.GoalId == goal.GoalId).ToList().Count();
            }
            return Ok(result);
        }

        [Route("UpdateGoal")]
        public IHttpActionResult UpdateGoal(GoalViewModel goalView)
        {
            var goal = _repoGoal.FindBy(g => g.GoalId == goalView.GoalId).FirstOrDefault();
            var user = _repoUser.FindUserByUserName(goalView.UserName);
            if (goal != null)
            {
                goal = Mapper.Map(goalView, goal);
                goal.Surveys = goalView.SurveysId != null
                    ? goalView.SurveysId.Select(sur => _repoGoal.GetSurveys().FirstOrDefault(s => s.SurveyId == sur))
                        .ToList()
                    : null;
                var participants = new List<UserModel>();
                if (goalView.TeamUsers.Length > 0)
                    participants.AddRange(goalView.TeamUsers.Select(u => _repoUser.FindUserByUserName(u)));
                goal.Participants.Clear();

                if (goal.StatusOfGoalId != null && GetUpdateTypeByGoalStatus((int) goal.StatusOfGoalId) > -1)
                {
                    goal.ClosedDate = DateTime.UtcNow;
                }

                
                goal.Participants = participants;
                _repoGoal.Edit(goal);


                if (goal.StatusOfGoalId != null)
                {
                    var updateStatus = GetUpdateTypeByGoalStatus((int) goal.StatusOfGoalId);
                    if (updateStatus > -1)
                    {
                        var feed =
                            _repoFeed.FindBy(
                                f =>
                                    f.GoalId == goal.GoalId && f.TeamId == goal.TeamId && f.UpdateTypeId == updateStatus)
                                .FirstOrDefault();
                        if (feed == null)
                        {
                            _repoFeed.Add(new TeamFeedModel
                            {
                                Goal = goal,
                                User = user,
                                Team = goal.Team,
                                UpdateTypeId = updateStatus,
                                CreationDate = DateTime.UtcNow
                            });
                        }


                        var goalToSave = Mapper.Map<GoalModel, GoalModel>(goal);
                        if (goalToSave.GoalTypeId == (int) GoalTypeEnum.Number)
                        {
                            goalToSave.ReachedAmount = 0;
                        }
                        goalToSave.StatusOfGoal = null;
                        goalToSave.Surveys = new List<SurveyModel>();

                        if (
                            goalToSave.RepeatId == (int) RepeatEnum.Day)
                        {
                            goalToSave.StatusOfGoalId = (int) StatusGoalEnum.Active;

                            goalToSave.DeadLine = DateTime.UtcNow.AddDays(1);
                            _repoGoal.Add(goalToSave);
                        }
                        if (
                            goalToSave.RepeatId == (int) RepeatEnum.Week)
                        {
                            goalToSave.StatusOfGoalId = (int) StatusGoalEnum.Active;
                            goalToSave.DeadLine = goalToSave.DeadLine.AddDays(7);
                            _repoGoal.Add(goalToSave);
                        }
                        if (
                            goal.RepeatId == (int) RepeatEnum.Month)
                        {
                            goalToSave.StatusOfGoalId = (int) StatusGoalEnum.Active;
                            goalToSave.DeadLine = goalToSave.DeadLine.AddMonths(1);
                            _repoGoal.Add(goalToSave);
                        }
                    }
                    else
                    {
                        if (goal.ReachedAmount/goal.TargetAmount >= 0.25 && goal.ReachedAmount/goal.TargetAmount < 0.5)
                        {
                            var checkIsFeeded = _repoFeed.FindBy(f => f.GoalId == goal.GoalId && f.UpdateTypeId == (int?) UpdateTypeEnum.Achieved25OfGoal).ToList().Count > 0;
                            if (!checkIsFeeded)
                            {
                                _repoFeed.Add(new TeamFeedModel
                                {
                                    Goal = goal,
                                    User = user,
                                    Team = goal.Team,
                                    UpdateTypeId = (int?) UpdateTypeEnum.Achieved25OfGoal,
                                    CreationDate = DateTime.UtcNow,
                                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                                });
                            }
                        }
                        else if (goal.ReachedAmount/goal.TargetAmount >= 0.5 && goal.ReachedAmount/goal.TargetAmount < 0.75)
                        {
                            var checkIsFeeded = _repoFeed.FindBy(f => f.GoalId == goal.GoalId && f.UpdateTypeId == (int?) UpdateTypeEnum.Achieved50OfGoal).ToList().Count > 0;
                            if (!checkIsFeeded)
                            {
                                _repoFeed.Add(new TeamFeedModel
                                {
                                    Goal = goal,
                                    User = user,
                                    Team = goal.Team,
                                    UpdateTypeId = (int?) UpdateTypeEnum.Achieved50OfGoal,
                                    CreationDate = DateTime.UtcNow,
                                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                                });
                            }
                        }
                        else if (goal.ReachedAmount/goal.TargetAmount >= 0.75)
                        {
                            var checkIsFeeded = _repoFeed.FindBy(f => f.GoalId == goal.GoalId && f.UpdateTypeId == (int?) UpdateTypeEnum.Achieved25OfGoal).ToList().Count > 0;
                            if (!checkIsFeeded)
                            {
                                _repoFeed.Add(new TeamFeedModel
                                {
                                    Goal = goal,
                                    User = user,
                                    Team = goal.Team,
                                    UpdateTypeId = (int?) UpdateTypeEnum.Achieved75OfGoal,
                                    CreationDate = DateTime.UtcNow,
                                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                                });
                            }
                        }
                    }
                }


                return Ok();
            }
            return BadRequest();
        }

        public int GetUpdateTypeByGoalStatus(int statusId)
        {
            var result = -1;
            switch (statusId)
            {
                case (int) StatusGoalEnum.Achieved:
                    result = (int) UpdateTypeEnum.AchievedGoal;
                    break;
                case (int) StatusGoalEnum.Cancel:
                    result = (int) UpdateTypeEnum.CancelledGoal;
                    break;
                case (int) StatusGoalEnum.DidntAchievied:
                    result = (int) UpdateTypeEnum.DidntAchievedGoal;
                    break;
            }
            return result;
        }

        [Route("SetGoalReachedAmount")]
        public IHttpActionResult SetGoalReachedAmount(int goalId, double amount)
        {
            var goal = _repoGoal.GetSingle(goalId);
            goal.ReachedAmount = amount;


            _repoGoal.Edit(goal);
            return Ok();
        }

        [AllowAnonymous]
        [Route("GetGoalSurveys")]
        public IHttpActionResult GetGoalSurveys()
        {
            return Ok(_repoGoal.GetSurveys().Select(r => new {id = r.SurveyId, name = r.SurveyDescription}));
        }

        [AllowAnonymous]
        [Route("GetGoalTypes")]
        public IHttpActionResult GetGoalTypes()
        {
            return Ok(_repoGoal.GetGoalTypes().Select(r => new {id = r.GoalTypeId, name = r.GoalTypeDescription}));
        }

        [AllowAnonymous]
        [Route("GetGoalRepeats")]
        public IHttpActionResult GetGoalRepeats()
        {
            return Ok(_repoGoal.GetRepeates().Select(r => new {id = r.RepeatId, name = r.RepeatDescription}));
        }

        [AllowAnonymous]
        [Route("GetGoalStatuses")]
        public IHttpActionResult GetGoalStatuses()
        {
            return Ok(_repoGoal.GetGoalStatuses().Select(r => new {id = r.StatusGoalId, name = r.StatusGoalDescription}));
        }


        [Route("Hipe")]
        public IHttpActionResult Hipe([FromBody] int id)
        {
            var goal = _repoGoal.FindBy(f => f.GoalId == id).FirstOrDefault();
            var identity = (ClaimsIdentity) User.Identity;
            var user = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            if (goal != null && user != null)
            {
                _repoHipe.Add(new HipeModel {Goal = goal, User = user, CreationDate = DateTime.UtcNow});
                return Ok();
            }
            return BadRequest();
        }
    }
}
