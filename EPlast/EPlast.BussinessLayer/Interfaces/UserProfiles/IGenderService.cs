﻿using EPlast.BussinessLayer.DTO.UserProfiles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPlast.BussinessLayer.Interfaces.UserProfiles
{
    public interface IGenderService
    {
        Task<IEnumerable<GenderDTO>> GetAllAsync();
    }
}
