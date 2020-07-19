﻿using EPlast.BLL.DTO;
using EPlast.BLL.DTO.UserProfiles;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPlast.BLL.Interfaces.UserProfiles
{
    public interface IUserService
    {
        Task<UserDTO> GetUserAsync(string userId);
        Task UpdateAsync(UserDTO user, IFormFile file, int? placeOfStudyId, int? specialityId, int? placeOfWorkId, int? positionId);
        Task UpdateAsyncForBase64(UserDTO user,string base64, int? placeOfStudyId, int? specialityId, int? placeOfWorkId, int? positionId);
        Task<TimeSpan> CheckOrAddPlastunRoleAsync(string userId, DateTime registeredOn);
        IEnumerable<ConfirmedUserDTO> GetConfirmedUsers(UserDTO user);
        ConfirmedUserDTO GetClubAdminConfirmedUser(UserDTO user);
        ConfirmedUserDTO GetCityAdminConfirmedUser(UserDTO user);
        Task<bool> CanApproveAsync(IEnumerable<ConfirmedUserDTO> confUsers, string userId, ClaimsPrincipal user);
        Task<string> GetImageBase64Async(string fileName);
    }
}
