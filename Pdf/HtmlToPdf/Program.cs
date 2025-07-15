using HtmlToPdf;
using HtmlToPdf.Contracts;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Razor.Templating.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<InvoiceFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("invoice-report", async (InvoiceFactory invoiceFactory) =>
{
    Invoice invoice = invoiceFactory.Create();

    var html = await RazorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoice);

    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();
    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });
    var page = await browser.NewPageAsync();

    await page.SetContentAsync(html);

    await page.EvaluateExpressionAsync("document.fonts.ready");

    var pdfData = await page.PdfDataAsync(new PdfOptions
    {
        Format = PaperFormat.A4,
        PrintBackground = true,
        MarginOptions = new MarginOptions
        {
            Top = "50px",
            Right = "20px",
            Left = "20px",
            Bottom = "50px"
        }
    });

    return Results.File(pdfData, "application/pdf", $"invoice-{invoice.Number}.pdf");
});
app.Run();
