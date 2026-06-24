# Memory Bank (index)

Полная документация перенесена в **[`memory-bank/`](../memory-bank/README.md)**.

## Быстрый обзор

**MasterApp** — .NET 9 Web API backend маркетплейса услуг МКД: мастера (mobile), диспетчеры (операторы УК), файлы (MinIO), интеграция Domokey (SSO).

| Модуль | Путь | Назначение |
|--------|------|------------|
| Auth | `src/Auth/` | Users, JWT, refresh, SSO |
| Master | `src/Master/` | Organizations, Services, ServiceQuests, notifications |
| Files | `src/Files/` | CQRS + MinIO + `IFileContract` |
| WebApi | `src/MasterApp.WebApi/` | HTTP entry point |

## Документы Memory Bank

- [projectbrief.md](../memory-bank/projectbrief.md) — цели и ограничения
- [productContext.md](../memory-bank/productContext.md) — сценарии и домен
- [systemPatterns.md](../memory-bank/systemPatterns.md) — архитектура
- [techContext.md](../memory-bank/techContext.md) — стек, API, деплой
- [activeContext.md](../memory-bank/activeContext.md) — текущий фокус
- [progress.md](../memory-bank/progress.md) — статус реализации

## Запуск

```bash
cd src
dotnet run --project MasterApp.WebApi
```

Решение: `src/MasterApp.sln`
