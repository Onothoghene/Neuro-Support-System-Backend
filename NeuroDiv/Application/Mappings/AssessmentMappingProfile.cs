using Application.DTOs.Assessment;
using Application.Features.Assessment.Command;
using AutoMapper;
using Domain.Entities;
using System.Linq;

namespace Application.Mappings
{
    public class AssessmentMappingProfile : Profile
    {
        public AssessmentMappingProfile()
        {
            // Template mappings
            CreateMap<AssessmentTemplate, AssessmentTemplateVM>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()));

            CreateMap<CreateAssessmentTemplateCommand, AssessmentTemplate>()
                .ForMember(dest => dest.MinPossibleScore, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.ScoreRanges, opt => opt.MapFrom(src => src.ScoreRanges))
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections))
                .ForMember(dest => dest.IsSystemTemplate, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<AssessmentTemplate, AssessmentTemplateSummaryVM>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()))
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.Sections.SelectMany(s => s.Questions).Count()));

            CreateMap<AssessmentSection, AssessmentSectionVM>();

            CreateMap<CreateAssessmentSectionRequest, AssessmentSection>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<AssessmentQuestion, AssessmentQuestionVM>()
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType.ToString()));

            CreateMap<CreateAssessmentQuestionRequest, AssessmentQuestion>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            CreateMap<AssessmentQuestionOption, QuestionOptionVM>();

            CreateMap<CreateQuestionOptionRequest, AssessmentQuestionOption>();

            CreateMap<AssessmentScoreRange, ScoreRangeVM>();

            // Snapshot mappings
            CreateMap<AssessmentSnapshot, AssessmentSnapshotVM>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.AssessmentTemplate.Name))
                .ForMember(dest => dest.ChildFirstName, opt => opt.MapFrom(src => src.ChildProfile.FirstName))
                .ForMember(dest => dest.ChildLastName, opt => opt.MapFrom(src => src.ChildProfile.LastName))
                .ForMember(dest => dest.TherapistFirstName, opt => opt.MapFrom(src => src.Therapist.FirstName))
                .ForMember(dest => dest.TherapistLastName, opt => opt.MapFrom(src => src.Therapist.LastName));

            CreateMap<AssessmentSnapshot, AssessmentSnapshotSummaryVM>()
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.AssessmentTemplate.Name));

            CreateMap<AssessmentResponse, AssessmentResponseVM>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.AssessmentQuestion.Text))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.AssessmentQuestion.QuestionType.ToString()))
                .ForMember(dest => dest.SelectedOption, opt => opt.MapFrom(src => src.SelectedOption != null ? src.SelectedOption.OptionText : null));

        }
    }
}