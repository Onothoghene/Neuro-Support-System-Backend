using System;

namespace Application.DTOs.Assessment
{
    public class AssessmentResponseRequest
    {
        public Guid QuestionId { get; set; }

        /// <summary>For Scale/MultiChoice/YesNo.</summary>
        public Guid? SelectedOptionId { get; set; }

        /// <summary>For Text questions.</summary>
        public string? TextResponse { get; set; }
    }


}
