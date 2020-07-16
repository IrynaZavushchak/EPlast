﻿using AutoMapper;
using EPlast.BLL.DTO.UserProfiles;
using EPlast.WebApi.Models.UserModels.UserProfileFields;

namespace EPlast.WebApi.Mapping.User.UserProfileFields
{
    public class GenderProfile : Profile
    {
        public GenderProfile()
        {
            CreateMap<GenderDTO, GenderViewModel>().ReverseMap();
        }
    }
}
