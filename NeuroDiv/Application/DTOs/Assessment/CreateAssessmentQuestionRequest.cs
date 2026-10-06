using Domain.Enums;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
	public class CreateAssessmentQuestionRequest
    {
        public string Text { get; set; }
        public string? HelpText { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool IsRequired { get; set; } = true;
        public int DisplayOrder { get; set; } = 1;
        public List<CreateQuestionOptionRequest> Options { get; set; } = new();
    }
}
