using Application.DTOs.CommonNodes;

namespace Application.DTOs.Assessment
{
    public class QuestionOptionVM : BaseEntityVM
    {
        public string OptionText { get; set; }
        public decimal Score { get; set; }
        public int DisplayOrder { get; set; }
    }


}
