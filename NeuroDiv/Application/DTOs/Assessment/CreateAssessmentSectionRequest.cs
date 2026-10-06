using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
	public class CreateAssessmentSectionRequest
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 1;
        public List<CreateAssessmentQuestionRequest> Questions { get; set; } = new();
    }
}
