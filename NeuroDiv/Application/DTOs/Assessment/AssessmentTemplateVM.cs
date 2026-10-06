using Application.DTOs.CommonNodes;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
    public class AssessmentTemplateVM : BaseEntityVM
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TargetCondition { get; set; }
        public bool IsSystemTemplate { get; set; }
        public string Visibility { get; set; }
        public bool HasAutoScoring { get; set; }
        public decimal MinPossibleScore { get; set; }
        public decimal MaxPossibleScore { get; set; }
        public bool IsActive { get; set; }
        public List<AssessmentSectionVM> Sections { get; set; } = new();
        public List<ScoreRangeVM> ScoreRanges { get; set; } = new();
    }


}
