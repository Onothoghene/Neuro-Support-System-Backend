using System;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
    public class SubmitAssessmentRequest
    {
        public Guid AssessmentTemplateId { get; set; }
        public Guid ChildProfileId { get; set; }

        /// <summary>Null for standalone assessments.</summary>
        public Guid? SessionOccurrenceId { get; set; }

        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
        public string? ClinicalNotes { get; set; }
        public List<AssessmentResponseRequest> Responses { get; set; } = new();
    }


}
