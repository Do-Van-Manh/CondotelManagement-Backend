using Xunit;

namespace CondotelManagement.Tests.TestCases
{
    /// <summary>
    /// Test to generate Test Report Document Excel file
    /// Run this test to create the Excel file
    /// </summary>
    public class GenerateTestReportTest
    {
        [Fact]
        public void GenerateTestReportExcel_ShouldCreateFile()
        {
            // Arrange
            var outputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TestCases",
                "TestReportDocument.xlsx"
            );

            // Ensure directory exists
            var directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // Act
            TestReportExcelGenerator.GenerateTestReportExcel(outputPath);

            // Assert
            Assert.True(File.Exists(outputPath), $"Excel file should be created at {outputPath}");
        }
    }
}














