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
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Drawing;

namespace PdfSplit
{
    class Program
    {
        static void Main(string[] args)
        {
            //split_01(@"../files/146.pdf");
            convert_img("10014656433355-0");
        }

        static void convert_img(string id)
        {
            var cacheWrite = RedisWrite.Db;
            var ms = RedisRead.Db.StreamRange("RAW", id, id, 1);
            if (ms.Length > 0)
            {
                var a = ms[0].Values;
                int max = a.Length;
                max = 2;
                for (int i = 1; i < max; i++)
                {
                    var buf = (byte[])a[i].Value;
                    var lz = LZ4Codec.Unwrap(buf);
                    //File.WriteAllBytes("../files/_/" + i + (i == 0 ? ".txt" : ".pdf"), lz);

                    var doc = PdfSharp_IO.PdfReader.Open(new MemoryStream(lz));
                    var page = doc.Pages[0];
                    using (var xgr = PdfSharp.Drawing.XGraphics.FromPdfPage(page))
                    {
                        Bitmap b = new Bitmap(1, 1, xgr.);
                    }





                }
            }
        }

        static void split_01(string file)
        {
            var db = RedisWrite.Db;
            // Open the file
            var inputDoc = PdfSharp_IO.PdfReader.Open(file, PdfSharp_IO.PdfDocumentOpenMode.Import);
            long fileSize = inputDoc.FileSize;
            int max = inputDoc.PageCount;
            string key = DocumentStatic.buildId(max, fileSize);

            string name = Path.GetFileNameWithoutExtension(file);
            //if (max > 5) max = 5;


            var obj = new Dictionary<string, object>() {
                { "id", long.Parse(key) },
                { "file_name", Path.GetFileNameWithoutExtension(file) },
                { "file_type", "pdf" },
                { "file_size", fileSize },
                { "file_created", "" },
                { "page", max }
            };

            string jsonInfo = JsonConvert.SerializeObject(obj);
            var bufInfo = ASCIIEncoding.UTF8.GetBytes(jsonInfo);
            var lsEntry = new List<NameValueEntry>() {
                new NameValueEntry("0", LZ4Codec.Wrap(bufInfo,0,bufInfo.Length))
            };
            db.HashSet("IDS", key, jsonInfo);

            for (int idx = 0; idx < max; idx++)
            {
                using (var outputDocument = new PdfSharp_.PdfDocument())
                {
                    var options = outputDocument.Options;
                    options.FlateEncodeMode = PdfSharp_.PdfFlateEncodeMode.BestCompression;
                    options.UseFlateDecoderForJpegImages = PdfSharp_.PdfUseFlateDecoderForJpegImages.Automatic;
                    options.CompressContentStreams = true;
                    options.NoCompression = false;
                    options.FlateEncodeMode = PdfSharp_.PdfFlateEncodeMode.BestCompression;

                    outputDocument.AddPage(inputDoc.Pages[idx]);

                    using (var ms = new MemoryStream())
                    {
                        outputDocument.Save(ms);
                        var bt = ms.ToArray();
                        int i = idx + 1;
                        //outputDocument.Save(@"C:\temp\" + i + "-.pdf"); 
                        lsEntry.Add(new NameValueEntry(i.ToString(), LZ4Codec.Wrap(bt, 0, bt.Length)));
                        Console.WriteLine("{0}-{1}...", i, max);
                    }

                    //// Create new document
                    //var outputDocument = new PdfSharp_.PdfDocument();
                    //outputDocument.Version = inputDocument.Version;
                    //outputDocument.Info.Title = String.Format("Page {0} of {1}", i, inputDocument.Info.Title);
                    //outputDocument.Info.Creator = inputDocument.Info.Creator;

                    //// Add the page and save it
                    //outputDocument.AddPage(inputDocument.Pages[idx]);
                    //outputDocument.Save(@"C:\temp\" + i + "-.pdf");
                    //m_app.RedisUpdate(key, i.ToString(), ms.ToArray());
                }
            }
            ////long kkk = db.StreamDelete("BUF", new RedisValue[] { key + "-0" });
            string did = db.StreamAdd("RAW", lsEntry.ToArray(), key + "-0");

        }

        static void test01(string file)
        {
            var db = RedisWrite.Db;

            if (!File.Exists(file)) return;

            var reader = new iTextSharpPdf.PdfReader(file);
            reader.RemoveUnusedObjects();
            long fileSize = reader.FileLength;

            int currentPage = 1;

            var readerCopy = new iTextSharpPdf.PdfReader(file);
            readerCopy.RemoveUnusedObjects();
            readerCopy.RemoveAnnotations();
            readerCopy.RemoveFields();
            readerCopy.RemoveUsageRights();
            string CreationDate = "";
            foreach (KeyValuePair<string, string> KV in readerCopy.Info)
            {
                if (KV.Key == "CreationDate") CreationDate = KV.Value;
                //readerCopy.Info.Remove(KV.Key);
            }

            //int headerSize = readerCopy.Metadata.Length;
            //string mt = Encoding.UTF8.GetString(readerCopy.Metadata);

            int max = reader.NumberOfPages;
            if (max > 5) max = 2;
            string key = DocumentStatic.buildId(max, fileSize);

            var obj = new Dictionary<string, object>() {
                { "id", long.Parse(key) },
                { "file_name", Path.GetFileNameWithoutExtension(file) },
                { "file_type", "pdf" },
                { "file_size", fileSize },
                { "file_created", CreationDate },
                { "page", max }
            };
            string jsonInfo = JsonConvert.SerializeObject(obj);
            var bufInfo = ASCIIEncoding.UTF8.GetBytes(jsonInfo);
            var lsEntry = new List<NameValueEntry>() {
                new NameValueEntry(0, LZ4.LZ4Codec.Encode(bufInfo,0,bufInfo.Length))
            };

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
                    //m_app.RedisUpdate(key, i.ToString(), ms.ToArray());

                    lsEntry.Add(new NameValueEntry(i, ms.ToArray()));
                }
            }
            readerCopy.Close();
            reader.Close();
            string did = db.StreamAdd("BUF", lsEntry.ToArray(), key + "-0");
        }
    }
}
