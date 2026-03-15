# СППР — Система поддержки принятия решений

> Интеллектуальная система на основе **ситуационного (прецедентного) подхода**: сравнивает текущую ситуацию с накопленными прецедентами и выдаёт ранжированные рекомендации.

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
cd DSS_Blazor
dotnet run
# Открыть: http://localhost:5000
```

### Локальный запуск — Avalonia (Linux/Windows/macOS)
```bash
cd DSS_Avalonia
dotnet run
```

---

## Как работает алгоритм

Система использует **взвешенное нормализованное евклидово расстояние**:

```
similarity = 1 − √( Σ wᵢ · ((aᵢ − bᵢ) / rangeᵢ)² / Σ wᵢ )
```

где `wᵢ` — вес параметра, `aᵢ/bᵢ` — значения, `rangeᵢ` — диапазон нормализации.

Результат — коэффициент сходства от 0 до 1 (0% — полная несхожесть, 100% — идентичное совпадение).

---

## Структура проекта

```
.
├── .github/workflows/
│   ├── deploy-pages.yml   # Авто-деплой на GitHub Pages
│   └── build.yml          # CI сборка
├── DSS_Blazor/            # Web-приложение (Blazor WebAssembly)
│   ├── Models/
│   ├── Services/
│   ├── Pages/
│   └── wwwroot/
├── DSS_Avalonia/          # Кроссплатформенный десктоп (Avalonia)
├── DSS_Situational/       # Windows WPF-версия
├── docs/                  # Документация
│   ├── README_DSS.md
│   ├── architecture.md
│   ├── algorithm.md
│   └── user-guide.md
├── CHANGELOG.md
└── LICENSE
```

---

## Документация

- [Руководство пользователя](docs/user-guide.md)
- [Архитектура системы](docs/architecture.md)
- [Описание алгоритма](docs/algorithm.md)
- [История изменений](CHANGELOG.md)

---

## Лицензия

[MIT](LICENSE) © 2026 samets742-gif
