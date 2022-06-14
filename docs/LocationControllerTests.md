# LocationControllerTests

`LocationControllerTests` is a test fixture class in the `signalr-map-realtime` project that validates the behavior of the `LocationController` API endpoints. It ensures correct handling of HTTP requests for location data, including retrieval, creation, update, deletion, and validation of input models.

## API

### `LocationControllerTests()`
Initializes a new instance of the `LocationControllerTests` class, typically setting up mock dependencies and test data required for the controller tests.

### `GetLocations_ReturnsSuccessAndCorrectContentType()`
Verifies that the `GetLocations` endpoint returns a successful HTTP response with the correct content type.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws an exception if the response status code is not 200 OK or the content type is not `application/json`.

### `PostLocation_ReturnsCreatedLocation()`
Confirms that the `PostLocation` endpoint successfully creates a location and returns the created entity with a 201 Created status.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 201 Created or the returned location does not match the input model.

### `GetLocationById_ReturnsNotFound_ForNonExistentId()`
Tests that the `GetLocationById` endpoint returns a 404 Not Found response when queried with a non-existent location ID.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 404 Not Found.

### `GetLocationById_ReturnsLocation_ForExistingId()`
Validates that the `GetLocationById` endpoint returns the correct location data for an existing ID with a 200 OK status.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 200 OK or the returned location does not match the expected data.

### `PutLocation_UpdatesExistingLocation()`
Ensures that the `PutLocation` endpoint updates an existing location and returns a 204 No Content response.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 204 No Content or the location is not updated in the repository.

### `DeleteLocation_RemovesLocation()`
Confirms that the `DeleteLocation` endpoint removes a location and returns a 204 No Content response.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 204 No Content or the location is not removed from the repository.

### `PostLocation_ReturnsBadRequest_ForInvalidModel()`
Tests that the `PostLocation` endpoint returns a 400 Bad Request response when provided with an invalid location model.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 400 Bad Request.

### `PutLocation_ReturnsBadRequest_ForMismatchedId()`
Verifies that the `PutLocation` endpoint returns a 400 Bad Request response when the route ID does not match the model ID.  
**Parameters**: None  
**Return Value**: `Task` (void)  
**Exceptions**: Throws if the response status code is not 400 Bad Request.

## Usage

```csharp
// Example 1: Testing successful location retrieval
[Fact]
public async Task GetLocations_ReturnsSuccessAndCorrectContentType()
{
    // Arrange
    var mockRepo = new Mock<ILocationRepository>();
    mockRepo.Setup(repo => repo.GetAll()).Returns(GetTestLocations());
    var controller = new LocationController(mockRepo.Object);

    // Act
    var response = await controller.GetLocations();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(response);
    Assert.Equal("application/json", response.ContentType);
}
```

```csharp
// Example 2: Testing invalid model during location creation
[Fact]
public async Task PostLocation_ReturnsBadRequest_ForInvalidModel()
{
    // Arrange
    var mockRepo = new Mock<ILocationRepository>();
    var controller = new LocationController(mockRepo.Object)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        }
    };
    controller.ModelState.AddModelError("Name", "Required");

    var invalidLocation = new Location { Id = 1, Name = "" };

    // Act
    var response = await controller.PostLocation(invalidLocation);

    // Assert
    Assert.IsType<BadRequestObjectResult>(response);
}
```

## Notes

- All test methods execute in isolation and do not require thread-safety considerations, as they operate on mocked dependencies and in-memory test data.
- Edge cases such as invalid models and mismatched IDs are explicitly tested to ensure the controller adheres to RESTful conventions and input validation rules.
- The tests assume that the `LocationController` properly delegates data operations to its injected `ILocationRepository` dependency, which is mocked during test execution.
