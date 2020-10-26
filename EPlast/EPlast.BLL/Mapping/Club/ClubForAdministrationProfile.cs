﻿using AutoMapper;
using EPlast.BLL.DTO.Club;
using System;
using System.Collections.Generic;
using System.Text;
using DatabaseEntities = EPlast.DataAccess.Entities;


namespace EPlast.BLL.Mapping.Club
{
    class ClubForAdministrationProfile : Profile
    {
        public ClubForAdministrationProfile()
        {
            CreateMap<DatabaseEntities.Club, ClubForAdministrationDTO>().ReverseMap();
        }
    }
}
