using System;
using System.Collections.Generic;
using System.Linq;
using DSS_Avalonia.Models;

namespace DSS_Avalonia.Services
{
    /// <summary>
    /// Вычисляет степень сходства между текущей ситуацией и эталонными.
    /// Использует взвешенное евклидово расстояние с нормализацией значений.
    /// </summary>
    public class SituationMatcher
    {
        private readonly Dictionary<string, (double Min, double Max)> _paramRanges = new();

        /// <summary>
        /// Обновить диапазоны параметров по всей базе знаний для нормализации.
        /// Вызывать при каждом изменении базы знаний.
        /// </summary>
        public void CalibrateRanges(IEnumerable<KnowledgeBaseEntry> entries)
        {
            _paramRanges.Clear();
            var allSituations = entries.Select(e => e.ReferenceSituation);

            foreach (var situation in allSituations)
            {
                foreach (var param in situation.Parameters)
                {
                    string key = param.Name.ToLowerInvariant();
                    if (!_paramRanges.TryGetValue(key, out var range))
                        _paramRanges[key] = (param.Value, param.Value);
                    else
                        _paramRanges[key] = (
                            Math.Min(range.Min, param.Value),
                            Math.Max(range.Max, param.Value)
                        );
                }
            }
        }

        /// <summary>
        /// Рассчитать сходство двух ситуаций [0..1].
        /// Учитываются только общие параметры.
        /// </summary>
        public double ComputeSimilarity(Situation current, Situation reference)
        {
            if (current.Parameters.Count == 0)
                return 0;

            double weightedSumSquares = 0;
            double totalWeight = 0;

            foreach (var currentParam in current.Parameters)
            {
                string key = currentParam.Name.ToLowerInvariant();

                var refParam = reference.Parameters.FirstOrDefault(p =>
                    string.Equals(p.Name, currentParam.Name, StringComparison.OrdinalIgnoreCase));

                if (refParam == null)
                    continue; // параметр отсутствует в эталоне — пропускаем

                double weight = currentParam.Weight;
                double normalizedDiff = NormalizedDifference(key, currentParam.Value, refParam.Value);

                weightedSumSquares += weight * normalizedDiff * normalizedDiff;
                totalWeight += weight;
            }

            if (totalWeight == 0)
                return 0;

            double weightedDistance = Math.Sqrt(weightedSumSquares / totalWeight);
            // Преобразуем расстояние в сходство: 0 расстояние → 1.0, большое расстояние → 0
            return Math.Max(0, 1.0 - weightedDistance);
        }

        private double NormalizedDifference(string paramKey, double a, double b)
        {
            if (_paramRanges.TryGetValue(paramKey, out var range))
            {
                double span = range.Max - range.Min;
                if (span > 0)
                    return Math.Abs(a - b) / span;
            }
            // Если диапазон не определён — нормализуем относительно большего значения
            double maxVal = Math.Max(Math.Abs(a), Math.Abs(b));
            if (maxVal < double.Epsilon)
                return 0;
            return Math.Abs(a - b) / maxVal;
        }
    }
}
