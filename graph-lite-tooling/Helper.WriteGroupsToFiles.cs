partial class Helper
{
    public static int WriteGroupsToFiles(Dictionary<string, List<string>> groupedOperationIds, string outputDirectory)
    {
        // Ensure output directory exists
        Directory.CreateDirectory(outputDirectory);

        // Variable to keep track of the total count of operation IDs
        int totalOperationCount = 0;

        foreach (var group in groupedOperationIds)
        {
            var fileName = $"{group.Key}.txt";  // Change file extension if needed
            var filePath = Path.Combine(outputDirectory, fileName);

            using (var streamWriter = new StreamWriter(filePath))
            {
                foreach (var operationId in group.Value)
                {
                    streamWriter.WriteLine(operationId);
                }
            }

            totalOperationCount += group.Value.Count;

            Console.WriteLine($"Written {group.Value.Count} operation IDs to file '{filePath}'.");
        }

         // Return the total count of operation IDs
        return totalOperationCount;
    }
}