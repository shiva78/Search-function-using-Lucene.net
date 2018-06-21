using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace Search_function.Models
{
    public class DocxToText
    {
        private const string ContentTypeNamespace =
            @"http://schemas.openxmlformats.org/package/2006/content-types";

        private const string WordprocessingXMlNamespace =
            @"http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        private const string DocumentXmlXPath =
            "/t:Types/t:Override[@ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"]";

        private const string BodyXPath = "/w:document/w:body";

        private string docxFile = "";
        private string docxFileLocation = "";


        public DocxToText(string fileName, string filePath)
        {
            docxFile = fileName;
            docxFileLocation = filePath;
        }

        /// Extracts text from the Docx file.
        public string ExtractText()
        {
            if (string.IsNullOrEmpty(docxFile))
                throw new Exception("Input file not specified.");
            if (string.IsNullOrEmpty(docxFileLocation))
                throw new Exception("It is not a valid Docx file.");

            return ReadDocumentXml();
        }
        //Returns text containing in the document.
        private string ReadDocumentXml()
        {
            StringBuilder sb = new StringBuilder();

            ZipFile zip = new ZipFile(docxFileLocation);
            foreach (ZipEntry entry in zip)
            {
                Stream documentXml = zip.GetInputStream(entry);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(documentXml);
                documentXml.Close();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("w", WordprocessingXMlNamespace);
                XmlNode node = xmlDoc.DocumentElement.SelectSingleNode(BodyXPath, nsmgr);
                sb.Append(ReadNode(node));
            }
            zip.Close();
            return sb.ToString();
        }

        // Reads content of the node and its nested childs.        
        // Returns Text containing in the node.
        private string ReadNode(XmlNode node)
        {
            if (node == null || node.NodeType != XmlNodeType.Element)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                switch (child.LocalName)
                {
                    case "t":                           // Text
                        sb.Append(child.InnerText.TrimEnd());
                        string space = ((XmlElement)child).GetAttribute("xml:space");
                        if (!string.IsNullOrEmpty(space) && space == "preserve")
                            sb.Append(' ');
                        break;
                    case "cr":                          // Carriage return
                    case "br":                          // Page break
                        sb.Append(Environment.NewLine);
                        break;
                    case "tab":                         // Tab
                        sb.Append("\t");
                        break;
                    case "p":                           // Paragraph
                        sb.Append(ReadNode(child));
                        sb.Append(Environment.NewLine);
                        sb.Append(Environment.NewLine);
                        break;
                    default:
                        sb.Append(ReadNode(child));
                        break;
                }
            }
            return sb.ToString();
        }
    }
}