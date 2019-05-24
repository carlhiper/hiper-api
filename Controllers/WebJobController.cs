using System;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Hiper.Api.Helpers;
using Hiper.Api.Helpers.Services;
using Hiper.Api.Models;
using Hiper.Api.Models.Enums;
using Hiper.Api.Repositories;
using SendGrid;

namespace Hiper.Api.Controllers
{
    [RoutePrefix("api/job")]
    public class WebJobController : ApiController
    {
        private readonly UserRepository _repoUser;
        private readonly TeamFeedRepository _repoFeed;
        private readonly GoalRepository _repoGoal;
        private readonly TeamRepository _repoTeam;
        private readonly HipeRepository _repoHipe;

        public WebJobController()
        {
            var context = new AppContext();
            _repoFeed = new TeamFeedRepository(context);
            _repoUser = new UserRepository(context);
            _repoGoal = new GoalRepository(context);
            _repoHipe = new HipeRepository(context);
            _repoTeam = new TeamRepository(context);
        }

        [Route("check")]
        public async Task<IHttpActionResult> GetCheck()
        {
            var oneDayGoals =
                await
                    _repoGoal.FindByAsync(
                        g =>
                            g.StatusOfGoalId == (int) StatusGoalEnum.Active &&
                            DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) <= 1 &&
                            DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) >= 0);
            var oneWeakGoals =
                await
                    _repoGoal.FindByAsync(
                        g =>
                            g.StatusOfGoalId == (int) StatusGoalEnum.Active &&
                            DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) == 7 &&
                            DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) > 3);
            var threeDaysGoals =
                await
                    _repoGoal.FindByAsync(
                        g =>
                            g.StatusOfGoalId == (int) StatusGoalEnum.Active &&
                            DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) == 3 &&
                            DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) > 1);
            foreach (var goal in oneDayGoals)
            {
                _repoFeed.Add(new TeamFeedModel
                {
                    User = !goal.IsTeamGoal ? goal.Participants.FirstOrDefault() : null,
                    Goal = goal,
                    Team = goal.Team,
                    UpdateTypeId = (int?) UpdateTypeEnum.OneDayLeft,
                    CreationDate = DateTime.UtcNow,
                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                });
            }
            foreach (var goal in oneWeakGoals)
            {
                _repoFeed.Add(new TeamFeedModel
                {
                    User = !goal.IsTeamGoal ? goal.Participants.FirstOrDefault() : null,
                    Goal = goal,
                    Team = goal.Team,
                    UpdateTypeId = (int?) UpdateTypeEnum.OneWeekLeft,
                    CreationDate = DateTime.UtcNow,
                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                });
            }
            foreach (var goal in threeDaysGoals)
            {
                _repoFeed.Add(new TeamFeedModel
                {
                    User = !goal.IsTeamGoal ? goal.Participants.FirstOrDefault() : null,
                    Goal = goal,
                    Team = goal.Team,
                    UpdateTypeId = (int?) UpdateTypeEnum.ThreeDaysLeft,
                    CreationDate = DateTime.UtcNow,
                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                });
            }
            var goals = await
                _repoGoal.FindByAsync(
                    g =>
                        g.StatusOfGoalId == (int) StatusGoalEnum.Active &&
                        DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) < 0 && DbFunctions.DiffDays(DateTime.UtcNow, g.DeadLine) >= -1);
            foreach (var goal in goals)
            {
                _repoFeed.Add(new TeamFeedModel
                {
                    User = !goal.IsTeamGoal ? goal.Participants.FirstOrDefault() : null,
                    Goal = goal,
                    Team = goal.Team,
                    UpdateTypeId = (int?) UpdateTypeEnum.MissedDeadline,
                    CreationDate = DateTime.UtcNow,
                    CurrentAmount = goal.GoalTypeId == (int) GoalTypeEnum.Number ? goal.ReachedAmount : null
                });
            }
            return Ok();
        }


        [HttpGet]
        [Route("MailUpdate/{updateType}")]
        public async Task<IHttpActionResult> MemberUpdate(string updateType)
        {
            var dayCount = 0;
            switch (updateType)
            {
                case "Weekly":
                    dayCount = 7;
                    break;
                case "Monthy":
                    var date = DateTime.UtcNow;
                    dayCount = DateTime.DaysInMonth(date.Year, date.Month);
                    break;
                case "Quarterly":
                    dayCount = 120;
                    break;
            }
            await SendMembersMails(updateType, dayCount);
            await SendManagerEmails(updateType, dayCount);


            return Ok();
        }

        public async Task SendMembersMails(string updateType, int dayCount)
        {
            var users = (await _repoUser.GetAllAsync()).Where(u => u.EmailConfirmed);

            var activeTemplate =
                UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberActive"]);
            var goalsTemplate =
                UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberGoals"]);
            var deadlineTemplate =
                UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberDeadline"]);
            var feedbacksTemplate =
                UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberFeedbacks"]);
            var dateNow = DateTime.UtcNow;
            var emailService = new EmailService();
            foreach (var user in users)
            {
                foreach (var team in user.Teams)
                {
                    if (team.AdministratorId != null && team.AdministratorId != user.Id && !string.IsNullOrEmpty(user.Email) && MailHelper.CheckIsEmail(user.Email))
                    {
                        var active = new StringBuilder();
                        var activeGoals =
                            await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.StatusOfGoalId == (int) StatusGoalEnum.Active);
                        foreach (var goal in activeGoals)
                        {
                            var amount = goal.ReachedAmount == null
                                ? 0
                                : goal.ReachedAmount/goal.TargetAmount == null ? 1 : goal.TargetAmount;
                            if (amount != null)
                                active.Append(String.Format(activeTemplate, goal.Title,
                                    (int) (dateNow - goal.DeadLine).TotalDays,
                                    goal.GoalTypeId == (int) GoalTypeEnum.Number
                                        ? Math.Round((double) amount, 2).ToString(CultureInfo.InvariantCulture)
                                        : ""));
                        }
                        var lastPeriod =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, g.CreatedDate) <=
                                         dayCount)))
                                .Count;

                        var totalAmount =
                            (await
                                _repoGoal.FindByAsync(
                                    g => g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId)).Count;

                        var achievedGoals =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.StatusOfGoalId == (int) StatusGoalEnum.Achieved)).Count;
                        var notAchieved =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.StatusOfGoalId == (int) StatusGoalEnum.DidntAchievied)).Count;
                        var canceled =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.StatusOfGoalId == (int) StatusGoalEnum.Cancel)).Count;
                        var feeds =
                            (await
                                _repoFeed.FindByAsync(
                                    f =>
                                        f.TeamId == team.TeamId && f.UserId == user.Id &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <=
                                         dayCount)))
                                .Count;
                        var hipes =
                            (await
                                _repoHipe.FindByAsync(
                                    f =>
                                        f.UserId == user.Id &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <=
                                         dayCount)))
                                .Count;


                        var goals = String.Format(goalsTemplate, activeGoals.Count(), lastPeriod, totalAmount,
                            achievedGoals, notAchieved, canceled, feeds, hipes);


                        var achievedDeadlines =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.DeadLine < g.ClosedDate &&
                                        g.StatusOfGoalId == (int) StatusGoalEnum.Achieved)).Count;
                        var missedDeadlines =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.DeadLine > g.ClosedDate &&
                                        g.StatusOfGoalId != (int) StatusGoalEnum.Active)).Count;


                        var deadlines = String.Format(deadlineTemplate, achievedDeadlines, missedDeadlines);


                        var enTime =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime) &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <=
                                         dayCount)))
                                .Count;
                        var enRes =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource) &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <=
                                         dayCount)))
                                .Count;
                        var enSupp =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport) &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <=
                                         dayCount)))
                                .Count;
                        var enRightSkills =
                            (await
                                _repoGoal.FindByAsync(
                                    g =>
                                        g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                        g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills) &&
                                        (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <=
                                         dayCount)))
                                .Count;

                        var feedBacks = String.Format(feedbacksTemplate, enTime, enRightSkills, enSupp, enRes);

                        var messageText = MailHelper.PrepareUpdateMemberEmail(active.ToString(), goals, deadlines,
                            feedBacks, user.FirstName, team.TeamName, updateType);
                        var message = new SendGridMessage();
                        message.AddTo(user.Email);
                        message.Subject = "Hiper";

                        message.Html = messageText;

                        await emailService.SendAsync(message);
                    }
                }
            }
        }

        public async Task SendManagerEmails(string updateType, int dayCount)
        {
            var teams = await _repoTeam.GetAllAsync();

            if (teams != null)
            {
                var activeTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberActive"]);
                var goalsTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberGoals"]);
                var deadlineTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberDeadline"]);
                var feedbacksTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMemberFeedbacks"]);
                var infoTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateManagerInfo"]);
                var teamGoalsTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateManagerGoals"]);
                var teamDeadlineTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateManagerDeadline"]);
                var teamFeedbacksTemplate =
                    UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateManagerFeedbacks"]);
                var dateNow = DateTime.UtcNow;
                var emailService = new EmailService();
                {
                    foreach (var team in teams)
                    {
                        if (team.Administrator != null && team.Users.Count > 0 && !string.IsNullOrEmpty(team.Administrator.Email) && MailHelper.CheckIsEmail(team.Administrator.Email))
                        {
                            var user = team.Administrator;
                            var active = new StringBuilder();
                            var activeGoals =
                                await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.StatusOfGoalId == (int) StatusGoalEnum.Active);
                            foreach (var goal in activeGoals)
                            {
                                var amount = goal.ReachedAmount == null
                                    ? 0
                                    : goal.ReachedAmount/goal.TargetAmount == null ? 1 : goal.TargetAmount;
                                if (amount != null)
                                    active.Append(String.Format(activeTemplate, goal.Title,
                                        (int) (dateNow - goal.DeadLine).TotalDays,
                                        goal.GoalTypeId == (int) GoalTypeEnum.Number
                                            ? Math.Round((double) amount, 2).ToString(CultureInfo.InvariantCulture)
                                            : ""));
                            }
                            var lastPeriod =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.CreatedDate) <= dayCount)))
                                    .Count;

                            var totalAmount =
                                (await
                                    _repoGoal.FindByAsync(
                                        g => g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId)).Count;

                            var achievedGoals =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.StatusOfGoalId == (int) StatusGoalEnum.Achieved)).Count;
                            var notAchieved =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.StatusOfGoalId == (int) StatusGoalEnum.DidntAchievied)).Count;
                            var canceled =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.StatusOfGoalId == (int) StatusGoalEnum.Cancel)).Count;
                            var feeds =
                                (await
                                    _repoFeed.FindByAsync(
                                        f =>
                                            f.TeamId == team.TeamId && f.UserId == user.Id &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <= dayCount)))
                                    .Count;
                            var hipes =
                                (await
                                    _repoHipe.FindByAsync(
                                        f =>
                                            f.UserId == user.Id &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <= dayCount)))
                                    .Count;


                            var goals = String.Format(goalsTemplate, activeGoals.Count(), lastPeriod, totalAmount,
                                achievedGoals, notAchieved, canceled, feeds, hipes);


                            var achievedDeadlines =
                                (await
                                    _repoGoal.FindByAsync(
                                        g => g.TeamId == team.TeamId &&
                                             g.Participants.Any(u => u.Id == user.Id) && g.DeadLine < g.ClosedDate &&
                                             g.StatusOfGoalId == (int) StatusGoalEnum.Achieved)).Count;
                            var missedDeadlines =
                                (await
                                    _repoGoal.FindByAsync(
                                        g => g.TeamId == team.TeamId &&
                                             g.Participants.Any(u => u.Id == user.Id) && g.DeadLine > g.ClosedDate &&
                                             g.StatusOfGoalId != (int) StatusGoalEnum.Active)).Count;


                            var deadlines = String.Format(deadlineTemplate, achievedDeadlines, missedDeadlines);


                            var enTime =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;
                            var enRes =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;
                            var enSupp =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;
                            var enRightSkills =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.Participants.Any(u => u.Id == user.Id) && g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;

                            var feedBacks = String.Format(feedbacksTemplate, enTime, enRightSkills, enSupp, enRes);


                            var teamMembersCount = team.Users.Count;
                            var newTeamMembersCount =
                                (await
                                    _repoFeed.FindByAsync(
                                        f =>
                                            f.TeamId == team.TeamId &&
                                            f.UpdateTypeId == (int) UpdateTypeEnum.IsNewMember &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <= dayCount)))
                                    .Count;
                            var leftTeamMembersCount =
                                (await
                                    _repoFeed.FindByAsync(
                                        f =>
                                            f.TeamId == team.TeamId && f.UpdateTypeId == (int) UpdateTypeEnum.LeftTeam &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <= dayCount)))
                                    .Count;
                            var teamUpdatesCount =
                                (await
                                    _repoFeed.FindByAsync(
                                        f =>
                                            f.TeamId == team.TeamId &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <= dayCount)))
                                    .Count;

                            var teamHipes =
                                (await
                                    _repoHipe.FindByAsync(
                                        f => (DbFunctions.DiffDays(DateTime.UtcNow, f.CreationDate) <= 7))).Count(h => team.Users.Any(u => h.User.Id == u.Id));
                            var teamInfo = String.Format(infoTemplate, team.TeamName, teamMembersCount, enSupp,
                                newTeamMembersCount, leftTeamMembersCount, teamUpdatesCount, teamHipes);


                            var teamLastPeriod =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.CreatedDate) <= dayCount)))
                                    .Count;

                            var teamTotalAmount = (await _repoGoal.FindByAsync(g => g.TeamId == team.TeamId)).Count;

                            var teamAchievedGoals =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId && g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))
                                    .Count;
                            var teamNotAchieved =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId &&
                                            g.StatusOfGoalId == (int) StatusGoalEnum.DidntAchievied)).Count;
                            var teamCanceled =
                                (await
                                    _repoGoal.FindByAsync(
                                        g => g.TeamId == team.TeamId && g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))
                                    .Count;
                            var teamGoals = String.Format(teamGoalsTemplate, teamTotalAmount, teamAchievedGoals,
                                teamNotAchieved, teamCanceled, teamLastPeriod);


                            var teamAchievedDeadlines =
                                (await
                                    _repoGoal.FindByAsync(
                                        g => g.TeamId == team.TeamId &&
                                             g.DeadLine < g.ClosedDate &&
                                             g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))
                                    .Count;
                            var teamMissedDeadlines =
                                (await
                                    _repoGoal.FindByAsync(
                                        g => g.TeamId == team.TeamId &&
                                             g.DeadLine > g.ClosedDate && g.StatusOfGoalId != (int) StatusGoalEnum.Active))
                                    .Count;


                            var teamDeadlines = String.Format(teamDeadlineTemplate, teamAchievedDeadlines,
                                teamMissedDeadlines);


                            var teamEnTime =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;
                            var teamEnRes =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;
                            var teamEnSupp =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;
                            var teamEnRightSkills =
                                (await
                                    _repoGoal.FindByAsync(
                                        g =>
                                            g.TeamId == team.TeamId &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills) &&
                                            (DbFunctions.DiffDays(DateTime.UtcNow, g.ClosedDate) <= dayCount)))
                                    .Count;

                            var teamFeedBacks = String.Format(teamFeedbacksTemplate, teamEnTime, teamEnRightSkills,
                                teamEnSupp, teamEnRes);

                            var messageText = MailHelper.PrepareUpdateManagerEmail(active.ToString(), goals, deadlines,
                                feedBacks, user.FirstName, team.TeamName, teamInfo, teamGoals, teamFeedBacks,
                                teamDeadlines, updateType);
                            var message = new SendGridMessage();
                            message.AddTo(user.Email);
                            message.Subject = "Hiper";

                            message.Html = messageText;

                            await emailService.SendAsync(message);
                        }
                    }
                }
            }
        }
    }
}
