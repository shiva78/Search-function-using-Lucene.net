using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using Search_function.Models;
using HtmlAgilityPack;
using Lucene.Net.Documents;
using System.Text.RegularExpressions;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search.Highlight;
using System.Data;

namespace Search_function.Controllers
{
    public class HomeController : Controller
    {
        readonly static SimpleFSLockFactory _LockFactory = new SimpleFSLockFactory();
        //GET: Home
        public ActionResult test()
        {
            return View();
        }

        public ActionResult Search()
        {
            var path = Server.MapPath("/Index-lucene");
            int numberOfFiles = System.IO.Directory.GetFiles(path).Length;
            var searchText = Request.QueryString.ToString();
            string output = searchText.Substring(searchText.IndexOf('=') + 1);
            string searchWord = output.Replace('+', ' ');
            ViewBag.YourSearch = searchWord;
            if (numberOfFiles != 0 && output.Length > 0)
            {
                Lucene.Net.Store.Directory dir = FSDirectory.Open(path);
                Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
                IndexReader indexReader = IndexReader.Open(dir, true);
                Searcher indexSearch = new IndexSearcher(indexReader);

                try
                {
                    var startSearchTime = DateTime.Now.TimeOfDay;
                    string totaltimeTakenToSearch = string.Empty;
                    var queryParser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, new string[] { "metaTag", "prevewContent", "fileNameWithoutExtension" }, analyzer);
                    var query = queryParser.Parse(searchWord);
                    //ViewBag.SearchQuery = "Searching for: \"" + searchWord + "\"";
                    TopDocs resultDocs = indexSearch.Search(query, indexReader.NumDocs());
                    ViewBag.SearchQuery = resultDocs.TotalHits + " result(s) found for \"" + searchWord + "\"";
                    TopScoreDocCollector collector = TopScoreDocCollector.Create(20000, true);
                    indexSearch.Search(query, collector);
                    ScoreDoc[] hits = collector.TopDocs().ScoreDocs;
                    IFormatter formatter = new SimpleHTMLFormatter("<span style=\"color: black; font-weight: bold;\">", "</span>");
                    SimpleFragmenter fragmenter = new SimpleFragmenter(160);
                    QueryScorer scorer = new QueryScorer(query);
                    Highlighter highlighter = new Highlighter(formatter, scorer);
                    highlighter.TextFragmenter = fragmenter; //highlighter.SetTextFragmenter(fragmenter);
                    List<ListofResult> parts = new List<ListofResult>();
                    for (int i = 0; i < hits.Length; i++)
                    {
                        int docId = hits[i].Doc;
                        float score = hits[i].Score;
                        Document doc = indexSearch.Doc(docId);
                        string url = doc.Get("URL");
                        string title = doc.Get("filename");
                        TokenStream stream = analyzer.TokenStream("", new StringReader(doc.Get("prevewContent")));
                        string content = highlighter.GetBestFragments(stream, doc.Get("prevewContent"), 3, "...");
                        if (content == null || content == "")
                        {
                            string contents = doc.Get("prevewContent");
                            if (contents != "")
                            {
                                if (contents.Length < 480)
                                {
                                    content = contents.Substring(0, contents.Length);
                                }
                                else
                                {
                                    content = contents.Substring(0, 480);
                                }
                            }
                        }
                        parts.Add(new ListofResult() { FileName = title, Content = content, URL = url });
                        var endSearchTime = DateTime.Now.TimeOfDay;
                        var timeTaken = endSearchTime.TotalMilliseconds - startSearchTime.TotalMilliseconds;
                        totaltimeTakenToSearch = timeTaken.ToString();

                    }
                    //Search completed, dispose IndexSearcher
                    indexSearch.Dispose();
                    //assigning list into ViewBag
                    ViewBag.SearchResult = parts;
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                return RedirectToAction("UploadFile", "Home");
            }
            return View();
        }

        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase uploadDocument)
        {
            if (uploadDocument != null)
            {
                string docSavePath = Server.MapPath("/Documents/") + uploadDocument.FileName;
                uploadDocument.SaveAs(docSavePath);
                //Converting pdf and docx file into txt                            
                string fileLocation = Server.MapPath("/Documents");
                textConverter textConverter = new textConverter(fileLocation);
                textConverter.pdfConverted();
                textConverter.docxConverted();
                //end converting
                //Start indexing
                string pdfPath = Server.MapPath("/temp");
                string pagePath = Server.MapPath("/");
                string indexPath = Server.MapPath("/Index-lucene");
                DirectoryInfo indexInfo = new DirectoryInfo(indexPath);
                DirectoryInfo dataInfo = new DirectoryInfo(pdfPath);
                DirectoryInfo dataInfo1 = new DirectoryInfo(pagePath);
                Lucene.Net.Store.Directory indexDir = FSDirectory.Open(
                                                                 indexInfo, _LockFactory);
                //Delete previous index first
                DeleteIndex(indexInfo, indexPath);
                //Generate new index
                var numIndexed = FileIndex(indexDir, dataInfo, dataInfo1);
                //end indexing
                ViewBag.numIndexed += "Number of file indexed: " + numIndexed;
            }
            return View();
        }

