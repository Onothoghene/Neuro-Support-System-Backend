using Application.DTOs.Session;
using Application.Features.Session.Command;
using AutoMapper;
using Domain.Entities;
using System;
using System.Linq;

namespace Application.Mappings
{
    public class SessionProfile : Profile
    {
        public SessionProfile()
        {
            // SessionClass -> SessionClassVm
            CreateMap<SessionClass, SessionClassVM>()
                .ForMember(dest => dest.TherapistFirstName, opt => opt.MapFrom(src => src.CreatedByNavigation.FirstName))
                .ForMember(dest => dest.TherapistLastName, opt => opt.MapFrom(src => src.CreatedByNavigation.LastName))
                .ForMember(dest => dest.ChildFirstName, opt => opt.MapFrom(src => src.ChildProfile.FirstName))
                .ForMember(dest => dest.ChildLastName, opt => opt.MapFrom(src => src.ChildProfile.LastName))
                .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.Mode.ToString()))
                .ForMember(dest => dest.UpcomingOccurrences, opt => opt.MapFrom(src => src.Occurrences
                           .Where(o => o.ScheduledDate >= DateTime.UtcNow.Date)
                           .OrderBy(o => o.ScheduledDate)
                           .Take(10)));

            // Session → SessionSummaryVM
            //CreateMap<SessionClass, SessionSummaryVM>()
            //    .ForMember(dest => dest.Type,
            //        opt => opt.MapFrom(src => src.Type.ToString()))
            //    .ForMember(dest => dest.Status,
            //        opt => opt.MapFrom(src => src.Status.ToString()))
            //    .ForMember(dest => dest.DurationLabel,
            //        opt => opt.MapFrom(src => src.SessionDuration != null
            //            ? src.SessionDuration.Label : "Custom"))
            //    .ForMember(dest => dest.ChildCount,
            //        opt => opt.MapFrom(src => src.ChildSessionRecords.Count));

        }
    }
}
