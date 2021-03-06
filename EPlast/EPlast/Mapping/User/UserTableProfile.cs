﻿using AutoMapper;
using EPlast.BLL.DTO;
using EPlast.ViewModels;

namespace EPlast.Mapping
{
    public class UserTableProfile:Profile
    {
        public UserTableProfile()
        {
            CreateMap<UserTableViewModel, UserTableDTO>().ReverseMap();
        }
    }
}
