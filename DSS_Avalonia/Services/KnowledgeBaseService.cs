using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSS_Avalonia.Models;

namespace DSS_Avalonia.Services
{
    /// <summary>
    /// Сервис управления базой знаний: загрузка, сохранение, CRUD-операции.
    /// </summary>
    public class KnowledgeBaseService
    {
        private readonly string _filePath;
        private List<KnowledgeBaseEntry> _entries = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public KnowledgeBaseService(string filePath)
        {
            _filePath = filePath;
        }

        public IReadOnlyList<KnowledgeBaseEntry> Entries => _entries.AsReadOnly();

        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                _entries = new List<KnowledgeBaseEntry>();
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath, System.Text.Encoding.UTF8);
                _entries = JsonSerializer.Deserialize<List<KnowledgeBaseEntry>>(json, _jsonOptions)
                           ?? new List<KnowledgeBaseEntry>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось загрузить базу знаний из '{_filePath}': {ex.Message}", ex);
            }
        }

        public void Save()
        {
            string? dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(_entries, _jsonOptions);
            File.WriteAllText(_filePath, json, System.Text.Encoding.UTF8);
        }

        public void Add(KnowledgeBaseEntry entry)
        {
            if (_entries.Any(e => e.Id == entry.Id))
                throw new InvalidOperationException($"Запись с Id={entry.Id} уже существует.");
            _entries.Add(entry);
        }

        public void Update(KnowledgeBaseEntry entry)
        {
            int idx = _entries.FindIndex(e => e.Id == entry.Id);
            if (idx < 0)
                throw new KeyNotFoundException($"Запись с Id={entry.Id} не найдена.");
            _entries[idx] = entry;
        }

        public void Remove(string id)
        {
            int idx = _entries.FindIndex(e => e.Id == id);
            if (idx < 0)
                throw new KeyNotFoundException($"Запись с Id={id} не найдена.");
            _entries.RemoveAt(idx);
        }

        public KnowledgeBaseEntry? GetById(string id) =>
            _entries.FirstOrDefault(e => e.Id == id);

        public IEnumerable<KnowledgeBaseEntry> GetByCategory(string category) =>
            _entries.Where(e =>
                string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<string> GetCategories() =>
            _entries.Select(e => e.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c);
    }
}
