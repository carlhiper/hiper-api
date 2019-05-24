using AutoMapper;
using Hiper.Api.Models;

namespace Hiper.Api
{
    public static class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            Mapper.CreateMap<UserModel, ProfileModel>();
            Mapper.CreateMap<ProfileModel, UserModel>();
            Mapper.CreateMap<TeamModel, TeamCreationViewModel>();
            Mapper.CreateMap<TeamCreationViewModel, TeamModel>();
            Mapper.CreateMap<GoalModel, GoalViewModel>();
            Mapper.CreateMap<GoalModel, GoalModel>().ForMember(x => x.GoalId, y => y.Ignore());
            
            Mapper.CreateMap<GoalViewModel, GoalModel>();
            Mapper.CreateMap<GoalModel, GoalCreateViewModel>();
            Mapper.CreateMap<GoalCreateViewModel, GoalModel>();
        }
    }
}