        private void DeleteIndex(DirectoryInfo indexInfo, string indexPath)
        {
            try
            {
                var paths = indexInfo.EnumerateFiles("*");
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        string file = path.ToString();
                        string filepath = Path.Combine(indexPath, file);
                        System.IO.File.Delete(filepath);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static int FileIndex(Lucene.Net.Store.Directory indexDir, DirectoryInfo dataInfo, DirectoryInfo dataInfo1)
        {
            Analyzer anlalyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            var writer = new IndexWriter(indexDir, anlalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            writer.SetMergePolicy(new LogDocMergePolicy(writer));
            //writer.SetMergeFactor(5);
            try
            {
                string[] searchPatterns = { "*.cshtml", "*.aspx", "*.html", "*.php" };
                var paths1 = searchPatterns.AsParallel().SelectMany(searchPattern => dataInfo1.EnumerateFiles(searchPattern, SearchOption.AllDirectories));
                foreach (var path in paths1)
                {
                    string filename = path.Name;
                    //Do not index _ViewStart and _Layout page
                    if (filename != "_ViewStart.cshtml" && filename != "_Layout.cshtml" && filename != "path.txt")
                    {
                        IndexFile(writer, path);
                    }
                }
            }
            catch (Exception ex)
            {
                writer.Dispose();
                throw ex;
            }
            try
            {
                var paths = dataInfo.EnumerateFiles("*.txt", SearchOption.AllDirectories);
                foreach (var path in paths)
                {
                    if (path.Name != "path.txt")
                    {
                        IndexFile(writer, path);
                    }
                }
            }
            catch (Exception ex)
            {
                writer.Dispose();
                throw ex;
            }
            var numIndexed = writer.MaxDoc();
            writer.Dispose();
            //delete temp created file after indexing
            deleteFiles(dataInfo);
            return numIndexed;
        }

        private static void IndexFile(IndexWriter writer, FileInfo file)
        {
            if (!file.Exists)
            {
                return;
            }
            Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
            string path = file.FullName;
            string folder = file.DirectoryName;
            string final_folder = Regex.Replace(folder, "temp", " ");
            TextReader readFile = new StreamReader(path);
            string temp_fileName = file.Name;
            string fileName = Regex.Replace(temp_fileName, ".txt", "");
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string test_content = "";
            string check_extention = Path.GetExtension(temp_fileName);
            var content = System.IO.File.ReadAllText(path);
            var test_content1 = string.Empty;
            string metaContent = string.Empty;
            string temp_relativePath = string.Empty;
            string final_relativePath = string.Empty;
            if (check_extention == ".cshtml" || check_extention == ".aspx" || check_extention == ".html" || check_extention == ".php")
            {
                relativePath relativepath = new relativePath(path);
                temp_relativePath = relativepath.getRelativePath(folder);
                final_relativePath = Regex.Replace(temp_relativePath, ".txt", "");
                if (content != null)
                {
                    string plainText = Regex.Replace(content, @"(<script\b[^>]*>(.*?)<\/script>)|(<style\b[^>]*>(.*?)<\/style>)", "", RegexOptions.Singleline); //remove javascript and css
                    string plainTextfromPageTemp = Regex.Replace(plainText, "<[^>]+?>|&\\w+;", ""); //remove HTML
                    if (check_extention == ".cshtml")
                    {
                        string temp1PlainTextfromCSHTML = Regex.Replace(plainTextfromPageTemp, @"[@]\{([^\)]+)\}", ""); //remove all text betwwen @{ }
                        string temp2PlainTextfromCSHTML = Regex.Replace(temp1PlainTextfromCSHTML, @"[@].*", ""); //remove line starting with @
                        plainTextfromPageTemp = Regex.Replace(temp2PlainTextfromCSHTML, @"\{([^\)]+)\}", ""); //remove all text betwwen { }
                    }
                    string plainTextfromPage1 = Regex.Replace(plainTextfromPageTemp, @"(\t|\n|\r)", " ");//remove blank lines
                    string plainTextfromPage = Regex.Replace(plainTextfromPage1, @"[ ]{2,}", " ");//remove unwanted spaces
                    test_content = test_content + " " + plainTextfromPage;
                    //collecting contents of meta tags
                    metaContent = GetMetaTags(path);
                }
            }
            else
            {
                //Assigning original path
                pathList pathlist = new pathList();
                string[] listofpath = pathlist.readPath();
                foreach (string pathlists in listofpath)
                {
                    if (pathlists != string.Empty)
                    {
                        DirectoryInfo indexInfo = new DirectoryInfo(pathlists);
                        string fileNameOnly = indexInfo.Name;
                        if (fileName == fileNameOnly)
                        {
                            relativePath relativepath = new relativePath(pathlists);
                            final_relativePath = relativepath.getRelativePath(folder);
                            break;
                        }
                    }
                }
                //
                test_content = test_content + " " + content;
            }
            //doc.Add(new Field("contents", readFile));
            doc.Add(new Field("prevewContent", test_content, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            doc.Add(new Field("URL", final_relativePath, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            doc.Add(new Field("filename", fileName, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            doc.Add(new Field("fileNameWithoutExtension", fileNameWithoutExtension, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            doc.Add(new Field("metaTag", metaContent, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            writer.AddDocument(doc);
            writer.Optimize();
            writer.Flush(true, true, true);
        }

        private static string GetMetaTags(string path)
        {
            var webGet = new HtmlWeb();
            var document = webGet.Load(path);
            var metaTags = document.DocumentNode.SelectNodes("//meta");
            string metaDescription = string.Empty;
            if (metaTags != null)
            {
                foreach (var tag in metaTags)
                {
                    if (tag.Attributes["name"] != null && tag.Attributes["content"] != null)
                    {
                        if (tag.Attributes["name"].Value == "description" || tag.Attributes["name"].Value == "keywords")
                        {
                            metaDescription = metaDescription + " " + tag.Attributes["content"].Value;
                        }
                    }
                }
            }
            return metaDescription;
        }

        private static void deleteFiles(DirectoryInfo dataInfo)
        {
            var filePath = dataInfo.GetFiles();
            foreach (var file in filePath)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                System.IO.File.Delete(file.FullName);
            }
        }
    }
}