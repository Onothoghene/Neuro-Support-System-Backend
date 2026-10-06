using Application.DTOs.CommonNodes;
using System.Collections.Generic;

namespace Application.DTOs.Assessment
{
    public class AssessmentSectionVM : BaseEntityVM
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public List<AssessmentQuestionVM> Questions { get; set; } = new();
    }


}
