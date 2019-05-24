using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hiper.Api.Models;

namespace Hiper.Api.Repositories
{
    public class GoalRepository : IRepository<GoalModel>
    {
        private readonly AppContext _ctx;
        private readonly DbSet<GoalModel> _goals;

        public GoalRepository(AppContext context)
        {
            _ctx = context;
            _goals = _ctx.Goals;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IQueryable<GoalModel> GetAll()
        {
            throw new NotImplementedException();
        }

        public GoalModel GetSingle(int barId)
        {
            var found = _goals.FirstOrDefault(g => g.GoalId == barId);
            return found;
        }

        public IQueryable<GoalModel> FindBy(Expression<Func<GoalModel, bool>> predicate)
        {
            var found = _goals.Where(predicate);
            return found;
        }

        public async Task<List<GoalModel>> FindByAsync(Expression<Func<GoalModel, bool>> predicate)
        {
            return await Task.FromResult(_goals.Where(predicate).ToList());
        }

        public GoalModel Add(GoalModel entity)
        {
            var found = _goals.Find(entity.GoalId);
            if (found != null)
            {
                var entry = _ctx.Entry(found);
                entry.OriginalValues.SetValues(found);
                entry.CurrentValues.SetValues(entity);
                _ctx.Entry(found).State = EntityState.Added;
            }
            else _ctx.Entry(entity).State = EntityState.Added;
            Save();
            return entity;
        }

        public void Delete(GoalModel entity)
        {
            throw new NotImplementedException();
        }


        public GoalModel Edit(GoalModel entity)
        {
            var found = _goals.Find(entity.GoalId);
            if (found != null)
            {
                var entry = _ctx.Entry(found);
                entry.OriginalValues.SetValues(found);
                entry.CurrentValues.SetValues(entity);
                Save();
              
            }
            return entity;
        }

        public List<GoalTypeModel> GetGoalTypes()
        {
            return _ctx.GoalType.ToList();
        }

        public List<StatusGoalModel> GetGoalStatuses()
        {
            return _ctx.StatusGoal.ToList();
        }

        public List<RepeatModel> GetRepeates()
        {
            return _ctx.Repeat.ToList();
        }

        public List<SurveyModel> GetSurveys()
        {
            return _ctx.Surveys.ToList();
        }

        public void Save()
        {
            _ctx.SaveChanges();
        }
    }
}