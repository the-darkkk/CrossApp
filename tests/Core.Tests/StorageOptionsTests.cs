using Core.Configuration;
using Xunit;

namespace Core.Tests;

public sealed class StorageOptionsTests
{
    [Fact]
    public void ResolveFullPath_WithRelativePath_ResolvesUnderBaseDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"cfg_test_{Guid.NewGuid():N}");
        try
        {
            var options = new StorageOptions
            {
                DataPath = "my_data",
                FileName = "catalog.json"
            };

            var fullPath = options.ResolveFullPath(tempDir);
            var expected = Path.GetFullPath(Path.Combine(tempDir, "my_data", "catalog.json"));

            Assert.Equal(expected, fullPath);
            Assert.True(Directory.Exists(Path.Combine(tempDir, "my_data")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ResolveFullPath_WithAbsolutePath_KeepsRootedPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"rooted_test_{Guid.NewGuid():N}");
        try
        {
            var options = new StorageOptions
            {
                DataPath = tempDir,
                FileName = "test.json"
            };

            var fullPath = options.ResolveFullPath("any_base_dir");
            var expected = Path.GetFullPath(Path.Combine(tempDir, "test.json"));

            Assert.Equal(expected, fullPath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}

