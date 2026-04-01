using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.HomeServices
{
    public class HomeService : IHomeService
    {
        public Task<byte[]> GenerateQrCodeAsync(string qrText)
        {
            qrText ??= string.Empty;

            using var qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            var pngQrCode = new PngByteQRCode(qrCodeData);
            byte[] imageBytes = pngQrCode.GetGraphic(20);

            return Task.FromResult(imageBytes);
        }

        public Task<byte[]> GenerateEmergencyListPdfAsync(List<emergencyList> emergencyLists)
        {
            using var memoryStream = new MemoryStream();
            using var document = new Document(PageSize.A4, 25, 25, 25, 15);

            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            for (int i = 0; i < emergencyLists.Count; i++)
            {
                var data = emergencyLists[i];

                var headingFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                document.Add(new Paragraph($"Location: {data.assemblyPoint?.ToUpperInvariant()}", headingFont));
                document.Add(new Paragraph($"Printed On: {data.printedOn}", normalFont));
                document.Add(new Paragraph(" "));

                var table = new PdfPTable(6)
                {
                    WidthPercentage = 100,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    SpacingBefore = 10f,
                    SpacingAfter = 20f
                };

                var titleCell = new PdfPCell(new Phrase("EMERGENCY LIST - People in the building", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)))
                {
                    Colspan = 6,
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                table.AddCell(titleCell);

                AddHeaderCell(table, "Employee Code");
                AddHeaderCell(table, "Surname");
                AddHeaderCell(table, "First Name");
                AddHeaderCell(table, "Mobile Number");
                AddHeaderCell(table, "Date");
                AddHeaderCell(table, "Remarks");

                foreach (var item in data.emergencyData)
                {
                    table.AddCell(item.email ?? string.Empty);
                    table.AddCell(item.surname ?? string.Empty);
                    table.AddCell(item.firstname ?? string.Empty);
                    table.AddCell(item.mobileNumber ?? string.Empty);
                    table.AddCell(item.punchDateTime.ToString());
                    table.AddCell(string.Empty);
                }

                document.Add(table);

                if (i < emergencyLists.Count - 1)
                    document.NewPage();
            }

            document.Close();
            return Task.FromResult(memoryStream.ToArray());
        }

        public Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage)
        {
            DateTime now = DateTime.Now;

            appexception newException = new appexception
            {
                source = source,
                message = message,
                originatedAt = originatedAt,
                stacktrace = stackTrace,
                innerexceptionmessage = innerExceptionMessage,
                createdOn = now,
                createdBy = "system",
                updatedOn = now,
                updatedBy = "system"
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            db.Insert(newException);

            return Task.CompletedTask;
        }

        private static void AddHeaderCell(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)))
            {
                BackgroundColor = BaseColor.PINK,
                Padding = 5
            };

            table.AddCell(cell);
        }
    }
}