using System;

namespace Hiper.Api.Models
{
    public class DeleteTeamsForUserViewModel
    {
        public string UserName { get; set; }
        public Int32[] TeamIds { get; set; }
    }
}