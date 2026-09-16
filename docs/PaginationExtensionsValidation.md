# PaginationExtensionsValidation

`PaginationExtensionsValidation` is a static extension class in the
`SignalRMapRealtime.Utilities` namespace. It validates a `PaginationInfo`
instance before the pagination state is used.

The class reports all validation problems it detects instead of stopping after
the first one. Its public API consists of `Validate`, `IsValid`, and
`EnsureValid`.

## Validate

```csharp
IReadOnlyList<string> Validate(this PaginationInfo value)
```

`Validate` returns a read-only list of human-readable problem descriptions. An
empty list means that the value passed every check. A null `value` causes an
`ArgumentNullException`.

Checks are performed in this order:

1. `PageNumber` must be at least `1`.
2. `PageSize` must be at least `1`.
3. `TotalCount` cannot be negative.
4. `TotalPages` cannot be negative.
5. `Skip` cannot be negative.
6. If `IsFirstPage` is `true`, `PageNumber` must equal `1`.
7. If `HasPreviousPage` is `true`, `PageNumber` must be greater than `1`.
8. If `HasNextPage` is `true`, `PageNumber` must be less than `TotalPages`.
9. `ItemsOnPage` cannot be negative.
10. `ItemsOnPage` cannot exceed `PageSize`.
11. `TotalCount` must be at least `ItemsOnPage + Skip`.
12. `TotalPages` must equal the value returned by
    `PaginationExtensions.CalculateTotalPages(TotalCount, PageSize)`.
13. `Skip` must equal the value returned by
    `PaginationExtensions.CalculateSkip(PageNumber, PageSize)`.

Each failed check adds a separate message to the result. Messages for numeric
checks include the relevant actual values; calculation checks include both the
stored and calculated values.

The boolean consistency checks are one-directional: a flag set to `true` is
checked against the numeric state. This validator does not add a problem merely
because one of those flags is `false` when the numeric state could support it.

## IsValid

```csharp
bool IsValid(this PaginationInfo value)
```

`IsValid` calls `Validate` and returns `true` when the returned list is empty;
otherwise, it returns `false`. A null `value` produces the same
`ArgumentNullException` as `Validate`.

## EnsureValid

```csharp
void EnsureValid(this PaginationInfo value)
```

`EnsureValid` returns normally when validation finds no problems. It throws:

- `ArgumentNullException` when `value` is null.
- `ArgumentException` when one or more validation checks fail.

For invalid state, the `ArgumentException` uses `value` as its parameter name.
Its message states the total number of problems, then lists every validation
message on a separate line prefixed with `- `.

## Example

```csharp
IReadOnlyList<string> problems = paginationInfo.Validate();

if (!paginationInfo.IsValid())
{
    // Inspect or report the problems returned by Validate.
}

paginationInfo.EnsureValid();
```

Use `Validate` when callers need all problem descriptions, `IsValid` for a
boolean result, and `EnsureValid` when invalid pagination state should stop the
current operation with an exception.
