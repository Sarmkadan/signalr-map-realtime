# VehicleControllerTests
The `VehicleControllerTests` class is designed to test the functionality of the `VehicleController` class, ensuring that it behaves as expected under various scenarios. This includes testing the retrieval of vehicles, creation of new vehicles, retrieval of vehicles by ID, updating of existing vehicles, deletion of vehicles, and error handling for invalid models and mismatched IDs.

## API
The `VehicleControllerTests` class contains the following public members:
* `public VehicleControllerTests`: The constructor for the `VehicleControllerTests` class.
* `public async Task GetVehicles_ReturnsSuccessAndCorrectContentType`: Tests that the `GetVehicles` method returns a successful response with the correct content type.
* `public async Task PostVehicle_ReturnsCreatedVehicle`: Tests that the `PostVehicle` method returns the created vehicle.
* `public async Task GetVehicleById_ReturnsNotFound_ForNonExistentId`: Tests that the `GetVehicleById` method returns a not found response for a non-existent ID.
* `public async Task GetVehicleById_ReturnsVehicle_ForExistingId`: Tests that the `GetVehicleById` method returns the vehicle for an existing ID.
* `public async Task PutVehicle_UpdatesExistingVehicle`: Tests that the `PutVehicle` method updates an existing vehicle.
* `public async Task DeleteVehicle_RemovesVehicle`: Tests that the `DeleteVehicle` method removes a vehicle.
* `public async Task PostVehicle_ReturnsBadRequest_ForInvalidModel`: Tests that the `PostVehicle` method returns a bad request response for an invalid model.
* `public async Task PutVehicle_ReturnsBadRequest_ForMismatchedId`: Tests that the `PutVehicle` method returns a bad request response for a mismatched ID.

## Usage
Here are two examples of using the `VehicleControllerTests` class:
```csharp
// Example 1: Testing the GetVehicles method
[TestMethod]
public async Task TestGetVehicles()
{
    // Arrange
    var vehicleControllerTests = new VehicleControllerTests();

    // Act
    await vehicleControllerTests.GetVehicles_ReturnsSuccessAndCorrectContentType();

    // Assert
    // Verify that the response is successful and has the correct content type
}

// Example 2: Testing the PostVehicle method
[TestMethod]
public async Task TestPostVehicle()
{
    // Arrange
    var vehicleControllerTests = new VehicleControllerTests();
    var vehicle = new Vehicle { Id = 1, Name = "Test Vehicle" };

    // Act
    await vehicleControllerTests.PostVehicle_ReturnsCreatedVehicle();

    // Assert
    // Verify that the created vehicle is returned
}
```

## Notes
The `VehicleControllerTests` class is designed to be thread-safe, as it uses asynchronous methods to test the `VehicleController` class. However, it is still important to ensure that the tests are run in isolation to avoid any potential conflicts. Additionally, the `PostVehicle_ReturnsBadRequest_ForInvalidModel` and `PutVehicle_ReturnsBadRequest_ForMismatchedId` tests may throw exceptions if the model is invalid or the ID is mismatched, respectively. It is also worth noting that the `GetVehicleById_ReturnsNotFound_ForNonExistentId` test will return a not found response if the ID does not exist, and the `DeleteVehicle_RemovesVehicle` test will remove the vehicle from the repository.
