# Архитектура системы СППР

## Обзор

Система построена на принципах **Clean Architecture** с разделением на слои:

```
┌─────────────────────────────────────────────────────┐
│                  Presentation Layer                   │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────┐  │
│  │DSS_Blazor   │  │DSS_Avalonia  │  │DSS_Situatio│  │
│  │(Web/Mobile) │  │(Desktop)     │  │nal (WPF)   │  │
│  └─────────────┘  └──────────────┘  └────────────┘  │
├─────────────────────────────────────────────────────┤
│                  Business Logic Layer                 │
│  ┌─────────────────────────────────────────────────┐ │
│  │              DecisionEngine                      │ │
│  │  ┌──────────────────┐  ┌──────────────────────┐ │ │
│  │  │ SituationMatcher │  │ KnowledgeBaseService │ │ │
│  │  └──────────────────┘  └──────────────────────┘ │ │
│  └─────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────┤
│                    Domain Layer                       │
│  Situation · SituationParameter · Decision           │
│  KnowledgeBaseEntry · RecognitionResult              │
└─────────────────────────────────────────────────────┘
```

---

## Слои и компоненты

### Domain Layer (Models/)

| Класс | Назначение |
|-------|-----------|
| `Situation` | Описание ситуации — список параметров с именами и значениями |
| `SituationParameter` | Один признак: имя, значение, вес [0..1], единица измерения |
| `Decision` | Рекомендуемое решение: заголовок, описание, приоритет, список действий |
| `KnowledgeBaseEntry` | Запись БЗ: эталонная ситуация + решение + метаданные |
| `RecognitionResult` | Результат сопоставления: запись БЗ + коэффициент сходства |

### Business Logic Layer (Services/)

#### `SituationMatcher`
Вычисляет степень сходства двух ситуаций.

- **Нормализация:** диапазоны значений каждого параметра вычисляются из всей базы знаний
- **Расстояние:** взвешенное евклидово расстояние по общим параметрам
- **Результат:** коэффициент сходства [0..1]

#### `KnowledgeBaseService`
CRUD-операции над базой знаний.

- **Blazor-версия:** хранение в `localStorage` браузера; первый запуск — загрузка из `wwwroot/data/knowledge_base.json`
- **Desktop-версии:** хранение в `knowledge_base.json` рядом с исполняемым файлом

#### `DecisionEngine`
Оркестратор анализа.

1. Фильтрация базы знаний по категории (опционально)
2. Калибровка диапазонов для нормализации
3. Вычисление сходства для каждой записи
4. Фильтрация по порогу сходства
5. Сортировка по убыванию сходства
6. Обновление статистики использования

### Presentation Layer

#### DSS_Blazor (основная версия)
- **Технология:** Blazor WebAssembly, .NET 8
- **Паттерн:** компонентная архитектура Razor
- **Хранение данных:** `localStorage` (браузерный) через `IJSRuntime`
- **Два экрана:** анализ ситуации (`Index.razor`) + база знаний (`KnowledgeBase.razor`)

#### DSS_Avalonia
- **Технология:** Avalonia 11, .NET 8
- **Паттерн:** MVVM + `IDialogService`
- **Особенности:** асинхронные команды (`AsyncRelayCommand`), кроссплатформенные диалоги

#### DSS_Situational (WPF)
- **Технология:** WPF, .NET 8 Windows
- **Паттерн:** MVVM

---

## Поток данных при анализе

```
User Input
    │
    ▼
[Situation]          [KnowledgeBaseService]
name, params ──────► entries: List<KBEntry>
                          │
                          ▼
                 [SituationMatcher.CalibrateRanges()]
                 Compute min/max per parameter
                          │
                          ▼
              for each entry in KB:
              [SituationMatcher.ComputeSimilarity()]
              → weighted normalized euclidean
                          │
                          ▼
              Filter by threshold (default 0.3)
              Sort by similarity DESC
              Take top N (default 5)
                          │
                          ▼
              [List<RecognitionResult>]
              → Update usage statistics
              → Persist KB
                          │
                          ▼
                    UI renders results
```

---

## Хранение данных

### Формат базы знаний (JSON)

```json
[
  {
    "id": "kb-001",
    "category": "Диагностика оборудования",
    "usageCount": 5,
    "lastUsed": "2026-03-15T19:00:00",
    "referenceSituation": {
      "id": "sit-001",
      "name": "Перегрев подшипника",
      "parameters": [
        { "name": "Температура", "value": 95.0, "weight": 1.0, "unit": "°C" },
        { "name": "Вибрация",    "value": 8.5,  "weight": 0.9, "unit": "мм/с" }
      ]
    },
    "decision": {
      "title": "Аварийная остановка",
      "description": "Критическая температура...",
      "priority": 1,
      "actions": ["Остановить оборудование", "Заменить подшипник"]
    }
  }
]
```

---

## CI/CD

```
Push to main
     │
     ▼
[GitHub Actions: build.yml]
├── dotnet build DSS_Blazor
├── dotnet build DSS_Avalonia
└── ✅ All builds pass
     │
     ▼
[GitHub Actions: deploy-pages.yml]
├── dotnet publish DSS_Blazor -c Release
└── Deploy to gh-pages branch
     │
     ▼
https://samets742-gif.github.io/Dss/
```
