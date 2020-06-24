﻿using AutoMapper;
using EPlast.BusinessLogicLayer.DTO;
using EPlast.BusinessLogicLayer.DTO.UserProfiles;
using EPlast.BusinessLogicLayer.Interfaces.AzureStorage;
using EPlast.BusinessLogicLayer.Interfaces.UserProfiles;
using EPlast.BusinessLogicLayer.Services.UserProfiles;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace EPlast.XUnitTest.Services.UserArea
{
    public class UserServiceTests
    {
        private Mock<IRepositoryWrapper> _repoWrapper;
        private Mock<IUserStore<User>> _userStoreMock;
        private Mock<UserManager<User>> _userManager;
        private Mock<IWebHostEnvironment> _hostEnv;
        private Mock<IMapper> _mapper;
        private Mock<IWorkService> _workService;
        private Mock<IEducationService> _educationService;
        private Mock<IUserBlobStorageRepository> _userBlobStorage;
        private Mock<IWebHostEnvironment> _env;

        public UserServiceTests()
        {
            _repoWrapper = new Mock<IRepositoryWrapper>();
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mapper = new Mock<IMapper>();
            _workService = new Mock<IWorkService>();
            _educationService = new Mock<IEducationService>();
            _userBlobStorage=new Mock<IUserBlobStorageRepository>();
            _env=new Mock<IWebHostEnvironment>();
        }

        private UserService GetService()
        {
            return new UserService(_repoWrapper.Object, _userManager.Object, _mapper.Object, _workService.Object, _educationService.Object, _userBlobStorage.Object,_env.Object);
        }
        [Fact]
        public async Task GetUserProfileTest()
        {
            _repoWrapper.SetupSequence(r => r.User.GetFirstAsync(It.IsAny<Expression<Func<User, bool>>>(), null)).ReturnsAsync(new User
            {
                FirstName = "Vova",
                LastName = "Vermii",
                UserProfile = new UserProfile
                {
                    Nationality = new Nationality { Name = "Українець" },
                    Religion = new Religion { Name = "Християнство" },
                    Education = new Education() { PlaceOfStudy = "ЛНУ", Speciality = "КН" },
                    Degree = new Degree { Name = "Бакалавр" },
                    Work = new Work { PlaceOfwork = "SoftServe", Position = "ProjectManager" },
                    Gender = new Gender { Name = "Чоловік" }
                }
            });

            var service = GetService();
            _mapper.Setup(x => x.Map<User, UserDTO>(It.IsAny<User>())).Returns(new UserDTO());
            // Act
            var result = await service.GetUserAsync("1");
            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<UserDTO>(result);
        }
        [Fact]
        public void GetConfirmedUsersTest()
        {
            UserDTO user = new UserDTO { ConfirmedUsers = new List<ConfirmedUserDTO>() };

            var service = GetService();            // Act
            var result = service.GetConfirmedUsers(user);
            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsAssignableFrom<IEnumerable<ConfirmedUserDTO>>(result);
        }
        [Fact]
        public void GetClubAdminConfirmedUserTest()
        {
            UserDTO user = new UserDTO { ConfirmedUsers = new List<ConfirmedUserDTO>() };

            var service = GetService();            // Act
            var result = service.GetConfirmedUsers(user);
            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsAssignableFrom<IEnumerable<ConfirmedUserDTO>>(result);
        }
        [Fact]
        public void GetCityAdminConfirmedUserTest()
        {
            UserDTO user = new UserDTO { ConfirmedUsers = new List<ConfirmedUserDTO>() };

            var service = GetService();            // Act
            var result = service.GetConfirmedUsers(user);
            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ConfirmedUserDTO>>(result);
        }
        [Fact]
        public async Task CanApproveTest()
        {
            var conUser = new ConfirmedUserDTO { UserID = "1", ConfirmDate = DateTime.Now, isClubAdmin = false, isCityAdmin = false };
            var appUser = new ApproverDTO { UserID = "3", ConfirmedUser = conUser };
            conUser.Approver = appUser;

            UserDTO user = new UserDTO { ConfirmedUsers = new List<ConfirmedUserDTO>() };
            var confUsers = new List<ConfirmedUserDTO> { conUser, conUser };
            _userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new User { Id = "1" });

            var service = GetService();            // Act
            var result = await service.CanApproveAsync(confUsers, "2", It.IsAny<ClaimsPrincipal>());
            // Assert
            var res = Assert.IsType<bool>(result);
            Assert.True(result);
        }
        [Fact]
        public async Task CanApproveTestFailure()
        {
            UserDTO user = new UserDTO { ConfirmedUsers = new List<ConfirmedUserDTO>() };
            var conUser = new ConfirmedUserDTO();
            var confUsers = new List<ConfirmedUserDTO> { conUser, conUser, conUser, conUser };
            _userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new User { Id = "1" });

            var service = GetService();            // Act
            var result = await service.CanApproveAsync(confUsers, "1", It.IsAny<ClaimsPrincipal>());
            // Assert
            var res = Assert.IsType<bool>(result);
            Assert.False(result);
        }
        [Fact]
        public async Task CheckOrAddPlastunRoleTest()
        {
            _userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());

            var service = GetService();            // Act
            var result = await service.CheckOrAddPlastunRoleAsync("1", DateTime.MinValue);
            // Assert
            var res = Assert.IsType<TimeSpan>(result);
        }
        [Fact]
        public async Task UpdateTest()
        {
            var userDTO = new UserDTO
            {
                FirstName = "Vova",
                LastName = "Vermii",
                UserProfile = new UserProfileDTO
                {
                    Nationality = new NationalityDTO { Name = "Українець" },
                    NationalityId = 1,
                    Religion = new ReligionDTO { Name = "Християнство" },
                    ReligionId = 1,
                    Education = new EducationDTO() { PlaceOfStudy = "ЛНУ", Speciality = "КН" },
                    EducationId = 1,
                    Degree = new DegreeDTO { Name = "Бакалавр" },
                    DegreeId = 1,
                    Work = new WorkDTO { PlaceOfwork = "SoftServe", Position = "ProjectManager" },
                    WorkId = 1,
                    Gender = new GenderDTO { Name = "Чоловік" },
                    GenderID = 1
                }
            };
            var user = new User
            {
                FirstName = "Vova",
                LastName = "Vermii",
                UserProfile = new UserProfile
                {
                    Nationality = new Nationality { Name = "Українець" },
                    Religion = new Religion { Name = "Християнство" },
                    Education = new Education() { PlaceOfStudy = "ЛНУ", Speciality = "КН" },
                    Degree = new Degree { Name = "Бакалавр" },
                    Work = new Work { PlaceOfwork = "SoftServe", Position = "ProjectManager" },
                    Gender = new Gender { Name = "Чоловік" }
                }
            };
            _repoWrapper.Setup(r => r.User.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), null)).ReturnsAsync(user);
            _repoWrapper.Setup(r => r.UserProfile.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), null)).ReturnsAsync(new UserProfile());
            _repoWrapper.Setup(r => r.Education.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Education, bool>>>(), null)).ReturnsAsync(new Education
            {
                PlaceOfStudy = "place",
                Speciality = "spec",
            });
            _repoWrapper.Setup(r => r.Work.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Work, bool>>>(), null)).ReturnsAsync(new Work
            {
                PlaceOfwork = "place",
                Position = "position",
            });
            _mapper.Setup(x => x.Map<UserDTO, User>(It.IsAny<UserDTO>())).Returns(user);
            var mockFile = new Mock<IFormFile>();

            var service = GetService();            // Act
            await service.UpdateAsync(userDTO, mockFile.Object, 1, 1, 1, 1);
            // Assert
            _repoWrapper.Verify(r => r.User.Update(It.IsAny<User>()), Times.Once());
            _repoWrapper.Verify(r => r.UserProfile.Update(It.IsAny<UserProfile>()), Times.Once());
            _repoWrapper.Verify(r => r.SaveAsync(), Times.Once());
        }
    }
}
