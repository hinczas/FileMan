using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Web.UI;
using System.Web.WebPages;

namespace Raf.FileMan.Models.ViewModels
{

    public class ItemViewModel
    {
        public Folder Current { get; set; }
        public List<Folder> ChildrenDirs { get; set; }
        public List<MasterFile> ChildrenFiles { get; set; }
        public List<BreadItem> Breadcrumbs { get; set; }
        public List<MasterFile> UnassignedFiles { get; set; }
        public bool Favourite { get; set; }
        public long FavId { get; set; }
        public bool Search { get; set; }

        public List<FolderPartialViewModel> FolderList { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BreadItem
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public bool HasChildren { get; set; }
        public bool Current { get; set; }
        public List<BreadItem> Children { get; set; }
    }
}