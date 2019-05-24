using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Hiper.Api.Models;

namespace Hiper.Api.Repositories
{
    public class HipeRepository:IRepository<HipeModel>
    {
        private AppContext _ctx;
        private DbSet<HipeModel> _hipes; 
        public HipeRepository(AppContext context)
        {
            _ctx = context;
            _hipes = _ctx.Hipes;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IQueryable<HipeModel> GetAll()
        {
            throw new NotImplementedException();
        }

        public HipeModel GetSingle(int barId)
        {
            var found = _hipes.FirstOrDefault(g => g.HipeId == barId);
            return found;
        }

        public IQueryable<HipeModel> FindBy(Expression<Func<HipeModel, bool>> predicate)
        {
            var found = _hipes.Where(predicate);
            return found;
        }

        public async Task<List<HipeModel>> FindByAsync(Expression<Func<HipeModel, bool>> predicate)
        {
            return await Task.FromResult(_hipes.Where(predicate).ToList());
        }

        public HipeModel Add(HipeModel entity)
        {
            var hiped =
                _hipes.Where(h => (h.GoalId == entity.GoalId || h.FeedId == entity.GoalId) && h.UserId == entity.UserId)
                    .ToList()
                    .Any();
            var found = _hipes.Find(entity.HipeId);
            if (!hiped&&found==null)
            {
                _ctx.Entry(entity).State = EntityState.Added;
            }
            Save();
            return entity;
        }

        public void Delete(HipeModel entity)
        {
            throw new NotImplementedException();
        }

        public HipeModel Edit(HipeModel entity)
        {
            var found = _hipes.Find(entity.GoalId);
            if (found != null)
            {
                var entry = _ctx.Entry(found);
                entry.OriginalValues.SetValues(found);
                entry.CurrentValues.SetValues(entity);
                Save();
             
            }
            return entity;
        }

        public void Save()
        {
            _ctx.SaveChanges();
        }
    }
}