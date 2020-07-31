﻿using EPlast.BLL.DTO.ActiveMembership;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPlast.BLL.Interfaces.ActiveMembership
{
    /// <summary>
    /// Implement  operations for work with active membership
    /// </summary>
    public interface IActiveMembershipService
    {
        /// <summary>
        /// Returns all plast degrees
        /// </summary>
        /// <returns>All plast degrees</returns>
        public Task<IEnumerable<PlastDegreeDTO>> GetDergeesAsync();
        /// <summary>
        /// Returns user date of entry
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>User date of entry</returns>
        public Task<DateTime> GetDateOfEntryAsync(string userId);
        /// <summary>
        /// Returns All degrees of current user
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>All degrees of current user</returns>
        public Task<IEnumerable<UserPlastDegreeDTO>> GetUserPlastDegreesAsync(string userId);
        /// <summary>
        /// Returns All access levels of current user
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>All access levels of current user</returns>
        public Task<IEnumerable<string>> GetUserAccessLevelsAsync(string userId);
    }
}
