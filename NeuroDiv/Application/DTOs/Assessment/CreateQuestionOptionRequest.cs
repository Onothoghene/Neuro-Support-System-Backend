namespace Application.DTOs.Assessment
{
	public class CreateQuestionOptionRequest
    {
        public string OptionText { get; set; }
        public decimal Score { get; set; } = 0;
        public int DisplayOrder { get; set; } = 1;
    }

}
