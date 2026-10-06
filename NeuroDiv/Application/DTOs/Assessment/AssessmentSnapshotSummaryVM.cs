using Application.DTOs.CommonNodes;
using System;

namespace Application.DTOs.Assessment
{
    public class AssessmentSnapshotSummaryVM : BaseEntityVM
    {
        public string TemplateName { get; set; }
        public DateTime AssessmentDate { get; set; }
        public decimal TotalScore { get; set; }
        public string? ScoreLabel { get; set; }
        public bool IsLegacyUpload { get; set; }
        public bool IsVerified { get; set; }
    }


}
