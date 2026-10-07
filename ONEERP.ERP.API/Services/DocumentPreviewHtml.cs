using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Services;

/* ---------------------------------------------------------------------------
   HTML renderer for the Document Designer. Given a persisted
   DesignerVersionDto it emits a real 210x297 mm (A4) HTML document.

   Two modes:
     • Sample mode (no print data): placeholder text per element - used when
       previewing straight from the designer without an invoice.
     • Data mode (SalesInvoicePrintDto supplied): every variable binding
       (Company.Name, Invoice.GrandTotal, Item.ProductName, ...) resolves to
       the actual database value of the sales invoice being printed.
   --------------------------------------------------------------------------- */

public class DocumentPreviewHtml
{
    private const decimal DefaultPageWidth = 210m;
    private const decimal DefaultPageHeight = 297m;

    public static string Render(
        DesignerVersionDto design,
        IReadOnlyDictionary<long, string>? componentTypes = null,
        decimal paperWidthMm = DefaultPageWidth,
        decimal paperHeightMm = DefaultPageHeight,
        SalesInvoicePrintDto? printData = null)
    {
        paperWidthMm = paperWidthMm > 0 ? paperWidthMm : DefaultPageWidth;
        paperHeightMm = paperHeightMm > 0 ? paperHeightMm : DefaultPageHeight;

        var sb = new System.Text.StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset=\"utf-8\">");
        sb.Append("<title>Sales Invoice Preview</title><style>");
        sb.Append("*{box-sizing:border-box}");
        sb.Append("body{margin:0;padding:16px;background:#e8eaed;font-family:Arial,Helvetica,sans-serif;}");
        sb.Append(".toolbar{margin:0 0 10px;text-align:center}");
        sb.Append(".toolbar button{padding:6px 18px;font:600 13px Arial;border:1px solid #888;background:#fff;");
        sb.Append("border-radius:4px;cursor:pointer}");
        sb.Append(".toolbar button:hover{background:#f0f0f0}");
        sb.Append("#paper{position:relative;overflow:hidden;margin:0 auto;color:#000;background:#fff;");
        sb.Append("width:").Append(Mm(paperWidthMm)).Append("mm;height:").Append(Mm(paperHeightMm)).Append("mm;");
        sb.Append("box-shadow:0 2px 14px rgba(0,0,0,.25)}");
        sb.Append("#pageInner{position:relative;width:100%;height:100%}");
        sb.Append(".section{position:absolute;overflow:visible}");
        sb.Append(".element{position:absolute;overflow:hidden;font-size:3.2mm;line-height:1.25}");
        sb.Append(".element .wrap{width:100%;height:100%;overflow:hidden;word-wrap:break-word;overflow-wrap:break-word;}");
        sb.Append("table.itab{border-collapse:collapse;table-layout:fixed;width:100%;height:100%;font-size:2.8mm}");
        sb.Append("table.itab th,table.itab td{border:0.2mm solid #999;padding:1mm;overflow:hidden;");
        sb.Append("word-wrap:break-word;overflow-wrap:break-word}");
        sb.Append("table.itab tr.empty td{border:none}");
        sb.Append("img.logo{max-height:100%;max-width:100%;object-fit:contain}");
        sb.Append("@page{size:").Append(Mm(paperWidthMm)).Append("mm ").Append(Mm(paperHeightMm)).Append("mm;margin:0}");
        sb.Append("@media print{body{background:#fff;padding:0}.toolbar{display:none}");
        sb.Append("#paper{box-shadow:none;width:").Append(Mm(paperWidthMm)).Append("mm;height:").Append(Mm(paperHeightMm)).Append("mm}}");
        sb.Append("</style></head><body>");
        sb.Append("<div class=\"toolbar\"><button onclick=\"window.print()\">Print</button></div>");
        sb.Append("<div id=\"paper\"><div id=\"pageInner\">");

        var pageWidth = paperWidthMm;
        foreach (var section in OrderedVisible(design.Sections))
        {
            var sLeft = section.X is >= 0 ? section.X.Value : 0m;
            var sTop = section.Y is >= 0 ? section.Y.Value : 0m;
            var sWidth = section.Width is > 0 ? Math.Min(section.Width.Value, pageWidth - sLeft) : pageWidth;
            var sHeight = section.Height is > 0 ? section.Height.Value : 25m;
            if (sLeft + sWidth > pageWidth) sWidth = Math.Max(0m, pageWidth - sLeft);

            sb.Append("<div class=\"section\" style=\"left:").Append(Mm(sLeft))
                .Append("mm;top:").Append(Mm(sTop))
                .Append("mm;width:").Append(Mm(sWidth))
                .Append("mm;height:").Append(Mm(sHeight)).Append("mm\">");

            var order = 0;
            foreach (var element in section.Elements.Where(e => e.IsVisible))
            {
                sb.Append(RenderElement(element, order++, sWidth, componentTypes, printData));
            }
            sb.Append("</div>");
        }

