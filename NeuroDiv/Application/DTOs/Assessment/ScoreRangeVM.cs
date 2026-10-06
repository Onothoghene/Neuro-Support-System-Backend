using Application.DTOs.CommonNodes;

namespace Application.DTOs.Assessment
{
    public class ScoreRangeVM : BaseEntityVM
    {
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public string Label { get; set; }
        public string? Description { get; set; }
        public string? ColorCode { get; set; }
    }


}
