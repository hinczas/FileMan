using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Models.ViewModels
{
    public class FolderPartialViewModel
    {
        public Int64 Id { get; set; }        
        public string Name { get; set; }
        public string Path { get; set; }
    }
}