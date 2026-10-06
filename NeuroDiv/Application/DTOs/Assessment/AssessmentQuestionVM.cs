using Application.DTOs.CommonNodes;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
    public class AssessmentQuestionVM : BaseEntityVM
    {
        public string Text { get; set; }
        public string? HelpText { get; set; }
        public string QuestionType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public List<QuestionOptionVM> Options { get; set; } = new();
    }


}
