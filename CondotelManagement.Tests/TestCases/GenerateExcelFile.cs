using OfficeOpenXml;
using System.Text;

namespace CondotelManagement.Tests.TestCases
{
    /// <summary>
    /// Utility class to generate Excel file from CSV
    /// Run this once to create Excel file
    /// </summary>
    public class ExcelGenerator
    {
        public static void GenerateExcelFromCsv(string csvPath, string excelPath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Test Cases");

            // Read CSV and parse
            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            if (lines.Length == 0) return;

            // Parse header
            var headers = ParseCsvLine(lines[0]);
            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = headers[col];
                worksheet.Cells[1, col + 1].Style.Font.Bold = true;
                worksheet.Cells[1, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Parse data rows
            for (int row = 1; row < lines.Length; row++)
            {
                var values = ParseCsvLine(lines[row]);
                for (int col = 0; col < values.Length && col < headers.Length; col++)
                {
                    worksheet.Cells[row + 1, col + 1].Value = values[col];
                }
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Save file
            var fileInfo = new FileInfo(excelPath);
            package.SaveAs(fileInfo);
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}














