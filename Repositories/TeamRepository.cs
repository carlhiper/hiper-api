using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Hiper.Api.Models;

namespace Hiper.Api.Repositories
{
    public class TeamRepository:IRepository<TeamModel>
    {
        private AppContext _ctx;
        private readonly DbSet<TeamModel> _teams;

        public TeamRepository(AppContext context)
        {
            _ctx = context;
            _teams = _ctx.Teams;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IQueryable<TeamModel> GetAll()
        {
            return _teams.Select(t => t);
        }

        public TeamModel GetSingle(int barId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TeamModel> FindBy(Expression<Func<TeamModel, bool>> predicate)
        {
            var found = _teams.Where(predicate);
            return found;
        }

        public Task<List<TeamModel>> FindByAsync(Expression<Func<TeamModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        public async Task<List<TeamModel>> GetAllAsync()
        {

            return await Task.FromResult(_teams.ToList());
        }
        public TeamModel Add(TeamModel entity)
        {
            //throw new NotImplementedException();
            var found = _teams.Find(entity.TeamId);
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

        public void Delete(TeamModel entity)
        {
            throw new NotImplementedException();
        }

        public TeamModel Edit(TeamModel entity)
        {
            var found = _teams.Find(entity.TeamId);
            if (found != null)
            {
                var entry = _ctx.Entry(found);
                entry.OriginalValues.SetValues(found);
                entry.CurrentValues.SetValues(entity);
            }
            else _ctx.Entry(entity).State = EntityState.Added;
            Save();
            return entity;
        }

        public List<TeamTypesModel> GetAllTeamTypes()
        {
            return _ctx.TeamTypes.ToList();
        }

        public void Save()
        {
            _ctx.SaveChanges();
      
        }
    }
}