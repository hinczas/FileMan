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

    public class MasterFileViewModel
    {
        public MasterFile Current { get; set; }
        public List<Folder> Breadcrumbs { get; set; }
        public long LatestRevision { get; set; }
        public long LatestRevisionId { get; set; }
        public string LatestRevisionComm { get; set; }
        public int RevisionsCount { get; set; }
        public string RevisionName { get; set; }
        public string DraftVersion { get; set; }
        public FileRevision LatestIssue { get; set; }
        public FileRevision LatestDraft { get; set; }
        public List<FolderPartialViewModel> FolderList { get; set; }
        public bool Promote { get; set; }
        public bool ShowChangelog { get; set; }
        public long RedirectId { get; set; }
        public string RedirectLabel { get; set; }
        public string RedirectFun { get; set; }
        public string Author { get; set; }
        public bool Locked { get; set; }
        public string LockedBy { get; set; }
        public bool Lockable { get; set; }
    }
}