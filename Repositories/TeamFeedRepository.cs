using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Hiper.Api.Models;

namespace Hiper.Api.Repositories
{
    public class TeamFeedRepository:IRepository<TeamFeedModel>
    {
        private AppContext _ctx;
        private readonly DbSet<TeamFeedModel> _feeds;

        public TeamFeedRepository(AppContext ctx)
        {
            _ctx = ctx;
            _feeds = _ctx.Feeds;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IQueryable<TeamFeedModel> GetAll()
        {
            return _feeds.Select(d=>d);
        }

        public TeamFeedModel GetSingle(int barId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TeamFeedModel> FindBy(Expression<Func<TeamFeedModel, bool>> predicate)
        {
            var found = _feeds.Where(predicate);
            return found;
        }

        public async Task<List<TeamFeedModel>> FindByAsync(Expression<Func<TeamFeedModel, bool>> predicate)
        {
            return await Task.FromResult(_feeds.Where(predicate).ToList());
        }

        public TeamFeedModel Add(TeamFeedModel entity)
        {
            //throw new NotImplementedException();
            var found = _feeds.Find(entity.TeamFeedId);
            if (found != null)
            {
                var entry = _ctx.Entry(found);
                entry.OriginalValues.SetValues(found);
                entry.CurrentValues.SetValues(entity);
                // ApplicationDbContext.Entry(terms).State = EntityState.Modified;
            }
            else _ctx.Entry(entity).State = EntityState.Added;
            Save();
            return entity;
        }

        public void Delete(TeamFeedModel entity)
        {
            throw new NotImplementedException();
        }


        public TeamFeedModel Edit(TeamFeedModel entity)
        {
            var found = _feeds.Find(entity.TeamFeedId);
            if (found != null)
            {
                var entry = _ctx.Entry(found);
                entry.OriginalValues.SetValues(found);
                entry.CurrentValues.SetValues(entity);
                // ApplicationDbContext.Entry(terms).State = EntityState.Modified;
            }
            else _ctx.Entry(entity).State = EntityState.Added;
            Save();
            return entity;
        }

        public List<UpdateTypeModel> GetUpdateTypes()
        {
            return _ctx.UpdateType.ToList();
        }

        public void Save()
        {
            try
            {
                _ctx.SaveChanges();
            }
            catch (Exception e)
            {
                var t = e;
            }

        }
    }
}