﻿using System;

namespace EPlast.BLL.DTO.EventUser
{
    public class EventGeneralInfoDTO
    {
        public int ID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDateStart { get; set; }
        public DateTime EventDateEnd { get; set; }
        public int EventStatusID { get; set; }
    }
}
