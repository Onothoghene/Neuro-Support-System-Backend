using Application.DTOs.SessionOccurrence;
using Application.Features.Session.Command;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;

namespace Application.Mappings
{
    public class SessionOccurrenceProfile : Profile
    {
        public SessionOccurrenceProfile()
        {
            // SessionOccurrence -> SessionOccurrenceSummaryVM
            CreateMap<SessionOccurrence, SessionOccurrenceSummaryVM>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // SessionOccurrence -> SessionOccurrenceVM
            CreateMap<SessionOccurrence, SessionOccurrenceVM>()
                .ForMember(dest => dest.SessionTitle, opt => opt.MapFrom(src => src.SessionClass.Title))
                .ForMember(dest => dest.TherapistFirstName, opt => opt.MapFrom(src => src.SessionClass.CreatedByNavigation.FirstName))
                .ForMember(dest => dest.TherapistLastName, opt => opt.MapFrom(src => src.SessionClass.CreatedByNavigation.LastName))
                .ForMember(dest => dest.ChildFirstName, opt => opt.MapFrom(src => src.SessionClass.ChildProfile.FirstName))
                .ForMember(dest => dest.ChildLastName, opt => opt.MapFrom(src => src.SessionClass.ChildProfile.LastName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.SessionClass.Mode.ToString()))
                .ForMember(dest => dest.OnlineDetails, opt => opt.MapFrom(src => src.SessionClass.OnlineDetails));

            CreateMap<CreateSessionClassCommand, SessionOccurrence>()
               .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => DateTime.UtcNow.Date))
               .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Schedule.StartTime))
               .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Schedule.EndTime))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SessionStatus.Scheduled));
        }
    }
}
