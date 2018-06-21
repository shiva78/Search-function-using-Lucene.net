using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Search_function.Models
{
    public class textConverter
    {
        private string filePath = string.Empty;
        string tempPath1 = HttpContext.Current.Server.MapPath("/temp/path.txt");
        string pathInfo = string.Empty;
        public textConverter(string pdfpaths)
        {
            filePath = pdfpaths;
        }
        public string pdfConverted()
        {
            string[] pdfFiles = Directory.GetFiles(filePath, "*.pdf", SearchOption.AllDirectories);
            string text = string.Empty;
            string fileName = string.Empty;
            foreach (string pdfFile in pdfFiles)
            {
                // temp folder path
                string tempPath = HttpContext.Current.Server.MapPath("/temp");
                //saving file path to tempfile
                pathInfo += pdfFile + ",";
                File.WriteAllText(tempPath1, pathInfo);
                string filenameWithoutPath = System.IO.Path.GetFileName(pdfFile);
                // saving file to the temp folder with file neme and extention of .txt
                fileName = System.IO.Path.Combine(tempPath, filenameWithoutPath + ".txt");
                ///////////
                PdfReader reader = new PdfReader(pdfFile);
                text = string.Empty;
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    text += PdfTextExtractor.GetTextFromPage(reader, page);
                }

                //writing into existing file
                StreamWriter streamWriter = File.AppendText(fileName);
                streamWriter.WriteLine(text);
                streamWriter.Close();
                ////////////
                reader.Close();
            }
            return text;
        }
        public string docxConverted()
        {
            string[] docxFiles = Directory.GetFiles(filePath, "*.docx", SearchOption.AllDirectories);
            string text = string.Empty;
            string fileName = string.Empty;
            string docxText = "";
            foreach (string docxFile in docxFiles)
            {
                string tempPath = HttpContext.Current.Server.MapPath("/temp");
                pathInfo += docxFile + ",";
                File.WriteAllText(tempPath1, pathInfo);
                string filenameWithoutPath = System.IO.Path.GetFileName(docxFile);
                DocxToText dtt = new DocxToText(filenameWithoutPath, docxFile);
                docxText = dtt.ExtractText();
                File.WriteAllText(System.IO.Path.Combine(tempPath, filenameWithoutPath + ".txt"), docxText);
            }
            return docxText;
        }
    }
}