using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Hiper.Api.Helpers
{
    public static class MailHelper
    {
        private const string Hiper = "hiper";

        public static string GetCustomUrl(string url)
        {
            var result = url.Substring(url.IndexOf(":", StringComparison.Ordinal));
            return Hiper + result;
        }

        public static String PrepareAutoReplyEmail(string mail)
        {
            var autoreply = UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailAutoReply"]);
            autoreply = autoreply.Replace("%body%", mail);
            return autoreply;
        }

        public static String PrepareUpdateMemberEmail(string active, string goals, string deadlines, string feedbacks, string firstName, string teamName, string period)
        {
            var mail = UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateMember"]);
            mail = mail.Replace("%ActiveGoals%", active);
            mail = mail.Replace("%Goals%", goals);
            mail = mail.Replace("%Deadlines%", deadlines);
            mail = mail.Replace("%Feedback%", feedbacks);
            mail = mail.Replace("%first_name%", firstName);
            mail = mail.Replace("%team_name%", teamName);
            mail = mail.Replace("%period%", period);
            return mail;
        }

        public static String PrepareUpdateManagerEmail(string active, string goals, string deadlines, string feedbacks, string firstName, string teamName, string info, string teamGoals, string teamFeedbacks, string teamDeadlines, string period)
        {
            var mail = UploadHelper.LoadEmailTemplate(ConfigurationManager.AppSettings["mailUpdateManager"]);
            mail = mail.Replace("%ActiveGoals%", active);
            mail = mail.Replace("%Goals%", goals);
            mail = mail.Replace("%Deadlines%", deadlines);
            mail = mail.Replace("%Feedback%", feedbacks);
            mail = mail.Replace("%TeamInfo%", info);
            mail = mail.Replace("%TeamGoals%", teamGoals);
            mail = mail.Replace("%TeamDeadlines%", teamDeadlines);
            mail = mail.Replace("%TeamFeedback%", teamFeedbacks);
            mail = mail.Replace("%first_name%", firstName);
            mail = mail.Replace("%team_name%", teamName);
            mail = mail.Replace("%period%", period);
            return mail;
        }

        public static bool CheckIsEmail(string email)
        {
            const string pattern = "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
            return Regex.IsMatch(email, pattern);
        }
    }
}