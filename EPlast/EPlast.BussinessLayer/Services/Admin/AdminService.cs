﻿using AutoMapper;
using EPlast.BussinessLayer.DTO;
using EPlast.BussinessLayer.DTO.UserProfiles;
using EPlast.BussinessLayer.ExtensionMethods;
using EPlast.BussinessLayer.Services.Interfaces;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPlast.BussinessLayer.Services
{
    public class AdminService : IAdminService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        public AdminService(IRepositoryWrapper repoWrapper, UserManager<User> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _repoWrapper = repoWrapper;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }
        public async Task<IEnumerable<IdentityRole>> GetRolesExceptAdminAsync()
        {
            var admin = _roleManager.Roles.Where(i => i.Name == "Admin");
            var allRoles = await _roleManager.Roles.
                Except(admin).
                OrderBy(i => i.Name).
                ToListAsync();
            return allRoles;
        }

        public async Task EditAsync(string userId, List<string> roles)
        {
            User user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            var addedRoles = roles.Except(userRoles);
            var removedRoles = userRoles.
                Except(roles).
                Except(new List<string> { "Admin" });
            await _userManager.AddToRolesAsync(user, addedRoles);
            await _userManager.RemoveFromRolesAsync(user, removedRoles);
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count == 0)
            {
                await _userManager.AddToRoleAsync(user, "Прихильник");
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            User user = await _repoWrapper.User.GetFirstOrDefaultAsync(x => x.Id == userId);
            var roles = await _userManager.GetRolesAsync(user);
            if (user != null && !roles.Contains("Admin"))
            {
                _repoWrapper.User.Delete(user);
                await _repoWrapper.SaveAsync();
            }
        }

        public async Task<IEnumerable<UserTableDTO>> UsersTableAsync()
        {
            var users = await _repoWrapper.User.GetAllAsync(null,
                i => i.Include(x => x.UserProfile).
                        ThenInclude(x => x.Gender).
                    Include(x => x.UserPlastDegrees));

            var cities = await _repoWrapper.City.
                GetAllAsync(null, x => x.Include(i => i.Region));
            var clubMembers = await _repoWrapper.ClubMembers.
                GetAllAsync(null, x => x.Include(i => i.Club));
            var cityMembers = await _repoWrapper.CityMembers.
                GetAllAsync(null, x => x.Include(i => i.City));
            List<UserTableDTO> userTable = new List<UserTableDTO>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var cityName = cityMembers.Where(x => x.User.Id.Equals(user.Id) && x.EndDate == null)
                                          .Select(x => x.City.Name)
                                          .LastOrDefault() ?? string.Empty;

                userTable.Add(new UserTableDTO
                {
                    User = _mapper.Map<User, UserDTO>(user),
                    ClubName = clubMembers.Where(x => x.UserId.Equals(user.Id) && x.IsApproved)
                                          .Select(x => x.Club.ClubName).LastOrDefault() ?? string.Empty,
                    CityName = cityName,
                    RegionName = !string.IsNullOrEmpty(cityName) ? cities
                        .FirstOrDefault(x => x.Name.Equals(cityName))
                        ?.Region.RegionName : string.Empty,

                    UserPlastDegreeName = user.UserPlastDegrees.Count != 0 ? user.UserPlastDegrees
                        .FirstOrDefault(x => x.UserId == user.Id && x.DateFinish == null)
                        ?.UserPlastDegreeType.GetDescription() : string.Empty,
                    UserRoles = string.Join(", ", roles)
                });
            }
            return userTable;
        }

    }
}
