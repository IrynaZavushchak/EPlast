﻿using AutoMapper;
using EPlast.BLL.DTO;
using EPlast.BLL.DTO.UserProfiles;
using EPlast.BLL.Interfaces.Logging;
using EPlast.BLL.Interfaces.UserProfiles;
using EPlast.BLL.Services.Interfaces;
using EPlast.WebApi.Models.Approver;
using EPlast.WebApi.Models.User;
using EPlast.WebApi.Models.UserModels;
using EPlast.WebApi.Models.UserModels.UserProfileFields;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPlast.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace EPlast.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserManagerService _userManagerService;
        private readonly IConfirmedUsersService _confirmedUserService;
        private readonly IUserPersonalDataService _userPersonalDataService;
        private readonly ILoggerService<UserController> _loggerService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public UserController(IUserService userService,
            IUserPersonalDataService userPersonalDataService,
            IConfirmedUsersService confirmedUserService,
            IUserManagerService userManagerService,
            ILoggerService<UserController> loggerService,
            IMapper mapper, UserManager<User> userManager)
        {
            _userService = userService;
            _userPersonalDataService = userPersonalDataService;
            _confirmedUserService = confirmedUserService;
            _userManagerService = userManagerService;
            _loggerService = loggerService;
            _mapper = mapper;
            _userManager = userManager;
        }

        /// <summary>
        /// Get a specify user
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>A user</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="404">User not found</response>
        [HttpGet("{userId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Get(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _loggerService.LogError("User id is null");
                return NotFound();
            }

            var user = await _userService.GetUserAsync(userId);
            if (user != null)
            {
                var time = _userService.CheckOrAddPlastunRole(user.Id, user.RegistredOn);
                var isUserPlastun = await _userManagerService.IsInRoleAsync(user, "Пластун")
                    || user.UserProfile.UpuDegreeID != 1
                    || !(await _userManagerService.IsInRoleAsync(user, "Прихильник")
                    && await _userService.IsApprovedCityMember(userId));

                var model = new PersonalDataViewModel
                {
                    User = _mapper.Map<UserDTO, UserViewModel>(user),
                    TimeToJoinPlast = (int)time.TotalDays,
                    IsUserPlastun = isUserPlastun,
                };

                return Ok(model);
            }

            _loggerService.LogError($"User not found. UserId:{userId}");
            return NotFound();
        }

        /// <summary>
        /// Get a specify user profile
        /// </summary>
        /// <param name="focusUserId">The id of the focus user</param>
        /// <param name="currentUserId">The id of the current user</param>
        /// <returns>A focus user profile</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="404">Focus user not found</response>
        [HttpGet("{currentUserId}/{focusUserId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetUserProfile(string currentUserId, string focusUserId)
        {
            if (string.IsNullOrEmpty(focusUserId))
            {
                _loggerService.LogError("User id is null");
                return NotFound();
            }

            var currentUser = await _userService.GetUserAsync(currentUserId);
            var focusUser = await _userService.GetUserAsync(focusUserId);
            if (focusUser != null)
            {
                var time = _userService.CheckOrAddPlastunRole(focusUser.Id, focusUser.RegistredOn);
                var isThisUser = currentUserId == focusUserId;
                var isUserSameCity = currentUser.CityMembers.FirstOrDefault()?.CityId
                    .Equals(focusUser.CityMembers.FirstOrDefault()?.CityId) 
                    == true;
                var isUserSameClub = currentUser.ClubMembers.FirstOrDefault()?.ClubId
                    .Equals(focusUser.ClubMembers.FirstOrDefault()?.ClubId)
                    == true;
                var isUserSameRegion = currentUser.RegionAdministrations.FirstOrDefault()?.RegionId
                    .Equals(focusUser.RegionAdministrations.FirstOrDefault()?.RegionId)
                    == true;
                var isUserAdmin = await _userManagerService.IsInRoleAsync(currentUser, "Admin");
                var isUserHeadOfCity = await _userManagerService.IsInRoleAsync(currentUser, "Голова Станиці");
                var isUserHeadOfClub = await _userManagerService.IsInRoleAsync(currentUser, "Голова Куреня");
                var isUserHeadOfRegion = await _userManagerService.IsInRoleAsync(currentUser, "Голова Округу");
                var isCurrentUserSupporter = await _userManagerService.IsInRoleAsync(currentUser, "Прихильник");
                var isCurrentUserPlastun = await _userManagerService.IsInRoleAsync(currentUser, "Пластун")
                    || currentUser.UserProfile.UpuDegreeID != 1
                    || !(isCurrentUserSupporter
                    && await _userService.IsApprovedCityMember(currentUserId));
                var isFocusUserSupporter = await _userManagerService.IsInRoleAsync(focusUser, "Прихильник");
                var isFocusUserPlastun = await _userManagerService.IsInRoleAsync(focusUser, "Пластун")
                    || focusUser.UserProfile.UpuDegreeID != 1
                    || !(isFocusUserSupporter
                    && await _userService.IsApprovedCityMember(focusUserId));

                if (isThisUser || 
                    isUserAdmin ||
                    (isUserHeadOfCity && isUserSameCity) ||
                    (isUserHeadOfClub && isUserSameClub) ||
                    (isUserHeadOfRegion && isUserSameRegion) ||
                    (isCurrentUserPlastun && isUserSameCity))
                {
                    var model = new PersonalDataViewModel
                    {
                        User = _mapper.Map<UserDTO, UserViewModel>(focusUser),
                        TimeToJoinPlast = (int)time.TotalDays,
                        IsUserPlastun = isFocusUserPlastun,
                    };

                    return Ok(model);
                }
                else if (isCurrentUserSupporter || isUserHeadOfCity || isUserHeadOfClub || isUserHeadOfRegion || isCurrentUserPlastun )
                {
                    var model = new PersonalDataViewModel
                    {
                        ShortUser = _mapper.Map<UserDTO, UserShortViewModel>(focusUser),
                        TimeToJoinPlast = (int)time.TotalDays,
                        IsUserPlastun = isFocusUserPlastun,
                    };

                    return Ok(model);
                }
            }

            _loggerService.LogError($"User not found. UserId:{focusUserId}");
            return NotFound();
        }

        /// <summary>
        /// Get a image
        /// </summary>
        /// <param name="imageName">The name of the image</param>
        /// <returns>Image in format base64</returns>
        /// <response code="200">Successful operation</response>
        [HttpGet("getImage/{imageName}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            return Ok(await _userService.GetImageBase64Async(imageName));
        }

        /// <summary>
        /// Get specify model for edit user
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>A data of user for editing</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="404">User not found</response>
        [HttpGet("edit/{userId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Edit(string userId)
        {
            if (userId == null)
            {
                _loggerService.LogError("User id is null");

                return NotFound();
            }
            var user = await _userService.GetUserAsync(userId);
            if (user != null)
            {
                var genders = _mapper.Map<IEnumerable<GenderDTO>, IEnumerable<GenderViewModel>>(await _userPersonalDataService.GetAllGendersAsync());
                var placeOfStudyUnique = _mapper.Map<IEnumerable<EducationDTO>, IEnumerable<EducationViewModel>>(await _userPersonalDataService.GetAllEducationsGroupByPlaceAsync());
                var specialityUnique = _mapper.Map<IEnumerable<EducationDTO>, IEnumerable<EducationViewModel>>(await _userPersonalDataService.GetAllEducationsGroupBySpecialityAsync());
                var placeOfWorkUnique = _mapper.Map<IEnumerable<WorkDTO>, IEnumerable<WorkViewModel>>(await _userPersonalDataService.GetAllWorkGroupByPlaceAsync());
                var positionUnique = _mapper.Map<IEnumerable<WorkDTO>, IEnumerable<WorkViewModel>>(await _userPersonalDataService.GetAllWorkGroupByPositionAsync());
                var upuDegrees = _mapper.Map<IEnumerable<UpuDegreeDTO>, IEnumerable<UpuDegreeViewModel>>(await _userPersonalDataService.GetAllUpuDegreesAsync());

                var educView = new UserEducationViewModel { PlaceOfStudyID = user.UserProfile.EducationId, SpecialityID = user.UserProfile.EducationId, PlaceOfStudyList = placeOfStudyUnique, SpecialityList = specialityUnique };
                var workView = new UserWorkViewModel { PlaceOfWorkID = user.UserProfile.WorkId, PositionID = user.UserProfile.WorkId, PlaceOfWorkList = placeOfWorkUnique, PositionList = positionUnique };
                var model = new EditUserViewModel()
                {
                    User = _mapper.Map<UserDTO, UserViewModel>(user),
                    Nationalities = _mapper.Map<IEnumerable<NationalityDTO>, IEnumerable<NationalityViewModel>>(await _userPersonalDataService.GetAllNationalityAsync()),
                    Religions = _mapper.Map<IEnumerable<ReligionDTO>, IEnumerable<ReligionViewModel>>(await _userPersonalDataService.GetAllReligionsAsync()),
                    EducationView = educView,
                    WorkView = workView,
                    Degrees = _mapper.Map<IEnumerable<DegreeDTO>, IEnumerable<DegreeViewModel>>(await _userPersonalDataService.GetAllDegreesAsync()),
                    Genders = genders,
                    UpuDegrees = upuDegrees,
                };

                return Ok(model);
            }
            _loggerService.LogError($"User not found. UserId:{userId}");
            return NotFound();
        }

        /// <summary>
        /// Edit a user
        /// </summary>
        /// <param name="model">Edit model</param>
        /// <response code="200">Successful operation</response>
        [HttpPut("editbase64")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> EditBase64([FromBody] EditUserViewModel model)
        {
            await _userService.UpdateAsyncForBase64(_mapper.Map<UserViewModel, UserDTO>(model.User),
                model.ImageBase64, model.EducationView.PlaceOfStudyID, model.EducationView.SpecialityID,
                model.WorkView.PlaceOfWorkID, model.WorkView.PositionID);
            _loggerService.LogInformation($"User was edited profile and saved in the database");

            return Ok();
        }

        /// <summary>
        /// Get approvers of selected user
        /// </summary>
        /// <param name="userId">The id of the user which can be approved</param>
        /// <param name="approverId">The id of the user which can approve</param>
        /// <returns>A specify data of user approvers for approve user or just review</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="404">User not found</response>
        [HttpGet("approvers/{userId}/{approverId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Approvers(string userId, string approverId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _loggerService.LogError("User id is null");
                return NotFound();
            }

            UserDTO user;

            try
            {
                user = await _userService.GetUserAsync(userId);
            }
            catch (Exception)
            {
                _loggerService.LogError($"User not found. UserId:{userId}");
                return NotFound();
            }
            var confirmedUsers = _userService.GetConfirmedUsers(user);
            var canApprove = _userService.CanApprove(confirmedUsers, userId, await _userManager.GetUserAsync(User));
            var time = _userService.CheckOrAddPlastunRole(user.Id, user.RegistredOn);
            var clubApprover = _userService.GetClubAdminConfirmedUser(user);
            var cityApprover = _userService.GetCityAdminConfirmedUser(user);

            var model = new UserApproversViewModel
            {
                User = _mapper.Map<UserDTO, UserInfoViewModel>(user),
                canApprove = canApprove,
                TimeToJoinPlast = ((int)time.TotalDays),
                ConfirmedUsers = _mapper.Map<IEnumerable<ConfirmedUserDTO>, IEnumerable<ConfirmedUserViewModel>>(confirmedUsers),
                ClubApprover = _mapper.Map<ConfirmedUserDTO, ConfirmedUserViewModel>(clubApprover),
                CityApprover = _mapper.Map<ConfirmedUserDTO, ConfirmedUserViewModel>(cityApprover),
                IsUserHeadOfCity = await _userManagerService.IsInRoleAsync(_mapper.Map<User,UserDTO>(await _userManager.GetUserAsync(User)), "Голова Станиці"),
                IsUserHeadOfClub = await _userManagerService.IsInRoleAsync(_mapper.Map<User, UserDTO>(await _userManager.GetUserAsync(User)), "Голова Куреня"),
                IsUserHeadOfRegion = await _userManagerService.IsInRoleAsync(_mapper.Map<User, UserDTO>(await _userManager.GetUserAsync(User)), "Голова Округу"),
                IsUserPlastun = await _userManagerService.IsInRoleAsync(user, "Пластун")
                    || user.UserProfile.UpuDegreeID != 1
                    || !(await _userManagerService.IsInRoleAsync(user, "Прихильник")
                    && await _userService.IsApprovedCityMember(userId)),
                CurrentUserId = approverId
            };
            foreach (var item in model.ConfirmedUsers)
            {
                item.Approver.User.ImagePath = await _userService.GetImageBase64Async(item.Approver.User.ImagePath);
            }
            if (model.ClubApprover != null)
            {
                model.ClubApprover.Approver.User.ImagePath = await _userService.GetImageBase64Async(model?.ClubApprover?.Approver?.User.ImagePath);
            }
            if (model.CityApprover != null)
            {
                model.CityApprover.Approver.User.ImagePath = await _userService.GetImageBase64Async(model?.CityApprover?.Approver?.User.ImagePath);
            }
            return Ok(model);
            
        }
        /// <summary>
        /// Approving user
        /// </summary>
        /// <param name="userId">The user ID which is confirmed</param>
        /// <param name="isClubAdmin">Confirm as an club admin</param>
        /// <param name="isCityAdmin">Confirm as an city admin</param>
        /// <response code="200">Successful operation</response>
        /// <response code="404">User not found</response>
        [HttpPost("approveUser/{userId}/{isClubAdmin}/{isCityAdmin}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Пластун, Голова Куреня, Голова Станиці, Голова Округу, Admin")]
        public async Task<IActionResult> ApproveUser(string userId, bool isClubAdmin = false, bool isCityAdmin = false)
        {
            if (userId != null)
            {
                await _confirmedUserService.CreateAsync(await _userManager.GetUserAsync(User), userId, isClubAdmin, isCityAdmin);
                return Ok();
            }
            _loggerService.LogError("User id is null");
            return NotFound();
        }

        /// <summary>
        /// Delete approve
        /// </summary>
        /// <param name="confirmedId">Confirmation ID to be deleted</param>
        /// <response code="200">Successful operation</response>
        /// <response code="404">Confirmed id is 0</response>
        [HttpDelete("deleteApprove/{confirmedId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ApproverDelete(int confirmedId)
        {
            if (confirmedId != 0)
            {
                await _confirmedUserService.DeleteAsync(await _userManager.GetUserAsync(User), confirmedId);
                _loggerService.LogInformation("Approve succesfuly deleted");
                return Ok();
            }
            _loggerService.LogError("Confirmed id is 0");
            return NotFound();
        }
    }
}
