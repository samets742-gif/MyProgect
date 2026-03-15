namespace DSS_Avalonia.Models
{
    /// <summary>Данные, возвращаемые диалогом добавления прецедента.</summary>
    public class LearnDialogResult
    {
        public string SituationTitle { get; init; } = string.Empty;
        public string Category { get; init; } = "Общее";
        public string DecisionTitle { get; init; } = string.Empty;
        public string DecisionDescription { get; init; } = string.Empty;
        public int Priority { get; init; } = 1;
    }
}
