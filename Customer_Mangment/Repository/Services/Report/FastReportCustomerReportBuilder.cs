using Customer_Mangment.CQRS.Customers.Queries.Report.Dtos;
using Customer_Mangment.Repository.Interfaces.Report;
using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Utils;
using System.Drawing;

namespace Customer_Mangment.Repository.Services.Reports
{
    public sealed class FastReportCustomerReportBuilder : ICustomerReportBuilder
    {
        private const float PageWidthPt = 600f;
        private const float PageHeightPt = 595.28f;

        private const float PtToFr = 96f / 72f;

        private const float MarginPt = 36f;

        private const float UsableWidthPt = PageWidthPt - MarginPt * 2;

        private const float RowHeightPt = 20f;

        private const float ColId = 150f;
        private const float ColName = 80f;
        private const float ColMobile = 80f;
        private const float ColCreatedBy = 80f;
        private const float ColUpdatedBy = 80f;
        private const float ColIsDeleted = 50f;



        private static readonly Color HeaderBg = Color.FromArgb(0x2E, 0x86, 0xAB);
        private static readonly Color HeaderFg = Color.White;
        private static readonly Color AltRowBg = Color.FromArgb(0xF2, 0xF8, 0xFB);
        private static readonly Color BorderColor = Color.FromArgb(0xCC, 0xCC, 0xCC);


        public byte[] Build(IReadOnlyList<CustomerReportRow> rows, DateTime? from, DateTime? to)
        {
            Config.WebMode = true;

            using var report = new Report();

            var page = new ReportPage
            {
                Name = "Page1",
                PaperWidth = PageWidthPt / 2.8346f,
                PaperHeight = PageHeightPt / 2.8346f,
                Landscape = true,
                LeftMargin = MarginPt / 2.8346f,
                RightMargin = MarginPt / 2.8346f,
                TopMargin = MarginPt / 2.8346f,
                BottomMargin = MarginPt / 2.8346f,
            };
            report.Pages.Add(page);

            var titleBand = new ReportTitleBand
            {
                Name = "Title",
                Height = Units.Centimeters * 2.2f,
            };
            page.ReportTitle = titleBand;

            AddText(titleBand, BuildTitle(from, to),
                frX: 0, frY: 0,
                frW: Pt2Fr(UsableWidthPt), frH: Pt2Fr(26f),
                fontSize: 14, bold: true, hAlign: HorzAlign.Center);

            AddText(titleBand, $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
                frX: 0, frY: Pt2Fr(28f),
                frW: Pt2Fr(UsableWidthPt), frH: Pt2Fr(16f),
                fontSize: 9, hAlign: HorzAlign.Center);

            var headerBand = new PageHeaderBand
            {
                Name = "PageHeader",
                Height = Pt2Fr(RowHeightPt + 2f),
            };
            page.PageHeader = headerBand;
            AddTableHeader(headerBand);

            var dataBand = new DataBand
            {
                Name = "Data",
                Height = Pt2Fr(RowHeightPt),
            };
            page.Bands.Add(dataBand);

            report.Dictionary.RegisterData(BuildDataTable(rows), "Customers", true);
            dataBand.DataSource = report.GetDataSource("Customers");

            float ptX = 0;
            AddDataCell(dataBand, "[Customers.Id]", ref ptX, ColId);
            AddDataCell(dataBand, "[Customers.Name]", ref ptX, ColName);
            AddDataCell(dataBand, "[Customers.Mobile]", ref ptX, ColMobile);
            AddDataCell(dataBand, "[Customers.CreatedBy]", ref ptX, ColCreatedBy);
            AddDataCell(dataBand, "[Customers.UpdatedBy]", ref ptX, ColUpdatedBy);
            AddDataCell(dataBand, "[Customers.IsDeleted]", ref ptX, ColIsDeleted,
                centerText: true);

            var summaryBand = new ReportSummaryBand
            {
                Name = "Summary",
                Height = Pt2Fr(36f),
            };
            page.ReportSummary = summaryBand;

            AddText(summaryBand, $"Total customers: {rows.Count}",
                frX: 0, frY: Pt2Fr(8f),
                frW: Pt2Fr(UsableWidthPt), frH: Pt2Fr(20f),
                fontSize: 10, bold: true, hAlign: HorzAlign.Left);

            report.Prepare();

            using var ms = new MemoryStream();
            using var export = new PDFSimpleExport();
            export.Export(report, ms);

            return ms.ToArray();
        }


