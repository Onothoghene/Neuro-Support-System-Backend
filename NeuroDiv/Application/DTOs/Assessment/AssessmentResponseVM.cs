using System;

namespace Application.DTOs.Assessment
{
    public class AssessmentResponseVM
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string? SelectedOption { get; set; }
        public string? TextResponse { get; set; }
        public decimal Score { get; set; }
    }


}
