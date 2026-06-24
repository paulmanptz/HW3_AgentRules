# План: Добавление GET-метода для получения id заявки по id обращения

## Задача
Добавить в контроллер `DispatcherQuestsController` GET-метод для получения id заявки для переданного id обращения. Искать в таблице `ServiceQuests` по колонке `relatedTicketId` и возвращать `id` записи, если значение найдено.

## Требования
- Маршрут: `GET /api/dispatcher/quests/by-ticket/{ticketId}`
- Учитывать организацию диспетчера (OrgId) из claims пользователя
- Возвращать 404, если заявка не найдена
- Возвращать только id заявки (объект с полем `QuestId`)

## Изменяемые файлы

### 1. `Master/MasterApp.Application/Interfaces/IDispatcherQuestsService.cs`
Добавить DTO для ответа и новый метод в интерфейс.

```csharp
public class QuestIdByTicketResponse
{
    public Guid QuestId { get; set; }
}

public interface IDispatcherQuestsService
{
    // существующие методы...
    Task<QuestIdByTicketResponse> GetQuestIdByTicketIdAsync(int orgId, Guid ticketId, CancellationToken cancellationToken);
}
```

### 2. `Master/MasterApp.Application/Services/DispatcherQuestsService.cs`
Реализовать метод `GetQuestIdByTicketIdAsync`.

```csharp
public async Task<QuestIdByTicketResponse> GetQuestIdByTicketIdAsync(int orgId, Guid ticketId, CancellationToken cancellationToken)
{
    var quest = await _context.ServiceQuests
        .FirstOrDefaultAsync(q => q.OrgId == orgId && q.RelatedTicketId == ticketId, cancellationToken);

    if (quest == null)
        throw new Exception("Заявка с указанным id обращения не найдена.");

    return new QuestIdByTicketResponse { QuestId = quest.Id };
}
```

### 3. `MasterApp.WebApi/Controllers/DispatcherQuestsController.cs`
Добавить новый endpoint.

```csharp
[HttpGet("by-ticket/{ticketId}")]
public async Task<IActionResult> GetQuestIdByTicketId(Guid ticketId, CancellationToken cancellationToken)
{
    try
    {
        var orgId = GetOrgId();
        var result = await _requestsService.GetQuestIdByTicketIdAsync(orgId, ticketId, cancellationToken);
        return Ok(result);
    }
    catch (Exception ex)
    {
        return NotFound(new { message = ex.Message });
    }
}
```

## Последовательность выполнения
1. Добавить DTO и метод в интерфейс `IDispatcherQuestsService`
2. Реализовать метод в `DispatcherQuestsService`
3. Добавить endpoint в контроллер `DispatcherQuestsController`
4. При необходимости обновить зависимости (не требуется, так как метод просто использует существующий DbContext)

## Проверка
- Убедиться, что миграция `20260508064816_AddRelatedTicketIdToServiceQuest` применена (поле `RelatedTicketId` существует в БД)
- Протестировать endpoint через Swagger или Postman

## Примечания
- Метод должен быть доступен только для роли `Dispatcher` (уже есть атрибут `[Authorize(Roles = "Dispatcher")]` на контроллере)
- Исключение обрабатывается и возвращается 404 Not Found
- Возвращаемый JSON: `{ "questId": "guid" }`