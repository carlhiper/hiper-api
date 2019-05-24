using System;
using System.Collections;
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
    [RoutePrefix("api/scoreboard")]
    public class ScoreboardController : ApiController
    {
        private readonly UserRepository _repoUser;
     
        private readonly GoalRepository _repoGoal;
        private readonly TeamRepository _repoTeam;
        private readonly HipeRepository _repoHipe;

        public ScoreboardController()
        {
            var context = new AppContext();
          
            _repoUser = new UserRepository(context);
            _repoGoal = new GoalRepository(context);
            _repoTeam = new TeamRepository(context);
            _repoHipe = new HipeRepository(context);
        }

        [Route("Scoreboard")]
        public IHttpActionResult Scoreboard(ScoreboardViewModel model)
        {
       
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
         
            var goals =
                _repoGoal.FindBy(
                    g =>
                        g.Participants.Any(u => u.UserName == model.Username) &&
                        g.StatusOfGoalId == (int) StatusGoalEnum.Active &&
                        (model.TeamId == -1 && !g.IsTeamGoal && g.Team == null || g.TeamId == model.TeamId)).ToList();
            var result = goals.Select(r => new
            {
                r.GoalId,
                r.Description,
                r.Title,
                TargetAmount = r.GoalTypeId == (int) GoalTypeEnum.SucceedFail ? "" : r.TargetAmount.ToString(),
                ReachedAmount = r.GoalTypeId == (int) GoalTypeEnum.SucceedFail ? "" : r.ReachedAmount.ToString(),
                Hipes = _repoHipe.FindBy(h => h.GoalId == r.GoalId).ToList().Count(),
                IsHiped =
                    _repoHipe.FindBy(h => h.UserId == currentUser.Id && h.GoalId == r.GoalId).ToList().Any() ||
                    (!r.IsTeamGoal && r.Participants.Any(p => p.Id == currentUser.Id)),
                Deadline = (int) ((new DateTime(r.DeadLine.Year, r.DeadLine.Month, r.DeadLine.Day, 0, 0, 0) - DateTime.Today)).TotalDays,
                r.IsTeamGoal
            }).OrderBy(r => r.Deadline).ToList();

            return Ok(result);
        }

        [Route("HistoryScoreboard")]
        public IHttpActionResult HistoryScoreboard(FilterViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
            var isAdmin = team != null && team.AdministratorId == currentUser.Id;

            var result =
                _repoGoal.FindBy(
                    g =>
                        ((isAdmin && model.TeamMember == "-1")
                            ? g.TeamId == model.TeamId
                            : (!isAdmin && model.TeamMember == "-1")
                                ? g.Participants.Any(u => u.UserName == currentUser.UserName)
                                : (isAdmin && model.TeamMember != "-1") &&
                                  g.Participants.Any(u => u.UserName == model.TeamMember))
                        && (model.TypeOfGoal == -1 || g.GoalTypeId == model.TypeOfGoal)
                        && (model.Status == -1 || g.StatusOfGoalId == model.Status)
                        && (model.Feedback == -1 || g.Surveys.Any(s => s.SurveyId == model.Feedback))
                        && DbFunctions.TruncateTime(g.ClosedDate) >= DbFunctions.TruncateTime(model.TimeFrom)
                        && DbFunctions.TruncateTime(g.ClosedDate) <= DbFunctions.TruncateTime(model.TimeTo)
                        &&
                        (g.StatusOfGoalId == (int) StatusGoalEnum.Cancel ||
                         g.StatusOfGoalId == (int) StatusGoalEnum.DidntAchievied ||
                         g.StatusOfGoalId == (int) StatusGoalEnum.Achieved) &&
                        (g.Team == null && model.TeamId == -1 || model.TeamId == g.TeamId)).ToList().Select(r => new
                        {
                            r.GoalId,
                            r.Description,
                            r.Title,
                            r.Hipes,
                            Deadline = (int) (r.DeadLine - DateTime.UtcNow).TotalDays,
                            r.IsTeamGoal,
                            ClosedDate = ((DateTime) r.ClosedDate).ToString("yy-MM-dd HH:mm"),
                            r.StatusOfGoal.StatusGoalDescription
                        }).OrderByDescending(r => r.ClosedDate);


            return Ok(result);
        }

        [Route("StatisticsScoreboard")]
        public IHttpActionResult StatisticsScoreboard(FilterViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
            var isAdmin = team != null && team.AdministratorId == currentUser.Id;
            var goalList =
                _repoGoal.FindBy(
                    g =>
                        ((isAdmin && model.TeamMember == "-1")
                            ? g.TeamId == model.TeamId
                            : (!isAdmin && model.TeamMember == "-1")
                                ? g.Participants.Any(u => u.UserName == currentUser.UserName)
                                : (isAdmin && model.TeamMember != "-1") &&
                                  g.Participants.Any(u => u.UserName == model.TeamMember))
                        &&
                        DbFunctions.TruncateTime(g.ClosedDate) >=
                        DbFunctions.TruncateTime(model.TimeFrom)
                        &&
                        DbFunctions.TruncateTime(g.ClosedDate) <=
                        DbFunctions.TruncateTime(model.TimeTo)
                        && (g.Team == null && model.TeamId == -1 || model.TeamId == g.TeamId) &&
                        ((int) g.StatusOfGoalId != (int) StatusGoalEnum.Active)).ToList();

            var achivedGoals =
                goalList.Count(
                    g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved));
            var notAchivedGoals =
                goalList.Count(
                    g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.DidntAchievied));
            var canceledGoals =
                goalList.Count(g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel));

            var hadEnTime = !goalList.Any()
                ? 0
                : ((double)
                    goalList.Count(
                        g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/
                   goalList.Count())*100;
            var hadRightSkills = !goalList.Any()
                ? 0
                : ((double)
                    goalList.Count(
                        g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                   goalList.Count())*100;
            var hadEnSupport = !goalList.Any()
                ? 0
                : ((double)
                    goalList.Count(
                        g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/
                   goalList.Count())*100;
            var hadEnResource = !goalList.Any()
                ? 0
                : ((double)
                    goalList.Count(
                        g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                   goalList.Count())*100;
            var ratio = !goalList.Any() ? 0 : ((double) achivedGoals/goalList.Count())*100;
            var result = new
            {
                achivedGoals,
                notAchivedGoals,
                canceledGoals,
                ratio = Math.Round(ratio, 2),
                hadEnTime = Math.Round(hadEnTime, 2),
                hadEnResource = Math.Round(hadEnResource, 2),
                hadEnSupport = Math.Round(hadEnSupport, 2),
                hadRightSkills = Math.Round(hadRightSkills, 2)
            };
            return Ok(result);
        }

        [Route("AdvancedStatisticsGraphScoreboard")]
        public IHttpActionResult AdvancedStatisticsGraphScoreboard(AdvancedStatisitcsViewModel model)
        {
        
            var dayCount = 0;

            switch (model.TimePeriod)
            {
                case "1m":
                    dayCount = (int) (DateTime.UtcNow - DateTime.UtcNow.AddMonths(-1)).TotalDays;
                    break;
                case "3m":
                    dayCount = (int) (DateTime.UtcNow - DateTime.UtcNow.AddMonths(-3)).TotalDays;
                    break;
                case "6m":
                    dayCount = (int) (DateTime.UtcNow - DateTime.UtcNow.AddMonths(-6)).TotalDays;
                    break;
                case "1y":
                    dayCount = (int) (DateTime.UtcNow - DateTime.UtcNow.AddYears(-1)).TotalDays;
                    break;
                case "3y":
                    dayCount = (int) (DateTime.UtcNow - DateTime.UtcNow.AddYears(-1)).TotalDays;
                    break;
                case "10y":
                    dayCount = (int) (DateTime.UtcNow - DateTime.UtcNow.AddYears(-1)).TotalDays;
                    break;
            }


            var dateByTimeperiod = DateTime.UtcNow.AddDays(-dayCount);
            var goalList =
                _repoGoal.FindBy(
                    g =>
                        ((model.TeamMember == "-1")
                            ? g.TeamId == model.TeamId
                            : (model.TeamMember != "-1") &&
                              g.Participants.Any(u => u.UserName == model.TeamMember))

//                      
                        &&
                        DbFunctions.TruncateTime(g.ClosedDate) >=
                        DbFunctions.TruncateTime(dateByTimeperiod)
                        && (g.Team == null && model.TeamId == -1 || model.TeamId == g.TeamId) &&
                        ((int) g.StatusOfGoalId != (int) StatusGoalEnum.Active))
                    .OrderByDescending(g => g.ClosedDate)
                    .ToList();

            var totalGoals = goalList.Count();
            var achivedGoals = new double[dayCount];
            var canceledGoals = new double[dayCount];


            var hadEnTime = new double[dayCount];
            var hadRightSkills = new double[dayCount];
            var hadEnSupport = new double[dayCount];
            var hadEnResource = new double[dayCount];
            var missedDeadlines = new double[dayCount];
            var targetReached = new double[dayCount];
            var dates = new DateTime[dayCount];

            for (var i = 0; i < dayCount; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i);
                dates[i] = date;
                var currentDayGoals =
                    goalList.Where(
                        g => (int) ((DateTime) g.ClosedDate - date).TotalDays == 0)
                        .ToList();
                hadEnTime[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/
                       totalGoals)*100;
                hadRightSkills[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                       totalGoals)*100;
                hadEnSupport[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/
                       totalGoals)*100;
                hadEnResource[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g => g.Surveys != null && g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                       totalGoals)*100;
                achivedGoals[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))/
                       totalGoals)*100;
                canceledGoals[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))/
                       totalGoals)*100;

                missedDeadlines[i] = totalGoals == 0
                    ? 0
                    : ((double) currentDayGoals.Count(g => g.StatusOfGoalId != null && g.DeadLine > g.ClosedDate)/
                       totalGoals)*100;
                targetReached[i] = totalGoals == 0
                    ? 0
                    : ((double)
                        currentDayGoals.Count(
                            g =>
                                g.StatusOfGoalId != null && g.GoalTypeId == (int) GoalTypeEnum.Number &&
                                (g.ReachedAmount == g.TargetAmount))/totalGoals)*100;
            }


            var result = new
            {
                achivedGoals,
                dates,
                canceledGoals,
                missedDeadlines,
                targetReached,
                hadEnTime,
                hadEnResource,
                hadEnSupport,
                hadRightSkills
            };
            return Ok(result);
        }

        [Route("AdvancedStatisticsTableScoreboard")]
        public IHttpActionResult AdvancedStatisticsTableScoreboard(AdvancedStatisitcsViewModel model)
        {
            var identity = (ClaimsIdentity) User.Identity;
            var currentUser = _repoUser.FindUserByUserName(identity.Claims.First().Value);
            var team = _repoTeam.FindBy(t => t.TeamId == model.TeamId).FirstOrDefault();
           

            var goalList =
                _repoGoal.FindBy(
                    g =>
                        ((model.TeamMember == "-1")
                            ? g.TeamId == model.TeamId
                            : (model.TeamMember != "-1") &&
                              g.Participants.Any(u => u.UserName == model.TeamMember))

                     
                        && (g.Team == null && model.TeamId == -1 || model.TeamId == g.TeamId) && g.ClosedDate != null &&
                        ((int) g.StatusOfGoalId != (int) StatusGoalEnum.Active)).OrderBy(r => r.ClosedDate).ToList();

            var dayCount = 0;
            if (goalList.Count > 0)
            {
                var lastClosed = (DateTime) goalList.FirstOrDefault().ClosedDate;
                switch (model.TimePeriod)
                {
                    case "1m":
                        dayCount = DateHelper.GetMonthDifference(DateTime.UtcNow, lastClosed) + 1;
                        break;
                    case "1y":
                        dayCount = DateTime.UtcNow.Year - lastClosed.Year + 1;
                        break;
                    case "1q":
                        dayCount = DateTime.UtcNow.Year - lastClosed.Year + 1;
                        break;
                    case "1q/1y":
                        dayCount = DateTime.UtcNow.Year - lastClosed.Year + 1;
                        break;
                    case "1m/1y":
                        dayCount = DateTime.UtcNow.Year - lastClosed.Year + 1;
                        break;
                }
            }


        

            var achivedGoals = new ArrayList();
          
            var canceledGoals = new ArrayList();


            var hadEnTime = new ArrayList();
            var hadRightSkills = new ArrayList();
            var hadEnSupport = new ArrayList();
            var hadEnResource = new ArrayList();
            var missedDeadlines = new ArrayList();
            var targetReached = new ArrayList();
            var dates = new ArrayList();

            for (var i = 0; i < dayCount; i++)
            {
                var date = DateTime.UtcNow;
                List<GoalModel> currentDayGoals;
                var totalGoals = 0;
                switch (model.TimePeriod)
                {
                    case "1m":
                    {
                        date = date.AddMonths(-i);
                        currentDayGoals = goalList.Where(
                            g => DateHelper.GetMonthDifference((DateTime) g.ClosedDate, date) == 0)
                            .ToList();
                        totalGoals = currentDayGoals.Count();


                        dates.Add(date);

                        hadEnTime.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadRightSkills.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        hadEnSupport.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadEnResource.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        achivedGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100),
                                2));
                        canceledGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        missedDeadlines.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(g => g.StatusOfGoalId != null && g.DeadLine > g.ClosedDate)/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        targetReached.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null && g.GoalTypeId == (int) GoalTypeEnum.Number &&
                                            (g.ReachedAmount == g.TargetAmount))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));

                        break;
                    }
                    case "1y":
                    {
                        date = date.AddYears(-i);
                        currentDayGoals = goalList.Where(
                            g => ((DateTime) g.ClosedDate).Year - date.Year == 0)
                            .ToList();
                        totalGoals = currentDayGoals.Count();


                        dates.Add(date);

                        hadEnTime.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadRightSkills.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        hadEnSupport.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadEnResource.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        achivedGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100),
                                2));
                        canceledGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        missedDeadlines.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(g => g.StatusOfGoalId != null && g.DeadLine > g.ClosedDate)/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        targetReached.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null && g.GoalTypeId == (int) GoalTypeEnum.Number &&
                                            (g.ReachedAmount == g.TargetAmount))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        break;
                    }
                    case "1q":
                    {
                        var currentQuarter = 1;
                        if (date.Year != (date.Year - i))
                        {
                            currentQuarter = 4;
                        }
                        else
                        {
                            currentQuarter = DateHelper.GetQuarter(date.AddYears(-i));
                        }

                        for (var y = currentQuarter; y > 0; y--)
                        {
                            var startDate = DateHelper.FirstDateOfQuarter(y, date.Year - i);
                            var endDate = startDate.AddMonths(3).AddDays(-1);
                            currentDayGoals = goalList.Where(
                                g => ((DateTime) g.ClosedDate >= startDate && (DateTime) g.ClosedDate <= endDate))
                                .ToList();
                            totalGoals = currentDayGoals.Count();


                            dates.Add(startDate);

                            hadEnTime.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.Surveys != null &&
                                                g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/(model.IsAbs ? 1 : totalGoals))*
                                    (model.IsAbs ? 1 : 100), 2));
                            hadRightSkills.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.Surveys != null &&
                                                g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                                     (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                            hadEnSupport.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.Surveys != null &&
                                                g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/(model.IsAbs ? 1 : totalGoals))*
                                    (model.IsAbs ? 1 : 100), 2));
                            hadEnResource.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.Surveys != null &&
                                                g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                                     (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                            achivedGoals.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.StatusOfGoalId != null &&
                                                ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100),
                                    2));
                            canceledGoals.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.StatusOfGoalId != null &&
                                                ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                            missedDeadlines.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(g => g.StatusOfGoalId != null && g.DeadLine > g.ClosedDate)/
                                     (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                            targetReached.Add(totalGoals == 0
                                ? 0
                                : Math.Round(
                                    ((double)
                                        currentDayGoals.Count(
                                            g =>
                                                g.StatusOfGoalId != null && g.GoalTypeId == (int) GoalTypeEnum.Number &&
                                                (g.ReachedAmount == g.TargetAmount))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        }
                    }

                        break;
                    case "1q/1y":
                    {
                        var startDate = DateHelper.FirstDateOfQuarter(4 - Int32.Parse(model.PeriodOffset), date.Year - i);
                        var endDate = startDate.AddMonths(3).AddDays(-1);
                        currentDayGoals = goalList.Where(
                            g => ((DateTime) g.ClosedDate >= startDate && (DateTime) g.ClosedDate <= endDate))
                            .ToList();
                        totalGoals = currentDayGoals.Count();


                        dates.Add(startDate);

                        hadEnTime.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadRightSkills.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        hadEnSupport.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadEnResource.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        achivedGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100),
                                2));
                        canceledGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        missedDeadlines.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(g => g.StatusOfGoalId != null && g.DeadLine > g.ClosedDate)/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        targetReached.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null && g.GoalTypeId == (int) GoalTypeEnum.Number &&
                                            (g.ReachedAmount == g.TargetAmount))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));


                        break;
                    }
                    case "1m/1y":
                    {
                        var startDate = new DateTime(date.Year - i, 12 - Int32.Parse(model.PeriodOffset), 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);
                        currentDayGoals = goalList.Where(
                            g => ((DateTime) g.ClosedDate >= startDate && (DateTime) g.ClosedDate <= endDate))
                            .ToList();
                        totalGoals = currentDayGoals.Count();


                        dates.Add(startDate);

                        hadEnTime.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnTime))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadRightSkills.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadRightSkills))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        hadEnSupport.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnSupport))/(model.IsAbs ? 1 : totalGoals))*
                                (model.IsAbs ? 1 : 100), 2));
                        hadEnResource.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.Surveys != null &&
                                            g.Surveys.Any(s => s.SurveyId == (int) SurveyEnum.IhadEnResource))/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        achivedGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100),
                                2));
                        canceledGoals.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null &&
                                            ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        missedDeadlines.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(g => g.StatusOfGoalId != null && g.DeadLine > g.ClosedDate)/
                                 (model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));
                        targetReached.Add(totalGoals == 0
                            ? 0
                            : Math.Round(
                                ((double)
                                    currentDayGoals.Count(
                                        g =>
                                            g.StatusOfGoalId != null && g.GoalTypeId == (int) GoalTypeEnum.Number &&
                                            (g.ReachedAmount == g.TargetAmount))/(model.IsAbs ? 1 : totalGoals))*(model.IsAbs ? 1 : 100), 2));


                        break;
                    }
            
                }
            }


            var result = new
            {
                achivedGoals,
                dates,
                canceledGoals,
                missedDeadlines,
                targetReached,
                hadEnTime,
                hadEnResource,
                hadEnSupport,
                hadRightSkills
            };
            return Ok(result);
        }


        [Route("GetTotalAmountOfAchievedGoals")]
        public IHttpActionResult GetTotalAmountOfAchievedGoals()
        {
            return
                Ok(
                    _repoGoal.FindBy(
                        g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Achieved))
                        .Count());
        }

        [Route("GetTotalAmountOfNotAchievedGoals")]
        public IHttpActionResult GetTotalAmountOfNotAchievedGoals()
        {
            return
                Ok(
                    _repoGoal.FindBy(
                        g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.DidntAchievied))
                        .Count());
        }

        [Route("GetTotalAmountOfCancelledGoals")]
        public IHttpActionResult GetTotalAmountOfCancelledGoals()
        {
            return
                Ok(
                    _repoGoal.FindBy(
                        g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Cancel))
                        .Count());
        }

        [Route("GetTotalAmountOfActiveGoals")]
        public IHttpActionResult GetTotalAmountOfActiveGoals()
        {
            return
                Ok(
                    _repoGoal.FindBy(
                        g => g.StatusOfGoalId != null && ((int) g.StatusOfGoalId == (int) StatusGoalEnum.Active))
                        .Count());
        }
    }
}
