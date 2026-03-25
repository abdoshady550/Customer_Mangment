using Customer_Mangment.CQRS.Customers.Queries.Report.Dtos;
using Customer_Mangment.Repository.Interfaces.Report;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Customer_Mangment.Repository.Services.Reports
{
    public sealed class CustomerReportBuilder : ICustomerReportBuilder
    {
        //colours
        private static readonly string HeaderBg = "#2E86AB";
        private static readonly string HeaderFg = "#FFFFFF";
        private static readonly string AltRowBg = "#F2F8FB";
        private static readonly string BorderColor = "#CCCCCC";
        private static readonly string TextDark = "#1A1A1A";

        public byte[] Build(
            IReadOnlyList<CustomerReportRow> rows,
            DateTime? from,
            DateTime? to)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    //    page setup 
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(36, Unit.Point);
                    page.DefaultTextStyle(t => t.FontFamily("Liberation Sans").FontSize(9));

                    //    header  
                    page.Header().Column(col =>
                    {
                        col.Item()
                           .AlignCenter()
                           .Text(BuildTitle(from, to))
                           .FontSize(14).Bold().FontColor(TextDark);

                        col.Item()
                           .AlignCenter()
                           .Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
                           .FontSize(9).FontColor("#555555");

                        col.Item().Height(8);
                    });

                    //    body (table)                                           
                    page.Content().Table(table =>
                    {
                        // column widths (relative units)
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);   // Customer ID
                            cols.RelativeColumn(2);   // Name
                            cols.RelativeColumn(2);   // Mobile
                            cols.RelativeColumn(2);   // Created By
                            cols.RelativeColumn(2);   // Updated By
                            cols.RelativeColumn(1);   // Is Deleted
                        });

                        //    header row                                         
                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(HeaderBg)
                             .Padding(6)
                             .AlignCenter();

                        table.Header(header =>
                        {
                            foreach (var label in new[]
                                { "Customer ID", "Name", "Mobile",
                              "Created By", "Updated By", "Is Deleted" })
                            {
                                header.Cell().Element(HeaderCell)
                                      .Text(label)
                                      .Bold().FontColor(HeaderFg).FontSize(8);
                            }
                        });

                        //    data rows                                         ─
                        for (int i = 0; i < rows.Count; i++)
                        {
                            var row = rows[i];
                            var bg = (i % 2 == 1) ? AltRowBg : "#FFFFFF";

                            IContainer DataCell(IContainer c) =>
                                c.Background(bg)
                                 .BorderBottom(0.5f).BorderColor(BorderColor)
                                 .PaddingVertical(4).PaddingHorizontal(6);

                            table.Cell().Element(DataCell).Text(row.Id.ToString())
                                 .FontSize(8).FontColor(TextDark);
                            table.Cell().Element(DataCell).Text(row.Name ?? "")
                                 .FontSize(8).FontColor(TextDark);
                            table.Cell().Element(DataCell).Text(row.Mobile ?? "")
                                 .FontSize(8).FontColor(TextDark);
                            table.Cell().Element(DataCell).Text(row.CreatedBy ?? "")
                                 .FontSize(8).FontColor(TextDark);
                            table.Cell().Element(DataCell).Text(row.UpdatedBy ?? "")
                                 .FontSize(8).FontColor(TextDark);
                            table.Cell().Element(DataCell)
                                 .AlignCenter()
                                 .Text(row.IsDeleted ? "Yes" : "No")
                                 .FontSize(8).FontColor(TextDark);
                        }
                    });

                    //    footer  
                    page.Footer().Row(footer =>
                    {
                        footer.RelativeItem()
                              .Text($"Total customers: {rows.Count}")
                              .Bold().FontSize(9);

                        footer.RelativeItem()
                              .AlignRight()
                              .Text(text =>
                              {
                                  text.Span("Page ").FontSize(9);
                                  text.CurrentPageNumber().FontSize(9);
                                  text.Span(" of ").FontSize(9);
                                  text.TotalPages().FontSize(9);
                              });
                    });
                });
            }).GeneratePdf();   // returns byte[] — no MemoryStream needed
        }

        private static string BuildTitle(DateTime? from, DateTime? to) =>
            (from, to) switch
            {
                (null, null) => "Customer Report — All Records",
                ({ } f, { } t) => $"Customer Report — {f:yyyy-MM-dd} to {t:yyyy-MM-dd}",
                ({ } f, null) => $"Customer Report — From {f:yyyy-MM-dd}",
                (null, { } t) => $"Customer Report — Up to {t:yyyy-MM-dd}",
            };
    }
}