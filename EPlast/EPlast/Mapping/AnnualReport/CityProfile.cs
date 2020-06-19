﻿using AutoMapper;
using EPlast.BussinessLayer.DTO.AnnualReport;
using EPlast.ViewModels.AnnualReport;
using DatabaseEntities = EPlast.DataAccess.Entities;

namespace EPlast.Mapping.AnnualReport
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<DatabaseEntities.City, CityAnnualReportDTO>().ReverseMap();
            CreateMap<CityViewModel, CityAnnualReportDTO>().ReverseMap();
        }
    }
}