using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class CommentRepositoryAsync : GenericRepositoryAsync<Comments>, ICommentRepositoryAsync
    {
        private readonly DbSet<Comments> _comments;

        public CommentRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _comments = dbContext.Set<Comments>();
        }

        public Task<Comments> GetCommentById(string id)
        {
            Guid IdGuid = Guid.Parse(id);
            return _comments.FirstOrDefaultAsync(c => c.Id == IdGuid);
        }

        public IQueryable<Comments> GetFoodComments(string foodId)
        {
            Guid foodIdGuid = Guid.Parse(foodId);
            return _comments.Where(x => x.Id == foodIdGuid);
        }

        public IQueryable<Comments> GetUserComments(string userId)
        {
            // return _comments.Where(x => x.CreatedBy  == userId);
            return _comments.Where(x => x.IsDeleted == false);
        }

        public IQueryable<Comments> GetUserFoodComments(string userId, string foodId)
        {
            Guid foodIdGuid = Guid.Parse(foodId);
            return _comments.Where(x => x.IsDeleted == false);
        }
    }
}