        private static float Pt2Fr(float pt) => pt * PtToFr;

        private static string BuildTitle(DateTime? from, DateTime? to) =>
            (from, to) switch
            {
                (null, null) => "Customer Report — All Records",
                ({ } f, { } t) => $"Customer Report — {f:yyyy-MM-dd} to {t:yyyy-MM-dd}",
                ({ } f, null) => $"Customer Report — From {f:yyyy-MM-dd}",
                (null, { } t) => $"Customer Report — Up to {t:yyyy-MM-dd}",
            };

        private static System.Data.DataTable BuildDataTable(IReadOnlyList<CustomerReportRow> rows)
        {
            var dt = new System.Data.DataTable("Customers");
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Mobile", typeof(string));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("IsDeleted", typeof(string));

            foreach (var r in rows)
                dt.Rows.Add(
                    r.Id.ToString(),
                    r.Name,
                    r.Mobile,
                    r.CreatedBy,
                    r.UpdatedBy,
                    r.IsDeleted ? "Yes" : "No");

            return dt;
        }


        private static float ColW(float colPt) => Pt2Fr(colPt);

        private void AddTableHeader(BandBase band)
        {
            (string Label, float ColPt)[] cols =
            [
                ("Customer ID", ColId),
                ("Name",        ColName),
                ("Mobile",      ColMobile),
                ("Created By",  ColCreatedBy),
                ("Updated By",  ColUpdatedBy),
                ("Is Deleted",  ColIsDeleted),
            ];

            float ptX = 0;
            foreach (var (label, colPt) in cols)
            {
                float frX = Pt2Fr(ptX);
                float frW = ColW(colPt);
                float frH = Pt2Fr(RowHeightPt);

                band.Objects.Add(new TextObject
                {
                    Name = $"Hdr_{label.Replace(" ", "")}",
                    Bounds = new RectangleF(frX, 0, frW, frH),
                    Text = label,
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    FillColor = HeaderBg,
                    TextColor = HeaderFg,
                    HorzAlign = HorzAlign.Center,
                    VertAlign = VertAlign.Center,
                });
                ptX += colPt;
            }
        }

        private static void AddDataCell(
            BandBase band, string expression,
            ref float ptX, float colPt,
            bool centerText = false)
        {
            float frX = Pt2Fr(ptX);
            float frW = ColW(colPt);
            float frH = Pt2Fr(RowHeightPt);

            band.Objects.Add(new TextObject
            {
                Name = $"Cell_{expression.GetHashCode():X}",
                Bounds = new RectangleF(frX, 0, frW, frH),
                Text = expression,
                Font = new Font("Arial", 8),
                HorzAlign = centerText ? HorzAlign.Center : HorzAlign.Left,
                VertAlign = VertAlign.Center,
                Border = new Border
                {
                    Lines = BorderLines.Bottom,
                    Color = BorderColor,
                },
            });
            ptX += colPt;
        }

        private static void AddText(BandBase band,
                                    string text,
                                    float frX,
                                    float frY,
                                    float frW,
                                    float frH,
                                    float fontSize = 10,
                                    bool bold = false,
                                    bool italic = false,
                                    HorzAlign hAlign = HorzAlign.Left)
        {
            var style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;

            band.Objects.Add(new TextObject
            {
                Name = $"Txt_{Guid.NewGuid():N}",
                Bounds = new RectangleF(frX, frY, frW, frH),
                Text = text,
                Font = new Font("Arial", fontSize, style),
                HorzAlign = hAlign,
                VertAlign = VertAlign.Center,
            });
        }
    }
}
