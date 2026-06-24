Ниже представлено описание бакенд-приложения, которое было выбрано мной для выполнения ДЗ №3 к занятию №10 "Архитектурные дискуссии, документирование и кодогенерация". 
Код приложения был перенесён в новый git, поэтому истории предыдущих коммитов не имеется

# MasterApp Backend

MasterApp Backend - модульный монолит на ASP.NET Core Web API для работы мобильного мастера и диспетчера. Приложение объединяет авторизацию, основной бизнес-домен заявок и файловое хранилище в одном HTTP-сервисе.

## Стек

- .NET 9, ASP.NET Core Controllers
- PostgreSQL, EF Core, Npgsql
- JWT Bearer authentication, SSO для диспетчеров
- MinIO для хранения вложений
- Swagger/Swashbuckle для документации API
- Docker, GitLab CI, Kubernetes

## Структура проекта

Основная кодовая база находится в `src/`.

- `src/MasterApp.WebApi` - точка входа HTTP API и composition root.
- `src/MasterApp.Migrator` - консольное приложение для применения EF Core миграций.
- `src/Auth` - пользователи, роли, устройства, JWT, refresh tokens и SSO.
- `src/Master` - основной домен: организации, мастера, услуги, заявки, статусы и уведомления.
- `src/Files` - загрузка, хранение, получение ссылок и удаление файлов через MinIO.
- `deploy` - Kubernetes manifests для приложения и мигратора.
- `Dockerfile` и `Migrator/Dockerfile` - сборка образов Web API и мигратора.

## Конфигурация

Основные параметры задаются через `appsettings*.json` или переменные окружения:

- `ConnectionStrings:DefaultConnection` - подключение к PostgreSQL.
- `Jwt:*` - issuer, audience и ключ подписи токенов.
- `Minio:*` - endpoint, credentials, bucket и настройки ссылок.
- `Domokey:*`, `Sso:*` - настройки SSO-интеграции.

Для локального запуска не храните реальные секреты в репозитории. Используйте переменные окружения, user secrets или защищенное хранилище секретов.

## Локальный запуск

Восстановить зависимости и собрать решение:

```bash
dotnet restore src/MasterApp.sln
dotnet build src/MasterApp.sln
```

Запустить Web API:

```bash
dotnet run --project src/MasterApp.WebApi
```

В Development-окружении Swagger доступен через стандартный Swagger UI приложения.

## Миграции

Проект использует несколько EF Core контекстов:

- `AppDbContext` для основного домена.
- `AuthDbContext` для схемы Auth.
- `FileDbContext` для схемы Files.

Применить миграции можно через отдельный мигратор:

```bash
dotnet run --project src/MasterApp.Migrator --ConnectionStrings:DefaultConnection="<connection-string>"
```

Также мигратор поддерживает переменную окружения `ConnectionStrings__DefaultConnection`.

## Docker и деплой

Собрать образ Web API:

```bash
docker build -t masterapp-webapi -f Dockerfile .
```

Собрать образ мигратора:

```bash
docker build -t masterapp-migrator -f Migrator/Dockerfile .
```

В GitLab CI сначала собирается и запускается Kubernetes job мигратора, затем собирается и обновляется deployment Web API.

## Полезные замечания

- Решение находится в `src/MasterApp.sln`.
- Основные API-контроллеры находятся в `src/MasterApp.WebApi/Controllers`.
- Подробное описание архитектуры проекта лежит в `architecture.md`.
- На момент написания README тестовые проекты в репозитории не обнаружены.
