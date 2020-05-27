﻿using AutoMapper;
using EPlast.BussinessLayer.DTO.EventUser;
using EPlast.BussinessLayer.Interfaces.Events;
using EPlast.BussinessLayer.Interfaces.EventUser;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EPlast.BussinessLayer.Services.EventUser
{
    public class EventUserManager : IEventUserManager
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IParticipantStatusManager _participantStatusManager;
        private readonly IParticipantManager _participantManager;
        private readonly IEventAdminManager _eventAdminManager;

        public EventUserManager(IRepositoryWrapper repoWrapper, UserManager<User> userManager, IParticipantStatusManager participantStatusManager, IMapper mapper, IParticipantManager participantManager, IEventAdminManager eventAdminManager)
        {
            _repoWrapper = repoWrapper;
            _userManager = userManager;
            _participantStatusManager = participantStatusManager;
            _mapper = mapper;
            _participantManager = participantManager;
            _eventAdminManager = eventAdminManager;
        }

        public EventUserDTO EventUser(string userId, ClaimsPrincipal user)
        {
            var currentUserId = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userId))
            {
                userId = currentUserId;
            }

            var _user = _repoWrapper.User.FindByCondition(q => q.Id == userId).First();
            EventUserDTO model = new EventUserDTO { User = _mapper.Map<User, UserDTO>(_user) };
            var eventAdmins = _eventAdminManager.GetEventAdminsByUserId(userId);
            var participants = _participantManager.GetParticipantsByUserId(userId);
            model.CreatedEvents = new List<EventGeneralInfoDTO>();
            foreach (var eventAdmin in eventAdmins)
            {
                var eventToAdd = _mapper.Map<Event, EventGeneralInfoDTO>(eventAdmin.Event);
                model.CreatedEvents.Add(eventToAdd);
            }
            model.PlanedEvents = new List<EventGeneralInfoDTO>();
            model.VisitedEvents = new List<EventGeneralInfoDTO>();
            foreach (var participant in participants)
            {
                var eventToAdd = _mapper.Map<Event, EventGeneralInfoDTO>(participant.Event);
                if (participant.Event.EventDateEnd >= DateTime.Now)
                {
                    model.PlanedEvents.Add(eventToAdd);
                }
                else if (participant.Event.EventDateEnd < DateTime.Now &&
                         participant.ParticipantStatusId == _participantStatusManager.GetStatusId("Учасник"))
                {
                    model.VisitedEvents.Add(eventToAdd);
                }
            }
            return model;
        }
    }
}