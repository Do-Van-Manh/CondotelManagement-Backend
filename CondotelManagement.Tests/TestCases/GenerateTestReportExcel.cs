using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace CondotelManagement.Tests.TestCases
{
    /// <summary>
    /// Utility class to generate Test Report Document Excel file
    /// </summary>
    public class TestReportExcelGenerator
    {
        public static void GenerateTestReportExcel(string excelPath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Test Report");

            int currentRow = 1;

            // Title
            worksheet.Cells[currentRow, 1, currentRow, 6].Merge = true;
            worksheet.Cells[currentRow, 1].Value = "TEST REPORT DOCUMENT";
            worksheet.Cells[currentRow, 1].Style.Font.Size = 20;
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            currentRow += 2;

            // Project Information Section
            worksheet.Cells[currentRow, 1].Value = "Project Information";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
            currentRow++;

            // Project Information Table Header
            worksheet.Cells[currentRow, 1].Value = "Label";
            worksheet.Cells[currentRow, 2].Value = "Value";
            worksheet.Cells[currentRow, 1, currentRow, 2].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1, currentRow, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[currentRow, 1, currentRow, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[currentRow, 1, currentRow, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            currentRow++;

            // Project Information Data
            var projectInfo = new Dictionary<string, string>
            {
                { "Project Name", "Condotel Management System" },
                { "Project Code", "CMS" },
                { "Document Code", "CMS_TRD_v1.0" },
                { "Creator", "" },
                { "Issue Date", "<Date when this test report is created>" },
                { "Version", "1.0" }
            };

            foreach (var item in projectInfo)
            {
                worksheet.Cells[currentRow, 1].Value = item.Key;
                worksheet.Cells[currentRow, 2].Value = item.Value;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1, currentRow, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                currentRow++;
            }

            currentRow += 2;

            // Record of Change Section
            worksheet.Cells[currentRow, 1].Value = "Record of Change";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
            currentRow++;

            // Record of Change Table Header
            var headers = new[] { "Effective Date", "Version", "Change Item", "*A,D,M", "Change description", "Reference" };
            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cells[currentRow, col + 1].Value = headers[col];
                worksheet.Cells[currentRow, col + 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, col + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, col + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 51, 102)); // Dark blue
                worksheet.Cells[currentRow, col + 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells[currentRow, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;

            // Function records from TestCaseManagement5.csv
            var functions = new[]
            {
                new { Item = "Add register function", Description = "Added register function for user account registration" },
                new { Item = "Add verify email function", Description = "Added verify email function with OTP code verification" },
                new { Item = "Add login function", Description = "Added login function for user authentication" },
                new { Item = "Add get current user function", Description = "Added get current user function to retrieve logged in user profile" },
                new { Item = "Add reset password function", Description = "Added reset password function using OTP code" },
                new { Item = "Add user management function", Description = "Added user management function for admin to manage user accounts and status" },
                new { Item = "Add booking function", Description = "Added booking function to manage booking operations for condotels" },
                new { Item = "Add payment function", Description = "Added payment function to create payment links for bookings" },
                new { Item = "Add review function", Description = "Added review function to manage reviews for completed bookings" },
                new { Item = "Add condotel function", Description = "Added condotel function to manage condotel CRUD operations and browsing" },
                new { Item = "Add voucher function", Description = "Added voucher function to manage voucher creation and viewing" },
                new { Item = "Add admin dashboard function", Description = "Added admin dashboard function to retrieve dashboard statistics and analytics" },
                new { Item = "Add profile function", Description = "Added profile function to manage user profile information" },
                new { Item = "Add reward points function", Description = "Added reward points function to manage reward points and redemption" },
                new { Item = "Add chat function", Description = "Added chat function to manage conversations and messages" },
                new { Item = "Add blog function", Description = "Added blog function to retrieve and manage blog posts and categories" },
                new { Item = "Add promotion function", Description = "Added promotion function to manage promotions for condotels" },
                new { Item = "Add service package function", Description = "Added service package function to manage service packages for hosts" },
                new { Item = "Add location function", Description = "Added location function to manage location CRUD operations" },
                new { Item = "Add resort function", Description = "Added resort function to manage resort CRUD operations" },
                new { Item = "Add utility function", Description = "Added utility function to manage utility CRUD operations" },
                new { Item = "Add host package function", Description = "Added host package function to manage host package subscriptions" },
                new { Item = "Add upload function", Description = "Added upload function to upload image files to Cloudinary" },
                new { Item = "Add customer function", Description = "Added customer function to retrieve customer information" },
                new { Item = "Add report function", Description = "Added report function to retrieve revenue and booking reports" }
            };

            foreach (var func in functions)
            {
                worksheet.Cells[currentRow, 1].Value = "2025-01-XX";
                worksheet.Cells[currentRow, 2].Value = "1.0";
                worksheet.Cells[currentRow, 3].Value = func.Item;
                worksheet.Cells[currentRow, 4].Value = "A";
                worksheet.Cells[currentRow, 5].Value = func.Description;
                worksheet.Cells[currentRow, 6].Value = "TestCaseManagement5.csv";
                for (int col = 1; col <= 6; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Empty rows for future entries
            for (int i = 0; i < 10; i++)
            {
                for (int col = 1; col <= 6; col++)
                {
                    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                currentRow++;
            }

            // Note
            currentRow++;
            worksheet.Cells[currentRow, 1].Value = "Note: *A,D,M: A = Added, D = Deleted, M = Modified";
            worksheet.Cells[currentRow, 1].Style.Font.Italic = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 10;

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Set column widths
            worksheet.Column(1).Width = 18;
            worksheet.Column(2).Width = 10;
            worksheet.Column(3).Width = 30;
            worksheet.Column(4).Width = 12;
            worksheet.Column(5).Width = 40;
            worksheet.Column(6).Width = 15;

            // Save file
            var fileInfo = new FileInfo(excelPath);
            package.SaveAs(fileInfo);
        }
    }
}

