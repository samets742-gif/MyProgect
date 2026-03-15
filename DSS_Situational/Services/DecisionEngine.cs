using System;
using System.Collections.Generic;
using System.Linq;
using DSS_Situational.Models;

namespace DSS_Situational.Services
{
    /// <summary>
    /// Движок принятия решений — ядро СППР.
    /// Реализует ситуационный подход: текущая ситуация сопоставляется с эталонными,
    /// выбираются наиболее близкие прецеденты и формируется список рекомендаций.
    /// </summary>
    public class DecisionEngine
    {
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly SituationMatcher _matcher;

        /// <summary>Минимальный порог сходства для включения в результат.</summary>
        public double SimilarityThreshold { get; set; } = 0.3;

        /// <summary>Максимальное число рекомендаций в результате.</summary>
        public int MaxResults { get; set; } = 5;

        /// <summary>Фильтр по категории (null — все категории).</summary>
        public string? CategoryFilter { get; set; }

        public DecisionEngine(KnowledgeBaseService knowledgeBase)
        {
            _knowledgeBase = knowledgeBase;
            _matcher = new SituationMatcher();
        }

        /// <summary>
        /// Выполнить распознавание ситуации и получить упорядоченные рекомендации.
        /// </summary>
        /// <param name="currentSituation">Текущая (анализируемая) ситуация.</param>
        /// <returns>
        /// Список результатов, отсортированных по убыванию сходства.
        /// </returns>
        public List<RecognitionResult> Analyze(Situation currentSituation)
        {
            if (currentSituation == null)
                throw new ArgumentNullException(nameof(currentSituation));

            var entries = _knowledgeBase.Entries;

            // Применяем фильтр по категории
            if (!string.IsNullOrEmpty(CategoryFilter))
                entries = entries
                    .Where(e => string.Equals(e.Category, CategoryFilter,
                                              StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // Калибруем диапазоны для нормализации
            _matcher.CalibrateRanges(entries);

            var results = entries
                .Select(entry => new RecognitionResult
                {
                    Entry = entry,
                    Similarity = _matcher.ComputeSimilarity(
                        currentSituation, entry.ReferenceSituation)
                })
                .Where(r => r.Similarity >= SimilarityThreshold)
                .OrderByDescending(r => r.Similarity)
                .Take(MaxResults)
                .ToList();

            // Обновляем статистику использования
            foreach (var result in results)
                result.Entry.RecordUsage();

            return results;
        }

        /// <summary>
        /// Добавить новый прецедент в базу знаний (обучение системы).
        /// </summary>
        public void LearnFromSituation(Situation situation, Decision decision,
                                       string category = "")
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
