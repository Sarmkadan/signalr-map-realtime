# IClusteringService
The `IClusteringService` interface provides a set of methods for clustering and heatmap generation, enabling the efficient grouping of data points and visualization of density patterns. This interface is designed to be used in applications that require real-time mapping and data analysis, such as geographic information systems or location-based services.

## API
### ClusteringService
The `ClusteringService` property provides an instance of the `ClusteringService` class, which implements the `IClusteringService` interface.

### GetClustersAsync
The `GetClustersAsync` method retrieves clusters of data points asynchronously. It returns a `ClusterResponseDto` object, which contains the clustered data. This method may throw exceptions if the input data is invalid or if an error occurs during the clustering process.

### GetHeatmapAsync
The `GetHeatmapAsync` method generates a heatmap asynchronously, representing the density of data points. It returns a `HeatmapResponseDto` object, which contains the heatmap data. This method may throw exceptions if the input data is invalid or if an error occurs during the heatmap generation process.

### AddClustering
The `AddClustering` method is a static method that adds clustering services to an `IServiceCollection`. This method is used to register the clustering services with the application's dependency injection container.

## Usage
The following examples demonstrate how to use the `IClusteringService` interface:
```csharp
// Example 1: Using the GetClustersAsync method
var clusteringService = new ClusteringService();
var clusters = await clusteringService.GetClustersAsync();
foreach (var cluster in clusters.Clusters)
{
    Console.WriteLine($"Cluster {cluster.Id} has {cluster.Points.Count} points");
}

// Example 2: Using the GetHeatmapAsync method
var heatmap = await clusteringService.GetHeatmapAsync();
foreach (var cell in heatmap.Cells)
{
    Console.WriteLine($"Cell at ({cell.X}, {cell.Y}) has a density of {cell.Density}");
}
```

## Notes
The `IClusteringService` interface is designed to be thread-safe, allowing multiple concurrent requests to be processed simultaneously. However, the implementation of the `ClusteringService` class may have specific requirements or limitations, such as the need for synchronization or the use of shared resources. Additionally, the `GetClustersAsync` and `GetHeatmapAsync` methods may throw exceptions if the input data is invalid or if an error occurs during the clustering or heatmap generation process. It is recommended to handle these exceptions and provide appropriate error handling mechanisms in the application.
