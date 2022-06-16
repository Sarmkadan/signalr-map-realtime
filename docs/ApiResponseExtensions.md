# ApiResponseExtensions

Extension methods for working with the `ApiResponse<T>` type, providing common operations for checking response status, accessing messages, and ensuring successful responses.

## API

### `IsSuccessful(ApiResponse response)`
Determines whether the provided `ApiResponse` indicates a successful operation.

- **Parameters**
  - `response` (`ApiResponse`): The response to check.
- **Return Value**
  - `bool`: `true` if the response is successful; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `response` is `null`.

### `IsSuccessful<T>(ApiResponse<T> response)`
Determines whether the provided `ApiResponse<T>` indicates a successful operation.

- **Parameters**
  - `response` (`ApiResponse<T>`): The response to check.
- **Return Value**
  - `bool`: `true` if the response is successful; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `response` is `null`.

### `GetMessageOrDefault(ApiResponse response)`
Gets the message from the response if available; otherwise, returns an empty string.

- **Parameters**
  - `response` (`ApiResponse`): The response from which to retrieve the message.
- **Return Value**
  - `string`: The message contained in the response, or an empty string if no message is present.
- **Throws**
  - `ArgumentNullException`: If `response` is `null`.

### `GetMessageOrDefault<T>(ApiResponse<T> response)`
Gets the message from the response if available; otherwise, returns an empty string.

- **Parameters**
  - `response` (`ApiResponse<T>`): The response from which to retrieve the message.
- **Return Value**
  - `string`: The message contained in the response, or an empty string if no message is present.
- **Throws**
  - `ArgumentNullException`: If `response` is `null`.

### `EnsureSuccess(ApiResponse response)`
Throws an exception if the provided `ApiResponse` indicates a failed operation.

- **Parameters**
  - `response` (`ApiResponse`): The response to validate.
- **Throws**
  - `InvalidOperationException`: If the response is not successful.
  - `ArgumentNullException`: If `response` is `null`.

### `EnsureSuccess<T>(ApiResponse<T> response)`
Throws an exception if the provided `ApiResponse<T>` indicates a failed operation.

- **Parameters**
  - `response` (`ApiResponse<T>`): The response to validate.
- **Throws**
  - `InvalidOperationException`: If the response is not successful.
  - `ArgumentNullException`: If `response` is `null`.

### `WithData<T>(ApiResponse response, T data)`
Creates a new `ApiResponse<T>` with the specified data while preserving the original response's status and message.

- **Parameters**
  - `response` (`ApiResponse`): The original response to derive status and message from.
  - `data` (`T`): The data to include in the new response.
- **Return Value**
  - `ApiResponse<T>`: A new response containing the provided data and the original response's status and message.
- **Throws**
  - `ArgumentNullException`: If `response` is `null`.

## Usage
