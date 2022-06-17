# AssetControllerExtensions

Provides static extension methods for ASP.NET Core controllers that encapsulate common asset‑query operations used by the SignalR‑based real‑time map. Each method returns an `IActionResult` suitable for direct use in controller actions, keeping controller code concise and focused on concerns such as routing and model binding.

## API

### GetAssetsByType
**Purpose** – Returns all assets that match a specified type identifier.  
**Parameters**  
- `assetType` (string): The type to filter by; must not be null or whitespace.  
- `cancellationToken` (CancellationToken, optional): Propagates notification that operations should be canceled.  
**Return Value** – `Task<IActionResult>` yielding:  
- `200 OK` with a JSON array of matching assets when any are found.  
- `404 NotFound` when no assets match the given type.  
**Exceptions** –  
- `ArgumentNullException` if `assetType` is null.  
- `ArgumentException` if `assetType` consists only of whitespace.  
- `InvalidOperationException` if the underlying asset repository cannot be accessed.

### GetAssetsByCondition
**Purpose** – Returns assets that satisfy an arbitrary predicate expression.  
**Parameters**  
- `predicate` (Expression<Func<Asset, bool>>): The condition to apply; must not be null.  
- `cancellationToken` (CancellationToken, optional): Propagates cancellation requests.  
**Return Value** – `Task<IActionResult>` yielding:  
- `200 OK` with a JSON array of assets that meet the condition.  
- `204 NoContent` when the condition yields no results.  
**Exceptions** –  
- `ArgumentNullException` if `predicate` is null.  
- `InvalidOperationException` if the data source throws while evaluating the expression.

### GetAssetStatistics
**Purpose** – Returns aggregate statistics (count, average value, min/max) for the asset set, optionally filtered by type.  
**Parameters**  
- `assetType` (string, optional): If supplied, limits statistics to assets of this type; null or whitespace means all types.  
- `cancellationToken` (CancellationToken, optional): Propagates cancellation requests.  
**Return Value** – `Task<IActionResult>` yielding:  
- `200 OK` with an object containing `Count`, `AverageValue`, `MinValue`, `MaxValue`.  
- `204 NoContent` when no assets exist for the requested filter.  
**Exceptions** –  
- `ArgumentException` if `assetType` is whitespace‑only.  
- `InvalidOperationException` if the repository cannot compute the aggregates.

### GetAssetsByDateRange
**Purpose** – Returns assets whose timestamp falls within a supplied inclusive date range.  
**Parameters**  
- `startDate` (DateTimeOffset): The lower bound of the range; must be earlier than or equal to `endDate`.  
- `endDate` (DateTimeOffset): The upper bound of the range; must be later than or equal to `startDate`.  
- `cancellationToken` (CancellationToken, optional): Propagates cancellation requests.  
**Return Value** – `Task<IActionResult>` yielding:  
- `200 OK` with a JSON array of assets ordered by timestamp ascending.  
- `204 NoContent` when no assets fall within the range.  
**Exceptions** –  
- `ArgumentOutOfRangeException` if `startDate` is after `endDate`.  
- `InvalidOperationException` if the underlying store cannot query by date.

### GetAssetsSortedByDate
**Purpose** – Returns all assets sorted by their timestamp, either ascending or descending.  
**Parameters**  
- `descending` (bool): When true, results are sorted newest‑first; otherwise oldest‑first.  
- `cancellationToken` (CancellationToken, optional): Propagates cancellation requests.  
**Return Value** – `Task<IActionResult>` yielding:  
- `200 OK` with a JSON array of assets sorted as requested.  
- `204 NoContent` when the asset collection is empty.  
**Exceptions** –  
- `InvalidOperationException` if the repository cannot order the results.

## Usage

```csharp
using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.Extensions; // namespace containing AssetControllerExtensions

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    // GET api/assets/type/{type}
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(string type, CancellationToken ct)
    {
        // The controller forwards the request to the extension method.
        return await this.GetAssetsByType(type, ct);
    }

    // GET api/assets/statistics?type=electric
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? type, CancellationToken ct)
    {
        return await this.GetAssetStatistics(type ?? string.Empty, ct);
    }
}
```

```csharp
using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.Extensions;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    // GET api/report/range?start=2024-01-01&end=2024-01-31
    [HttpGet("range")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        CancellationToken ct)
    {
        // Validate before calling the extension to provide a clear 400 response.
        if (start > end)
            return BadRequest("Start date must not be later than end date.");

        return await this.GetAssetsByDateRange(start, end, ct);
    }
}
```

## Notes
- The extension methods are stateless; they rely on injected services accessed via the controller’s `HttpContext`. Consequently, they are safe to invoke concurrently from multiple requests, provided the underlying services are thread‑safe.  
- When a method returns `204 NoContent`, the response body is empty; clients should treat this as a successful query with no matching data.  
- Date‑range parameters are interpreted as UTC offsets; ensure that client‑side values are correctly converted to `DateTimeOffset` to avoid unexpected filtering.  
- Large result sets are not automatically paginated; callers should consider applying `Skip`/`Take` or using the returned data for further aggregation if payload size is a concern.  
- If the underlying asset repository throws an exception, the extension methods propagate it as an `InvalidOperationException`; controllers may catch and translate these into appropriate HTTP 500 responses when needed.
