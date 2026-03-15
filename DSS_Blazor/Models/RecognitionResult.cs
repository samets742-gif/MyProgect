namespace DSS_Blazor.Models
{
    /// <summary>
    /// Результат сопоставления текущей ситуации с эталоном из базы знаний.
    /// </summary>
    public class RecognitionResult
    {
        /// <summary>Эталонная запись из базы знаний.</summary>
        public KnowledgeBaseEntry Entry { get; set; } = null!;

        /// <summary>
        /// Коэффициент сходства [0..1], где 1 — полное совпадение.
        /// </summary>
        public double Similarity { get; set; }

        /// <summary>Рекомендуемое решение из этой записи.</summary>
        public Decision RecommendedDecision => Entry.Decision;

        /// <summary>Название эталонной ситуации.</summary>
        public string SituationName => Entry.ReferenceSituation.Name;

        /// <summary>Категория записи.</summary>
        public string Category => Entry.Category;

        /// <summary>Отображаемое сходство в процентах.</summary>
        public string SimilarityPercent => $"{Similarity * 100:F1}%";

        /// <summary>Уровень уверенности на основе сходства.</summary>
        public string ConfidenceLevel => Similarity switch
        {
            >= 0.90 => "Высокий",
            >= 0.70 => "Средний",
            >= 0.50 => "Низкий",
            _ => "Очень низкий"
        };
    }
}
