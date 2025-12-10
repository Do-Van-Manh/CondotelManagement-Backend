namespace CondotelManagement.Tests.TestCases
{
    /// <summary>
    /// Program to generate Test Report Document Excel file
    /// Run this program to create the Excel file
    /// </summary>
    public class GenerateTestReportProgram
    {
        public static void Main(string[] args)
        {
            var outputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TestCases",
                "TestReportDocument.xlsx"
            );

            Console.WriteLine("Generating Test Report Document Excel file...");
            TestReportExcelGenerator.GenerateTestReportExcel(outputPath);
            Console.WriteLine($"Excel file created successfully at: {outputPath}");
        }
    }
}














