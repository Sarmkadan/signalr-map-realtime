# ClusteringServiceTests

`ClusteringServiceTests` contains unit‑test methods that verify the behavior of the clustering and heatmap generation logic in the SignalR‑Map‑Realtime project. Each test exercises a specific scenario of the `ClusteringService` and asserts the expected outcome.

## API

### `public async Task GetClusters_WithEmptyLocationSet_ReturnsZeroClusters`
- **Purpose**: Confirms that when the clustering service receives an empty set of locations, it produces no clusters.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the test finishes; the test passes if the service returns zero clusters.
- **Throws**: May throw an exception (e.g., `Xunit.AssertException`) if the service returns a non‑zero number of clusters or if an unexpected error occurs during execution.

### `public async Task GetClusters_WithCollocatedPoints_CollapsesSameCellIntoOneCluster`
- **Purpose**: Verifies that points that fall into the same geographic cell are collapsed into a single cluster.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the test finishes; the test passes if the service returns exactly one cluster for the supplied collocated points.
- **Throws**: May throw an exception if the service returns more than one cluster or encounters an error.

### `public async Task GetHeatmap_WithMultiplePoints_NormalisesIntensityToOne`
- **Purpose**: Ensures that the heatmap generation normalises the intensity values so that the maximum intensity across all tiles equals 1 when multiple points are supplied.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the test finishes; the test passes if the normalised maximum intensity is 1 (within a tolerance).
- **Throws**: May throw an exception if the normalisation fails or if the service throws during heatmap creation.

### `public async Task GetHeatmap_WithNoPoints_ReturnsEmptyTilesAndZeroMaxCount`
- **Purpose**: Checks that when no points are provided, the heatmap service returns an empty tile collection and reports a maximum count of zero.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the test finishes; the test passes if the returned tile set is empty and the max count is zero.
- **Throws**: May throw an exception if tiles are returned or the max count is non‑zero.

## Usage

```csharp
using Xunit;
using SignalRMapRealtime.Tests; // namespace containing ClusteringServiceTests

public class TestRunner
{
    [Fact]
    public async Task RunEmptyLocationSetTest()
    {
        var testInstance = new ClusteringServiceTests();
        await testInstance.GetClusters_WithEmptyLocationSet_ReturnsZeroClusters();
    }

    [Fact]
    public async Task RunHeatmapNormalisationTest()
    {
        var testInstance = new ClusteringServiceTests();
        await testInstance.GetHeatmap_WithMultiplePoints_NormalisesIntensityToOne();
    }
}
```

The test class can also be executed directly with a test runner (e.g., `dotnet test` or Visual Studio Test Explorer), which will discover and run each `[Fact]` method.

## Notes

- The test methods are stateless; they do not rely on shared fields, making them safe to run in parallel with other tests.
- If the underlying `ClusteringService` depends on external resources (e.g., configuration files), those should be mocked or provided via test setup to ensure deterministic results.
- Edge cases covered by these tests include:
  - An empty input set producing no output.
  - Multiple inputs that map to the same spatial cell being collapsed.
  - Intensity normalisation across multiple points.
  - Absence of input yielding empty output and zero maxima.
- No thread‑safety guarantees are required for the test class itself; each method operates on its own local data and does not modify shared state. However, the production `ClusteringService` used by the tests should be thread‑safe if it is intended for concurrent calls.
