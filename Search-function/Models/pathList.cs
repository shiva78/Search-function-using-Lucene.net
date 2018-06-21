using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Search_function.Models
{
    public class pathList
    {
        string pathTxt = string.Empty;
        public pathList()
        {
            pathTxt = HttpContext.Current.Server.MapPath("/temp/path.txt");
        }

        public string[] readPath()
        {
            string readPathTxt = File.ReadAllText(pathTxt);
            char[] seperator = new char[] { ',' };
            string[] listOfPaths = readPathTxt.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            return listOfPaths;
        }
    }
}