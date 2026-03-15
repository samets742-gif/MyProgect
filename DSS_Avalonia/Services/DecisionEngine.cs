using System;
using System.Collections.Generic;
using System.Linq;
using DSS_Avalonia.Models;

namespace DSS_Avalonia.Services
{
    public class DecisionEngine
    {
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly SituationMatcher _matcher;

        public double SimilarityThreshold { get; set; } = 0.3;
        public int MaxResults { get; set; } = 5;
        public string? CategoryFilter { get; set; }

        public DecisionEngine(KnowledgeBaseService knowledgeBase)
        {
            _knowledgeBase = knowledgeBase;
            _matcher = new SituationMatcher();
        }

        public List<RecognitionResult> Analyze(Situation currentSituation)
        {
            if (currentSituation == null) throw new ArgumentNullException(nameof(currentSituation));

            var entries = _knowledgeBase.Entries;

            if (!string.IsNullOrEmpty(CategoryFilter))
                entries = entries
                    .Where(e => string.Equals(e.Category, CategoryFilter,
                                              StringComparison.OrdinalIgnoreCase))
                    .ToList();

            _matcher.CalibrateRanges(entries);

            var results = entries
                .Select(entry => new RecognitionResult
                {
                    Entry = entry,
                    Similarity = _matcher.ComputeSimilarity(currentSituation, entry.ReferenceSituation)
                })
                .Where(r => r.Similarity >= SimilarityThreshold)
                .OrderByDescending(r => r.Similarity)
                .Take(MaxResults)
                .ToList();

            foreach (var result in results)
                result.Entry.RecordUsage();

            return results;
        }

        public void LearnFromSituation(Situation situation, Decision decision, string category = "")
        {
            var entry = new KnowledgeBaseEntry
            {
                ReferenceSituation = situation.Clone(),
                Decision = decision,
                Category = category
            };
            entry.ReferenceSituation.Name =
                string.IsNullOrEmpty(situation.Name)
                    ? $"Ситуация {DateTime.Now:yyyy-MM-dd HH:mm}"
                    : situation.Name;

            _knowledgeBase.Add(entry);
            _knowledgeBase.Save();
        }
    }
}
