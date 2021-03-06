﻿using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseEntities = EPlast.DataAccess.Entities;

namespace EPlast.BLL.Services.Region.RegionAccess.RegionAccessGetters
{
    public class RegionAccessForRegionAdminGetter : IRegionAccessGetter
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly AdminType _RegionAdminType;

        public RegionAccessForRegionAdminGetter(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _RegionAdminType = _repositoryWrapper.AdminType.GetFirstAsync(
                    predicate: a => a.AdminTypeName == "Голова Округу").Result;
        }

        public async Task<IEnumerable<DatabaseEntities.Region>> GetRegionAsync(string userId)
        {
            var RegionAdministration = await _repositoryWrapper.RegionAdministration.GetFirstOrDefaultAsync(
                    predicate: c => c.UserId == userId && (DateTime.Now < c.EndDate || c.EndDate == null) && 
                    c.AdminTypeId == _RegionAdminType.ID);
            return RegionAdministration != null ? await _repositoryWrapper.Region.GetAllAsync(
                predicate: c => c.ID == RegionAdministration.RegionId)
                : Enumerable.Empty<DatabaseEntities.Region>();
        }

    }
}
