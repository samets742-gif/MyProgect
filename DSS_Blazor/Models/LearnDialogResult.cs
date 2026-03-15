namespace DSS_Blazor.Models
{
    public class LearnDialogResult
    {
        public string SituationTitle { get; set; } = string.Empty;
        public string Category { get; set; } = "Общее";
        public string DecisionTitle { get; set; } = string.Empty;
        public string DecisionDescription { get; set; } = string.Empty;
        public int Priority { get; set; } = 1;
    }
}
