using Application.DTOs.CommonNodes;
using System;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
    public class AssessmentSnapshotVM : BaseEntityVM
    {
        public string TemplateName { get; set; }
        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public DateTime AssessmentDate { get; set; }
        public decimal TotalScore { get; set; }
        public string? ScoreLabel { get; set; }
        public string? ClinicalNotes { get; set; }
        public bool IsLegacyUpload { get; set; }
        public string? UploadedFilePath { get; set; }
        public bool IsVerified { get; set; }
        public List<AssessmentResponseVM> Responses { get; set; } = new();
    }


}
