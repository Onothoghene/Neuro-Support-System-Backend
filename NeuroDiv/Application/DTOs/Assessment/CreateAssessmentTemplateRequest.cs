using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
    public class CreateAssessmentTemplateRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TargetCondition { get; set; }
        public TemplateVisibility Visibility { get; set; }
            = TemplateVisibility.Private;
        public Guid? OrganizationId { get; set; }
        public bool HasAutoScoring { get; set; } = true;
        public List<CreateAssessmentSectionRequest> Sections { get; set; } = new();
        public List<CreateScoreRangeRequest> ScoreRanges { get; set; } = new();
    }

}
