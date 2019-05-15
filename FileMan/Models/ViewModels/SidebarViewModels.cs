using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Models.ViewModels
{
    public class UserSideBarVM
    {
        public List<LinkItem> Favs { get; set; }
        public List<LinkItem> Locks { get; set; }
    }

    public class LinkItem
    {
        public int Type { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public string Label { get; set; }
        public string Extra { get; set; }
    }
}