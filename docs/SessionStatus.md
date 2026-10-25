# SessionStatus

Source: `src/SignalRMapRealtime/Domain/Enums/SessionStatus.cs`

The file lives in the `SignalRMapRealtime.Domain.Enums` namespace and defines one public type:

- `SessionStatus` - a public enum for the state of a tracking session. Its summary says it "represents the state of a tracking session."

The enum has no attributes and no methods. Its underlying type is the default `int`. Each value is set explicitly, numbered `0` to `5` in declaration order.

## Values

| Name | Value | Meaning (from the XML doc comment) |
|------|-------|------------------------------------|
| `Pending` | `0` | Session has been initialized but not started. |
| `Active` | `1` | Session is currently active and tracking. |
| `Paused` | `2` | Session has been paused temporarily. |
| `Completed` | `3` | Session has been completed normally. |
| `Cancelled` | `4` | Session was terminated abnormally. |
| `Expired` | `5` | Session has expired due to timeout. |

## Notes on the values

- The values are stored as their integer numbers. Any code or stored data that uses the numbers depends on this order. Changing a number or reordering values changes what it means.
- `Pending` is the state before a session starts. `Active` is the state while it tracks. `Paused` is a temporary pause from `Active`.
- `Completed`, `Cancelled`, and `Expired` are the end states described in the comments. `Completed` means the session finished normally. `Cancelled` means it ended abnormally. `Expired` means it ended because of a timeout.
- The comments do not say which transitions are allowed, for example whether a `Paused` session can return to `Active` or whether an ended session can be restarted.
- The comments do not say which of the end states are final. They also do not say whether `Cancelled` and `Expired` are both abnormal endings, although `Cancelled` is the only one described as abnormal.
