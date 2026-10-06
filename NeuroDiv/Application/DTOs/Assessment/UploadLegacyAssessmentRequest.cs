using System;

namespace Application.DTOs.Assessment
{
    public class UploadLegacyAssessmentRequest
    {
        public Guid AssessmentTemplateId { get; set; }
        public Guid ChildProfileId { get; set; }
        public Guid? SessionOccurrenceId { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string UploadedFilePath { get; set; }
        public string? ClinicalNotes { get; set; }
    }


}
