﻿using EPlast.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EPlast.DataAccess.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(EPlastDBContext dbContext)
            : base(dbContext)
        {
        }

        public new void Update(User item)
        {
            var user = EPlastDBContext.Users.Find(item.Id);
            user.FirstName = item.FirstName;
            user.LastName = item.LastName;
            user.FatherName = item.FatherName;
            user.ImagePath = item.ImagePath;
            user.PhoneNumber = item.PhoneNumber;
            EPlastDBContext.Users.Update(user);
        }
        public async Task<int> GetTotalUsersCountAsync()
        {
            var usersCount = await EPlastDBContext.Users.Where(x => x.EmailConfirmed).CountAsync();

           return usersCount;
        }
    }
}