        sb.Append("</div></div></body></html>");
        return sb.ToString();
    }

    private static string Mm(decimal v) => Math.Round(v, 2).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static IEnumerable<DesignerSectionDto> OrderedVisible(IEnumerable<DesignerSectionDto> sections)
        => sections.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder).ThenBy(s => s.SectionId ?? 0);

    private static string RenderElement(
        DesignerElementDto element,
        int order,
        decimal sectionWidth,
        IReadOnlyDictionary<long, string>? componentTypes,
        SalesInvoicePrintDto? data)
    {
        var left = element.X is >= 0 ? element.X.Value : 0m;
        var top = element.Y is >= 0 ? element.Y.Value : 0m;
        var width = element.Width is > 0 ? Math.Min(element.Width.Value, sectionWidth - left) : sectionWidth - left;
        var height = element.Height is > 0 ? element.Height.Value : 8m;

        var style = new System.Text.StringBuilder();
        style.Append("left:").Append(Mm(left)).Append("mm;top:").Append(Mm(top))
             .Append("mm;width:").Append(Mm(Math.Max(0m, width))).Append("mm;height:").Append(Mm(height)).Append("mm;");
        if (element.Style is { } st)
        {
            if (st.FontSize is > 0) style.Append("font-size:").Append(Mm(st.FontSize.Value)).Append("mm;");
            if (!string.IsNullOrWhiteSpace(st.FontWeight)) style.Append("font-weight:").Append(st.FontWeight.Trim().ToLowerInvariant()).Append(";");
            if (!string.IsNullOrWhiteSpace(st.FontStyle)) style.Append("font-style:").Append(st.FontStyle.Trim().ToLowerInvariant()).Append(";");
            if (!string.IsNullOrWhiteSpace(st.TextAlign)) style.Append("text-align:").Append(st.TextAlign.Trim().ToLowerInvariant()).Append(";");
            if (!string.IsNullOrWhiteSpace(st.VerticalAlign))
                style.Append("display:flex;align-items:").Append(MapVerticalAlign(st.VerticalAlign)).Append(";");
            if (!string.IsNullOrWhiteSpace(st.TextColor)) style.Append("color:").Append(SafeColor(st.TextColor)).Append(";");
            if (!string.IsNullOrWhiteSpace(st.BackgroundColor)) style.Append("background:").Append(SafeColor(st.BackgroundColor)).Append(";");
            var borderColor = string.IsNullOrWhiteSpace(st.BorderColor) ? "#444" : SafeColor(st.BorderColor);
            style.Append("padding:").Append(Mm(st.PaddingTop ?? 0)).Append(' ')
                 .Append(Mm(st.PaddingRight ?? 0)).Append(' ')
                 .Append(Mm(st.PaddingBottom ?? 0)).Append(' ')
                 .Append(Mm(st.PaddingLeft ?? 0)).Append(";");
            if (st.BorderTop) style.Append("border-top:0.2mm solid ").Append(borderColor).Append(";");
            if (st.BorderRight) style.Append("border-right:0.2mm solid ").Append(borderColor).Append(";");
            if (st.BorderBottom) style.Append("border-bottom:0.2mm solid ").Append(borderColor).Append(";");
            if (st.BorderLeft) style.Append("border-left:0.2mm solid ").Append(borderColor).Append(";");
        }

        string inner;
        var isTable = element.ItemColumns.Count > 0
            || ComponentTypeOf(element, componentTypes) == "TABLE";
        if (isTable)
        {
            inner = RenderItemTable(element, data);
        }
        else if (element.ComponentId > 0
            && ComponentTypeOf(element, componentTypes) is "IMAGE" or "LOGO" or "SIGNATURE")
        {
            var src = Resolve("Company.LogoUrl", data) ?? string.Empty;
            inner = string.IsNullOrWhiteSpace(src)
                ? "<span style=\"opacity:.45\">Logo</span>"
                : $"<img class=\"logo\" src=\"{Esc(src)}\" alt=\"logo\"/>";
        }
        else
        {
            inner = RenderText(element, data);
        }

        return $"<div class=\"element\" id=\"el{order}\" style=\"{style}\"><div class=\"wrap\">{inner}</div></div>";
    }

    private static string MapVerticalAlign(string v) => v.Trim().ToLowerInvariant() switch
    {
        "bottom" => "flex-end",
        "middle" or "center" => "center",
        _ => "flex-start"
    };

    /// <summary>Accepts only hex (#RGB/#RRGGBB) or a small CSS safe-name set,</summary>
    private static string SafeColor(string c)
    {
        var t = c.Trim();
        if (System.Text.RegularExpressions.Regex.IsMatch(t, "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")) return t;
        return t.ToLowerInvariant() switch
        {
            "black" => "black", "white" => "white", "red" => "red", "green" => "green",
            "blue" => "blue", "gray" or "grey" => "gray", "transparent" => "transparent",
            _ => string.Empty
        };
    }

    private static string? ComponentTypeOf(DesignerElementDto element, IReadOnlyDictionary<long, string>? componentTypes)
        => element.ComponentId > 0 && componentTypes is not null
            && componentTypes.TryGetValue(element.ComponentId, out var t) ? t : null;

    /* ------------------------------------------------------------------ */
    /* Text / variable elements                                            */
    /* ------------------------------------------------------------------ */

    private static string RenderText(DesignerElementDto element, SalesInvoicePrintDto? data)
    {
        // A variable element with mapped fields renders "Label: {value}" lines.
        if (element.Fields.Count > 0)
        {
            var lines = new List<string>();
            foreach (var f in element.Fields.Where(f => f.IsVisible).OrderBy(f => f.TemplateFieldId))
            {
                var value = Resolve(f.BindingPath, data) ?? string.Empty;
                var label = string.IsNullOrWhiteSpace(f.Label) ? null : f.Label;
                lines.Add(label is null
                    ? $"<div>{Esc(value)}</div>"
                    : $"<div><b>{Esc(label)}:</b> {Esc(value)}</div>");
            }
            return string.Join(string.Empty, lines);
        }

        // Static element: ElementName is the literal text the designer saved.
        var text = element.ElementName ?? string.Empty;
        return $"<div style=\"white-space:pre-wrap\">{Esc(text)}</div>";
    }

    /* ------------------------------------------------------------------ */
    /* Item table                                                          */
    /* ------------------------------------------------------------------ */

    private static string RenderItemTable(DesignerElementDto element, SalesInvoicePrintDto? data)
    {
        var columns = element.ItemColumns.Count > 0
            ? element.ItemColumns.Where(c => c.IsVisible).OrderBy(c => c.DisplayOrder).ToList()
            : DefaultColumns();

        var items = data?.Items ?? new List<SalesInvoicePrintItemDto>();
        // Sample rows when no real invoice is attached to the preview (spec §24).
        if (items.Count == 0 && data is null)
        {
            items = new List<SalesInvoicePrintItemDto>
            {
                new() { SlNo = 1, ProductName = "Product A", HsnCode = "8517", UnitName = "NOS", Quantity = 2, Rate = 500m, TaxPercent = 18m, TaxAmount = 180m, LineTotal = 1180m },
                new() { SlNo = 2, ProductName = "Product B", HsnCode = "8471", UnitName = "NOS", Quantity = 1, Rate = 300m, TaxPercent = 18m, TaxAmount = 54m, LineTotal = 354m },
            };
        }

        var sb = new System.Text.StringBuilder();
        sb.Append("<table class=\"itab\"><thead><tr>");
        foreach (var c in columns)
        {
            var w = c.Width is > 0 ? $" style=\"width:{Mm(Math.Min(c.Width.Value, 200))}mm\"" : string.Empty;
            sb.Append("<th").Append(w).Append(">").Append(Esc(c.HeaderText)).Append("</th>");
        }
        sb.Append("</tr></thead><tbody>");

        foreach (var item in items)
        {
            sb.Append("<tr>");
            foreach (var c in columns)
            {
                var align = (c.Alignment ?? "LEFT").Trim().ToLowerInvariant() switch
                {
                    "right" => "right",
                    "center" => "center",
                    _ => "left"
                };
                sb.Append("<td style=\"text-align:").Append(align).Append("\">")
                    .Append(Esc(ItemValue(c.FieldName, item))).Append("</td>");
            }
            sb.Append("</tr>");
        }

        // Pad a couple of empty rows so an empty table still shows its borders.
        if (items.Count == 0)
        {
            for (var i = 0; i < 3; i++)
            {
                sb.Append("<tr class=\"empty\">");
                foreach (var _ in columns) sb.Append("<td>&nbsp;</td>");
                sb.Append("</tr>");
            }
        }

        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    private static List<DesignerItemColumnDto> DefaultColumns() => new()
    {
        new() { FieldName = "SlNo", HeaderText = "#", DisplayOrder = 1, Width = 10, Alignment = "CENTER" },
        new() { FieldName = "ProductName", HeaderText = "Product", DisplayOrder = 2 },
        new() { FieldName = "UnitName", HeaderText = "Unit", DisplayOrder = 3, Width = 15, Alignment = "CENTER" },
        new() { FieldName = "Quantity", HeaderText = "Qty", DisplayOrder = 4, Width = 15, Alignment = "RIGHT" },
        new() { FieldName = "Rate", HeaderText = "Rate", DisplayOrder = 5, Width = 20, Alignment = "RIGHT" },
        new() { FieldName = "LineTotal", HeaderText = "Amount", DisplayOrder = 6, Width = 25, Alignment = "RIGHT" }
    };

    private static readonly string[] KnownItemFields =
        ["SlNo", "ProductCode", "ProductName", "HsnCode", "UnitName", "Quantity", "Rate", "DiscountAmount", "TaxableAmount", "TaxPercent", "TaxAmount", "LineTotal"];

    private static string ItemValue(string fieldName, SalesInvoicePrintItemDto item) => fieldName switch
    {
        "SlNo" => item.SlNo.ToString(),
        "ProductCode" => item.ProductCode ?? string.Empty,
        "ProductName" => item.ProductName ?? string.Empty,
        "HsnCode" or "HSNSAC" or "HSN" => item.HsnCode ?? string.Empty,
        "UnitName" or "Unit" => item.UnitName ?? string.Empty,
        "Quantity" => Fmt(item.Quantity),
        "Rate" => Fmt(item.Rate),
        "DiscountAmount" => Fmt(item.DiscountAmount),
        "TaxableAmount" => Fmt(item.TaxableAmount),
        "TaxPercent" => Fmt(item.TaxPercent),
        "TaxAmount" => Fmt(item.TaxAmount),
        "LineTotal" => Fmt(item.LineTotal),
        _ => string.Empty
    };

    public static string KnownItemFieldsCsv => string.Join(",", KnownItemFields);

    /* ------------------------------------------------------------------ */
    /* Variable binding resolution                                         */
    /* ------------------------------------------------------------------ */

    private static string? Resolve(string? bindingPath, SalesInvoicePrintDto? d)
    {
        if (string.IsNullOrWhiteSpace(bindingPath) || d is null) return null;
        var value = bindingPath.Trim() switch
        {
            "Company.Name" => d.CompanyName,
            "Company.GSTIN" => d.CompanyGstin,
            "Company.Address" => d.CompanyAddress,
            "Company.Phone" => d.CompanyPhone,
            "Company.Email" => d.CompanyEmail,
            "Company.LogoUrl" => d.CompanyLogoUrl,
            "Invoice.Number" => d.SalesInvoiceNo,
            "Invoice.Date" => d.InvoiceDate.ToString("dd-MMM-yyyy"),
            "Invoice.Type" => d.InvoiceTypeName,
            "Invoice.SubTotal" => Fmt(d.TotalGrossAmount),
            "Invoice.Discount" => Fmt(d.TotalDiscountAmount),
            "Invoice.Tax" => Fmt(d.TotalTaxAmount != 0 ? d.TotalTaxAmount : d.TotalCGSTAmount + d.TotalSGSTAmount + d.TotalIGSTAmount + d.TotalCESSAmount),
            "Invoice.CGST" => Fmt(d.TotalCGSTAmount),
            "Invoice.SGST" => Fmt(d.TotalSGSTAmount),
            "Invoice.IGST" => Fmt(d.TotalIGSTAmount),
            "Invoice.CESS" => Fmt(d.TotalCESSAmount),
            "Invoice.TaxableAmount" => Fmt(d.TotalTaxableAmount),
            "Invoice.RoundOff" => Fmt(d.TotalRoundOff),
            "Invoice.GrandTotal" => Fmt(d.GrandTotal),
            "Invoice.PaidAmount" => Fmt(d.PaidAmount),
            "Invoice.BalanceAmount" => Fmt(d.BalanceAmount),
            "Customer.Name" => d.CustomerName,
            "Customer.GSTIN" => d.CustomerGstin,
            "Customer.Address" => d.CustomerAddress,
            "Customer.Phone" => d.CustomerPhone,
            "Supplier.Name" => d.CustomerName,
            "Supplier.GSTIN" => d.CustomerGstin,
            "Supplier.Address" => d.CustomerAddress,
            "Supplier.Phone" => d.CustomerPhone,
            "Branch.Name" => d.BranchName,
            "Warehouse.Name" => d.WarehouseName,
            "Payment.Method" => d.PaymentMode,
            "Payment.Amount" => Fmt(d.PaidAmount),
            "Invoice.Remarks" => d.Remarks,
            _ => null
        };
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string Fmt(decimal v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

    private static string Esc(string? s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("\"", "&quot;").Replace("'", "&#39;");
    }
}
