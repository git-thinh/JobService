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
using System.Text;
using System.Drawing.Imaging;

public class JPdf : IJob
{
    readonly IApp m_app;
    CancellationTokenSource cancellationToken;
    public JPdf(IApp app) => m_app = app;
    public void Cancel() => cancellationToken.Cancel();

    public void test(PerformContext context)
    {
        var data = new Dictionary<string, object>() {
            { "file", m_app.PATH_ROOT + "Files\\2.pdf" },
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
        if (File.Exists(file))
        {
            //_splitToPages_iTextSharp(file);
            //_splitToPages_PDFsharp(file);
            _splitToPages_PdfiumViewer(file);
        }
        context.WriteLine("-> Done: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss" + " ..."));
    }

    static string buildTotalPage(int page)
    {
        string s = page.ToString();
        switch (s.Length)
        {
            default: return "100000";
            case 1: return "10000" + s;
            case 2: return "1000" + s;
            case 3: return "100" + s;
            case 4: return "10" + s;
            case 5: return "1" + s;
        }
    }

    void _splitToPages_PdfiumViewer(string file)
    {
        var fileInfo = new FileInfo(file);
        var inputDoc = PdfiumViewer.PdfDocument.Load(file);
        long fileSize = fileInfo.Length;
        int max = inputDoc.PageCount;
        string key = string.Format("{0}{1}", buildTotalPage(max), fileSize);

        if (max > 5) max = 5;
        int w, h;
        for (int i = 0; i < max; i++)
        {
            int pageNumber = i + 1;
            w = (int)inputDoc.PageSizes[i].Width;
            h = (int)inputDoc.PageSizes[i].Height;
            //if (w >= h) w = this.Width;
            //else w = 1200;
            if (w < 1200) w = 1200;
            h = (int)((w * inputDoc.PageSizes[i].Height) / inputDoc.PageSizes[i].Width);
            using (var ms = new MemoryStream())
            using (var image = inputDoc.Render(i, w, h, 100, 100, false))
            {
                //image.Save(@"C:\temp\" + pageNumber + "-.jpg", ImageFormat.Jpeg);
                image.Save(ms, ImageFormat.Jpeg);

                var lz = LZ4Codec.Wrap(ms.ToArray());
                //var decompressed = LZ4Codec.Unwrap(compressed);
                //long s1 = ms.Length;
                //long s2 = lz.Length;
                //m_app.RedisUpdate(key, i.ToString(), lz);

                //var db = RedisStore
            }
        }
    }

    void _splitToPages_PDFsharp(string file)
    {
        // Open the file
        var inputDoc = PdfSharp_IO.PdfReader.Open(file, PdfSharp_IO.PdfDocumentOpenMode.Import);
        long fileSize = inputDoc.FileSize;
        int max = inputDoc.PageCount;
        string key = string.Format("{0}{1}", buildTotalPage(max), inputDoc.FileSize);

        string name = Path.GetFileNameWithoutExtension(file);
        if (max > 5) max = 5;
        for (int idx = 0; idx < max; idx++)
        {
            int i = idx + 1;
            var ms = new MemoryStream();
            using (var outputDocument = new PdfSharp_.PdfDocument())
            {
                var options = outputDocument.Options;
                options.FlateEncodeMode = PdfSharp_.PdfFlateEncodeMode.BestCompression;
                options.UseFlateDecoderForJpegImages = PdfSharp_.PdfUseFlateDecoderForJpegImages.Automatic;
                options.CompressContentStreams = true;
                options.NoCompression = false;
                options.FlateEncodeMode = PdfSharp_.PdfFlateEncodeMode.BestCompression;

                outputDocument.AddPage(inputDoc.Pages[idx]);

                //outputDocument.Save(@"C:\temp\" + i + "-.pdf");                
                outputDocument.Save(ms);
            }

            //// Create new document
            //var outputDocument = new PdfSharp_.PdfDocument();
            //outputDocument.Version = inputDocument.Version;
            //outputDocument.Info.Title = String.Format("Page {0} of {1}", i, inputDocument.Info.Title);
            //outputDocument.Info.Creator = inputDocument.Info.Creator;

            //// Add the page and save it
            //outputDocument.AddPage(inputDocument.Pages[idx]);
            //outputDocument.Save(@"C:\temp\" + i + "-.pdf");
            m_app.RedisUpdate(key, i.ToString(), ms.ToArray());
        }
    }

    void _splitToPages_iTextSharp(string file)
    {
        var reader = new iTextSharpPdf.PdfReader(file);
        reader.RemoveUnusedObjects();
        long fileSize = reader.FileLength;

        int currentPage = 1;

        var readerCopy = new iTextSharpPdf.PdfReader(file);
        readerCopy.RemoveUnusedObjects();
        readerCopy.RemoveAnnotations();
        readerCopy.RemoveFields();
        readerCopy.RemoveUsageRights();
        foreach (KeyValuePair<string, string> KV in readerCopy.Info)
            readerCopy.Info.Remove(KV.Key);
        //int headerSize = readerCopy.Metadata.Length;
        //string mt = Encoding.UTF8.GetString(readerCopy.Metadata);

        int max = reader.NumberOfPages;
        if (max > 5) max = 5;
        string key = string.Format("{0}{1}", buildTotalPage(max), fileSize);
        for (int i = 1; i <= max; i++)
        {

            ////using (FileStream fs = new FileStream(@"C:\temp\" + i + "-.pdf", FileMode.Create, FileAccess.Write, FileShare.None))
            ////{
            ////    using (var d = new iTextSharpText.Document())
            ////    {
            ////        using (var w = new iTextSharpPdf.PdfCopy(d, fs))
            ////        {
            ////            d.Open();
            ////            w.AddPage(w.GetImportedPage(reader, i));
            ////            d.Close();
            ////        }                     
            ////    }
            ////}

            using (var ms = new MemoryStream())
            {
                var docCopy = new iTextSharpText.Document(reader.GetPageSizeWithRotation(currentPage));
                //var pdfCopy = new iTextSharpPdf.PdfCopy(docCopy, new FileStream(@"C:\temp\" + i + "-.pdf", FileMode.Create));
                var pdfCopy = new iTextSharpPdf.PdfCopy(docCopy, ms);
                docCopy.Open();
                var page = pdfCopy.GetImportedPage(readerCopy, currentPage);
                pdfCopy.SetFullCompression();
                pdfCopy.AddPage(page);
                currentPage += 1;
                //long len = ms.Length;
                docCopy.Close();
                pdfCopy.Close();
                m_app.RedisUpdate(key, i.ToString(), ms.ToArray());
            }
        }
        readerCopy.Close();
        reader.Close();
    }
}