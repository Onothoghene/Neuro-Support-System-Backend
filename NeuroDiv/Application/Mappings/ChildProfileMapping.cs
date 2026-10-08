using Application.DTOs.ChildProfile;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Linq;

namespace Application.Mappings
{
    public class ChildProfileMapping : Profile
    {
        public ChildProfileMapping()
        {
            CreateMap<AddChildProfileRequest, ChildProfile>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.DiagnosisType, opt => opt.Ignore())
               .ForMember(dest => dest.Organization, opt => opt.Ignore())
               .ForMember(dest => dest.TherapyGoals, opt => opt.Ignore())
               .ForMember(dest => dest.TherapistAssignments, opt => opt.Ignore())
               .ForMember(dest => dest.Parents, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedByNavigation, opt => opt.Ignore())
               .ForMember(dest => dest.IsActive, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
               .ForMember(dest => dest.Created, opt => opt.Ignore());

            CreateMap<ChildProfile, ChildProfileVM>()
                .ForMember(dest => dest.DiagnosisName, opt => opt.MapFrom(src => src.DiagnosisType.Name))
                .ForMember(dest => dest.DiagnosisCode, opt => opt.MapFrom(src => src.DiagnosisType.Code))
                .ForMember(dest => dest.TherapyGoals, opt => opt.MapFrom(src => src.TherapyGoals))
                .ForMember(dest => dest.AssignedTherapists, opt => opt.MapFrom(src => src.TherapistAssignments.Where(a => a.EndDate == null)))
                .ForMember(dest => dest.Parents, opt => opt.MapFrom(src => src.Parents.Select(p => p.ParentProfile)));

            CreateMap<ChildProfile, ChildProfileSummaryVM>()
                .ForMember(dest => dest.DiagnosisName, opt => opt.MapFrom(src => src.DiagnosisType.Name))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth)))
                .ForMember(dest => dest.TotalGoals, opt => opt.MapFrom(src => src.TherapyGoals.Count))
                .ForMember(dest => dest.AchievedGoals, opt => opt.MapFrom(src => src.TherapyGoals.Count(g => g.Status == GoalStatus.Achieved)))
                .ForMember(dest => dest.InProgressGoals, opt => opt.MapFrom(src => src.TherapyGoals.Count(g => g.Status == GoalStatus.InProgress)));


        }

        private static int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }


    }
}
