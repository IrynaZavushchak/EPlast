﻿using AutoMapper;
using EPlast.BLL.DTO.EventUser;
using EPlast.ViewModels.EventUser;

namespace EPlast.Mapping.EventUser
{
    public class EventAdministrationProfile : Profile
    {
        public EventAdministrationProfile()
        {
            CreateMap<EventAdministrationDTO, EventAdministrationViewModel>();
        }
    }
}
