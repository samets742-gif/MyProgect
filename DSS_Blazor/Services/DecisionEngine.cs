using DSS_Blazor.Models;

namespace DSS_Blazor.Services
{
    public class DecisionEngine
    {
        private readonly KnowledgeBaseService _kb;
        private readonly SituationMatcher _matcher = new();

        public double SimilarityThreshold { get; set; } = 0.3;
        public int MaxResults { get; set; } = 5;
        public string? CategoryFilter { get; set; }

        public DecisionEngine(KnowledgeBaseService kb) => _kb = kb;

        public async Task<List<RecognitionResult>> AnalyzeAsync(Situation situation)
        {
            var entries = string.IsNullOrEmpty(CategoryFilter)
                ? _kb.Entries.ToList()
                : _kb.Entries
                      .Where(e => string.Equals(e.Category, CategoryFilter,
                                                StringComparison.OrdinalIgnoreCase))
                      .ToList();

            _matcher.CalibrateRanges(entries);

            var results = entries
                .Select(e => new RecognitionResult
                {
                    Entry = e,
                    Similarity = _matcher.ComputeSimilarity(situation, e.ReferenceSituation)
                })
                .Where(r => r.Similarity >= SimilarityThreshold)
                .OrderByDescending(r => r.Similarity)
                .Take(MaxResults)
                .ToList();

            foreach (var r in results) r.Entry.RecordUsage();
            await _kb.SaveAsync();
            return results;
        }

        public async Task LearnAsync(Situation situation, Decision decision, string category)
        {
            var entry = new KnowledgeBaseEntry
            {
                ReferenceSituation = situation.Clone(),
                Decision = decision,
                Category = category
            };
            entry.ReferenceSituation.Name =
                string.IsNullOrWhiteSpace(situation.Name)
                    ? $"Ситуация {DateTime.Now:dd.MM.yyyy HH:mm}"
                    : situation.Name;

            _kb.Add(entry);
            await _kb.SaveAsync();
        }
    }
}
