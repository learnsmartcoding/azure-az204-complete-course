using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.IO;

namespace AZ204_Functions_Demo
{
    public class BlogTriggerFunction
    {
        [FunctionName("BlogTriggerFunction")]
        //This gets blob and its name to process whenever we upload a file to container
        public void Run([BlobTrigger("%ContainerName%", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            // Get the file extension
            string extension = Path.GetExtension(name);

            // Check if the file is an Excel file
            if (extension == ".xls" || extension == ".xlsx")
            {
                // Process the Excel file
                using (var package = new ExcelPackage(myBlob))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;
                    var columnCount = worksheet.Dimension.Columns;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var phoneNumber = worksheet.Cells[row, 1].Value?.ToString();
                        if (phoneNumber!=null && phoneNumber.Length == 10)
                        {
                            var firstName = worksheet.Cells[row, 2].Value?.ToString();
                            var lastName = worksheet.Cells[row, 3].Value?.ToString();
                            var address = worksheet.Cells[row, 4].Value?.ToString();
                            var groupName = worksheet.Cells[row, 5].Value?.ToString();

                            var model = new MyModel
                            {
                                PhoneNumber = phoneNumber,
                                FirstName = firstName,
                                LastName = lastName,
                                Address = address,
                                GroupName = groupName
                            };

                            log.LogInformation($"Processed row {row - 1}: {model}");
                        }
                    }
                }
            }
            else
            {
                log.LogInformation($"Ignoring blob {name} because it is not an Excel file.");
            }

           
        }

        public class MyModel
        {
            public string PhoneNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string GroupName { get; set; }

            public override string ToString()
            {
                return $"{{PhoneNumber={PhoneNumber},FirstName={FirstName}, LastName={LastName}, Address={Address}, GroupName={GroupName}}}";
            }
        }
    }
}
