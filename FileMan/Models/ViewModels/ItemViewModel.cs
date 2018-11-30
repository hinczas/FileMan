using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Web.UI;
using System.Web.WebPages;

namespace FileMan.Models.ViewModels
{

    public class ItemViewModel
    {
        public Folder Current { get; set; }
        public List<Folder> ChildrenDirs { get; set; }
        public List<MasterFile> ChildrenFiles { get; set; }
        public List<Folder> Breadcrumbs { get; set; }


    }
}