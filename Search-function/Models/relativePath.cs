using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Search_function.Models
{
    public class relativePath
    {
        private string path = string.Empty;
        public relativePath(string paths)
        {
            path = paths;
        }
        public string getRelativePath(string folder)
        {
            //string final_folder = System.Text.RegularExpressions.Regex.Replace(folder, "temp", " ");
            string final_folder = string.Empty;
            //if condition to remove Views folder name as MVC has all views files within Views folder and egnores the folder name while browsing
            if (folder.Contains("\\Views\\"))
            {
                final_folder = Regex.Replace(folder, @"Views\\.*", @"Views\\");
                //remove extension as MVC project rejects .cshtml file extension while browsing
                path = Regex.Replace(path, @".cshtml", "");
            }
            else
            {
                final_folder = Regex.Replace(folder, @"Search-function\\.*", @"Search-function\\");
            }            
            Uri pathUri = new Uri(path);
            if (!final_folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                final_folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(final_folder);
            string temp_relativePath = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
            //add ..\\ if missing to get into root directory
            if (!temp_relativePath.Contains(".."))
            {
                temp_relativePath = "..\\" + temp_relativePath;
            }
            //string relativePath = Regex.Replace(temp_relativePath, @"\.\.", "");
            return temp_relativePath;
        }
    }
}