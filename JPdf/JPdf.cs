using Hangfire.Console;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LZ4;
using iTextSharpText = iTextSharp.text;
using iTextSharpPdf = iTextSharp.text.pdf;
using PdfSharp_ = PdfSharp.Pdf;
using PdfSharp_IO = PdfSharp.Pdf.IO;

public class JPdf : IJob
{
    readonly IApp m_app;
    private Dictionary<string, object> m_data;
    CancellationTokenSource cancellationToken;
    public JPdf(IApp app) => m_app = app;
    public void Cancel() => cancellationToken.Cancel();

    public void test(PerformContext context)
    {
        var data = new Dictionary<string, object>() {
            { "file", @"D:\tt\core\JobService\Test\2.pdf" },
            { "_rd_publish", true }
        };
        call(context, data);
    }
    public void call(PerformContext context, Dictionary<string, object> data)
    {
        cancellationToken = new CancellationTokenSource();
        if (data == null) return;
        context.WriteLine("-> Started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + " ..."));

        string file = data.Get<string>("file", string.Empty);
        if (file.Length > 0)
        {
            //_splitToPages_iTextSharp(file);
            _splitToPages_PDFsharp(file);
        }
    }

    void _splitToPages_PDFsharp(string file)
    {
        // Open the file
        var inputDocument = PdfSharp_IO.PdfReader.Open(file, PdfSharp_IO.PdfDocumentOpenMode.Import);

        string name = Path.GetFileNameWithoutExtension(file);
        int max = inputDocument.PageCount;
        if (max > 5) max = 5;
        for (int idx = 0; idx < max; idx++)
        {
            int i = idx + 1;
            using (var outputDocument = new PdfSharp_.PdfDocument())
            {
                var options = outputDocument.Options;
                options.FlateEncodeMode = PdfSharp_.PdfFlateEncodeMode.BestCompression;
                options.UseFlateDecoderForJpegImages = PdfSharp_.PdfUseFlateDecoderForJpegImages.Automatic;
                options.CompressContentStreams = true;
                options.NoCompression = false;

                outputDocument.AddPage(inputDocument.Pages[idx]);
                //outputDocument.Save(@"C:\temp\" + i + "-.pdf");
                //outputDocument.Save()
            }

            //// Create new document
            //var outputDocument = new PdfSharp_.PdfDocument();
            //outputDocument.Version = inputDocument.Version;
            //outputDocument.Info.Title = String.Format("Page {0} of {1}", i, inputDocument.Info.Title);
            //outputDocument.Info.Creator = inputDocument.Info.Creator;

            //// Add the page and save it
            //outputDocument.AddPage(inputDocument.Pages[idx]);
            //outputDocument.Save(@"C:\temp\" + i + "-.pdf");
        }
    }

    void _splitToPages_iTextSharp(string file)
    {
        var reader = new iTextSharpPdf.PdfReader(file);
        reader.RemoveUnusedObjects();
        int fileSize = reader.FileLength;

        int pageCount = reader.NumberOfPages;
        int currentPage = 1;
        
        var readerCopy = new iTextSharpPdf.PdfReader(file);
        readerCopy.RemoveUnusedObjects();
        readerCopy.RemoveAnnotations();
        readerCopy.RemoveFields();
        readerCopy.RemoveUsageRights();

        int headerSize = readerCopy.Metadata.Length;

        if (pageCount > 5) pageCount = 5;
        for (int i = 1; i <= pageCount; i++)
        {
            var docCopy = new iTextSharpText.Document(reader.GetPageSizeWithRotation(currentPage));
            var pdfCopy = new iTextSharpPdf.PdfCopy(docCopy, new FileStream(@"C:\temp\" + i + "-.pdf", FileMode.Create));
            //var ms = new MemoryStream();
            //PdfCopy pdfCpy = new PdfCopy(doc, ms);
            docCopy.Open();

            var page = pdfCopy.GetImportedPage(readerCopy, currentPage);            
            pdfCopy.SetFullCompression();
            pdfCopy.AddPage(page);
            currentPage += 1;


            //var lz = LZ4Codec.Wrap(ms.ToArray());
            ////var decompressed = LZ4Codec.Unwrap(compressed);
            //long s1 = ms.Length;
            //long s2 = lz.Length;
            //File.WriteAllBytes(@"C:\temp\" + i + ".lz", lz);

            //long len = ms.Length;
            docCopy.Close();
            pdfCopy.Close();
        }
        readerCopy.Close();
        reader.Close();
    }
}