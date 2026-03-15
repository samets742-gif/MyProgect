using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSS_Blazor.Models;
using Microsoft.JSInterop;

namespace DSS_Blazor.Services
{
    /// <summary>
    /// Браузерная версия: хранит базу знаний в localStorage.
    /// При первом запуске загружает встроенный JSON из wwwroot/data/.
    /// </summary>
    public class KnowledgeBaseService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private List<KnowledgeBaseEntry> _entries = new();
        private const string StorageKey = "dss_kb_v1";

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public KnowledgeBaseService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public IReadOnlyList<KnowledgeBaseEntry> Entries => _entries.AsReadOnly();

        public async Task InitializeAsync()
        {
            try
            {
                var stored = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(stored))
                {
                    _entries = JsonSerializer.Deserialize<List<KnowledgeBaseEntry>>(stored, _opts)
                               ?? new List<KnowledgeBaseEntry>();
                    return;
                }
            }
            catch { /* localStorage недоступен */ }

            // Первый запуск — загружаем встроенный JSON
            try
            {
                _entries = await _http.GetFromJsonAsync<List<KnowledgeBaseEntry>>(
                    "data/knowledge_base.json", _opts) ?? new();
                await SaveAsync(); // сохраняем в localStorage
            }
            catch { _entries = new(); }
        }

        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_entries, _opts);
                await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            }
            catch { }
        }

        public void Add(KnowledgeBaseEntry entry) => _entries.Add(entry);

        public void Remove(string id)
        {
            var idx = _entries.FindIndex(e => e.Id == id);
            if (idx >= 0) _entries.RemoveAt(idx);
        }

        public IEnumerable<string> GetCategories() =>
            _entries.Select(e => e.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c);

        public async Task ResetToDefaultAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            await InitializeAsync();
        }
    }
}
