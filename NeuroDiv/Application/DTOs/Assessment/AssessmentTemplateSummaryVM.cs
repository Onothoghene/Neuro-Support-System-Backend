using Application.DTOs.CommonNodes;

namespace Application.DTOs.Assessment
{
    public class AssessmentTemplateSummaryVM : BaseEntityVM
    {
        public string Name { get; set; }
        public string? TargetCondition { get; set; }
        public bool IsSystemTemplate { get; set; }
        public string Visibility { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsActive { get; set; }
    }


}
