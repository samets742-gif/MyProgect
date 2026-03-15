# СППР — Система поддержки принятия решений

> **Ситуационный подход к принятию решений** — интеллектуальная система, которая сравнивает текущую ситуацию с накопленными прецедентами и выдаёт ранжированные рекомендации.

[![GitHub Pages](https://img.shields.io/badge/Web_App-Live-brightgreen?logo=github)](https://samets742-gif.github.io/Dss/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://blazor.net/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.2-blue)](https://avaloniaui.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

---

## Демо

**Открыть в браузере (телефон / ПК):**
```
https://samets742-gif.github.io/Dss/
```

---

## О проекте

СППР реализует **ситуационный (прецедентный) подход** — одну из классических парадигм ИИ. Система:

1. Хранит **базу знаний** из эталонных ситуаций с готовыми решениями
2. Принимает описание **текущей ситуации** (набор числовых параметров)
3. Находит **наиболее похожие прецеденты** с помощью взвешенного евклидова расстояния
4. Выдаёт **ранжированные рекомендации** с уровнем уверенности
5. Позволяет **обучаться** — добавлять новые прецеденты в базу знаний

### Область применения

| Домен | Примеры задач |
|-------|--------------|
| Диагностика оборудования | Определение причин неисправности по показаниям датчиков |
| Медицина | Постановка предварительного диагноза по симптомам |
| Производство | Управление качеством, выявление отклонений |
| Бизнес-аналитика | Классификация рыночных ситуаций |
| Любой домен | Настраивается через базу знаний |

---

## Как это работает

```
Текущая ситуация          База знаний
(набор параметров)        (эталонные прецеденты)
       │                         │
       └──────────┬──────────────┘
                  ▼
         SituationMatcher
    (взвешенное евклидово расстояние)
                  │
                  ▼
         DecisionEngine
    (фильтрация + ранжирование)
                  │
                  ▼
     Список рекомендаций
     с коэффициентом сходства [0..1]
```

**Формула сходства:**
```
similarity = 1 − √( Σ wᵢ · ((aᵢ − bᵢ) / rangeᵢ)² / Σ wᵢ )
```
где `wᵢ` — вес параметра, `aᵢ/bᵢ` — значения, `rangeᵢ` — диапазон нормализации.

---

## Версии приложения

| Версия | Технология | Платформа | Папка |
|--------|-----------|-----------|-------|
| **Web** ⭐ | Blazor WebAssembly | Любой браузер, телефон | `DSS_Blazor/` |
| Desktop | Avalonia UI | Linux / Windows / macOS | `DSS_Avalonia/` |
| Desktop (Windows) | WPF | Windows | `DSS_Situational/` |

---

## Быстрый старт

### Web (без установки)
```
https://samets742-gif.github.io/Dss/
```

### Локальный запуск — Blazor Web
```bash
git clone https://github.com/samets742-gif/Dss
cd Dss/DSS_Blazor
dotnet run
# Открыть: http://localhost:5000
```

### Локальный запуск — Avalonia (Linux/Windows/macOS)
```bash
cd Dss/DSS_Avalonia
dotnet run
```

### Сборка и публикация
```bash
# Web — статический сайт
cd DSS_Blazor
dotnet publish -c Release -o ./publish
# Содержимое publish/wwwroot/ — готовый сайт

# Desktop — self-contained
cd DSS_Avalonia
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

---

## Структура проекта

```
Dss/
├── .github/workflows/
│   ├── deploy-pages.yml   # Авто-деплой на GitHub Pages
│   └── build.yml          # CI сборка
├── DSS_Blazor/            # Web-приложение
│   ├── Models/            # Доменные модели
│   ├── Services/          # Бизнес-логика
│   ├── Pages/             # Razor-страницы
│   └── wwwroot/           # Статические файлы + начальная БЗ
├── DSS_Avalonia/          # Кроссплатформенный десктоп
├── DSS_Situational/       # Windows WPF-версия
├── docs/                  # Документация
├── README.md
├── CHANGELOG.md
└── LICENSE
```

---

## База знаний

Начальная база: **7 прецедентов**, 2 домена.

**Диагностика оборудования:**
- Перегрев подшипника, дисбаланс, утечка давления, перегрузка, норма

**Производственный процесс:**
- Брак продукции, низкая производительность

Базу расширяйте прямо в интерфейсе.

---

## Документация

- [Архитектура системы](architecture.md)
- [Описание алгоритма](algorithm.md)
- [Руководство пользователя](user-guide.md)
- [История изменений](../CHANGELOG.md)

---

## Лицензия

[MIT](../LICENSE) © 2026 samets742-gif
