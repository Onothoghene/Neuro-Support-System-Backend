using Domain.Enums;

namespace Application.DTOs.Assessment
{
    public class UpdateAssessmentTemplateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TargetCondition { get; set; }
        public TemplateVisibility? Visibility { get; set; }
        public bool? IsActive { get; set; }
    }


}
