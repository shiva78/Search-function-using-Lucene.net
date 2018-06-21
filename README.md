# Search function using Lucene.net
Search text within web pages and files including pdf, docx and txt of a project using Lucene.net in a ASP.NET MVC project.

Packages used (you can install into your project through NuGet Package Manager)
1. Lucene.Net v3.0.3
2. Lucene.Net.Contrib v3.0.3
3. HtmlAgilityPack v1.8.4
4. iTextSharp v5.5.13 (To read pdf file)

Lucene indexer generates index file of the documents and web pages which is then can be readable by Lucene searcher.
- I'm updating index everytime while uploading documents (deleting existing index and generating new ones).
- Meta tag is also collected while indexing

By the help of Lucene searcher, we can search files/web pages (which were indexed) by passing search text.
In this project
- I'm displaying the search result in a descending order according to "search text score".
- Highlighting the search text
- Displaying title of a document/web page
- Displaying prevew content
- Linking search result to the reletated document/web page

It is a fully functional and tested web project, if you would like to see how it works then add it into your project. You can pass search text as "test" to see the result (as it already has index file).
Try to upload some pdf and docx (note: this project is only indexing ".docx" version of word file format for now) file and then search the word from those files.

P.S. I've used Visual Studio 2017.