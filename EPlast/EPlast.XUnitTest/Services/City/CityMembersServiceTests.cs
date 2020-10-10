﻿using AutoMapper;
using EPlast.BLL.DTO.City;
using EPlast.BLL.Interfaces.City;
using EPlast.BLL.Services.City;
using EPlast.BLL.Services.Interfaces;
using EPlast.BLL.DTO.UserProfiles;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using DatabaseEntities = EPlast.DataAccess.Entities;

namespace EPlast.XUnitTest.Services.City
{
    public class CityMembersServiceTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapper = new Mock<IRepositoryWrapper>();
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>();
        private readonly Mock<ICityService> _cityService;
        private readonly Mock<IUserManagerService> _userManagerService;
        private readonly ICityMembersService _cityMembersService;
        
        public CityMembersServiceTests()
        {
            _cityMembersService = new CityMembersService(_repositoryWrapper.Object, _mapper.Object, null, null);
        }

        [Fact]
        public async Task GetCurrentByCityIdAsyncCorrect()
        {
            // Arrange
            _repositoryWrapper.Setup(r => r.CityMembers.GetAllAsync(It.IsAny<Expression<Func<DatabaseEntities.CityMembers, bool>>>(),
                It.IsAny<Func<IQueryable<DatabaseEntities.CityMembers>, IIncludableQueryable<DatabaseEntities.CityMembers, object>>>()))
                    .ReturnsAsync(new List<DatabaseEntities.CityMembers> { new DatabaseEntities.CityMembers() });

            // Act
            await _cityMembersService.GetMembersByCityIdAsync(It.IsAny<int>());

            // Assert
            _mapper.Verify(m => m.Map<IEnumerable<DatabaseEntities.CityMembers>, IEnumerable<CityMembersDTO>>(It.IsAny<IEnumerable<DatabaseEntities.CityMembers>>()));
        }
        
        /*[Fact]
        public async Task AddFollowerAsyncTest()
        {
            //Arrange
            _cityService.
                Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new CityDTO());
            _userManagerService.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserDTO());
            _repositoryWrapper.Setup(s => s.CityMembers.GetFirstOrDefaultAsync
            (It.IsAny<Expression<Func<CityMembers, bool>>>(), null))
                .ReturnsAsync(new CityMembers());

            //Act
            await _cityMembersService.AddFollowerAsync(It.IsAny<int>(), It.IsAny<string>());

            //Assert
            _repositoryWrapper.Verify(i => i.CityMembers.CreateAsync(It.IsAny<CityMembers>()), Times.Once());
            _repositoryWrapper.Verify(i => i.CityMembers.Delete(It.IsAny<CityMembers>()), Times.Once());
        }*/
    }
}