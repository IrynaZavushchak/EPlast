﻿using AutoMapper;
using EPlast.BLL.DTO.Admin;
using EPlast.DataAccess.Entities;
using EPlast.WebApi.Models.Admin;

namespace EPlast.WebApi.Mapping.Admin
{
    public class AdminTypeProfile : Profile
    {
        public AdminTypeProfile()
        {
            CreateMap<AdminType, AdminTypeDTO>().ReverseMap();
            CreateMap<AdminTypeDTO, AdminTypeViewModel>().ReverseMap();
        }
    }
}
