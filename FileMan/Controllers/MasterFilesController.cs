using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileMan.Controllers
{
    [Authorize]
    public class MasterFilesController : Controller
    {
        // GET: MasterFiles
        private ItemService _is;
        private DatabaseCtx _db;

        public MasterFilesController()
        {
            _is = new ItemService();
            _db = new DatabaseCtx();
        }

        // GET: MasterFiles/Details/5
        public ActionResult Details(int id)
        {

            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId);

            return View(file);
        }

        // GET: MasterFiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Description,Comment,Number")] MasterFile item, long FolderId, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                Folder parent = _db.Folder.Where(a=>a.Id == FolderId).FirstOrDefault();

                var added = DateTime.Now;
                var changelog = string.Format("{0} - Item created \n", added);
                var changelogParent = string.Format("{0} - File created : {1} \n", added, item.Name);
                string path = "";
                if (parent != null)
                {
                    string oldChng = parent.Changelog;
                    parent.Changelog = oldChng + changelogParent;
                    path = parent.Type.Equals("root") ? "" : parent.Path;
                }


                string number = item.GetHashCode().ToString();
                item.Number = number;

                var rootPath = _db.Folder.Where(a=> a.Type.Equals("root")).FirstOrDefault().Path;
                path = Path.Combine(path, number);
                string extension = "";

                Directory.CreateDirectory(Path.Combine(rootPath, number));

                item.Path = path;
                item.Added = added;
                item.Changelog = changelog;
                item.Extension = extension;
                item.Folders = new List<Folder>() {
                    parent
                };
                _db.MasterFile.Add(item);
                _db.SaveChanges();

                if (file != null)
                {
                    extension = Path.GetExtension(file.FileName).Replace(".", "").ToUpper();
                    var revision = 1;
                    var filname = System.IO.Path.GetFileNameWithoutExtension(file.FileName)+"_v"+revision+"."+extension.ToLower();
                    var fullpath = Path.Combine(rootPath, number, filname);
                    string draft = _is.Increment(string.Empty);

                    FileRevision rf = new FileRevision()
                    {
                        MasterFileId = item.Id,
                        Revision = revision,
                        Name = file.FileName,
                        Comment = item.Comment,
                        FullPath = fullpath,
                        Draft = draft,
                        Type = "draft",
                        Added = added
                    };

                    file.SaveAs(fullpath);
                    item.Extension = extension;
                    item.Changelog = item.Changelog + string.Format("{0} - Revision added : {1} \n", added, filname);

                    _db.FileRevision.Add(rf);
                    _db.SaveChanges();

                }

                return Redirect(Request.UrlReferrer.ToString());
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        // GET: MasterFiles/Edit/5
        public ActionResult Edit(int id)
        {
            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId);

            return View(file);
        }

        // POST: MasterFiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Comment,Number")] MasterFile item)
        {
            if (ModelState.IsValid)
            {
                var date = DateTime.Now;
                MasterFile mf = _db.MasterFile.Find(item.Id);
                mf.Name = item.Name;
                mf.Description = item.Description;
                mf.Comment = item.Comment;
                mf.Edited = date;
                mf.Changelog = mf.Changelog + string.Format("{0} - Document edited \n", date);

                _db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = item.Id });
        }

        // POST: MasterFiles/Delete/5
        public ActionResult Delete(int id, int folderId)
        {
            MasterFile item = _db.MasterFile.Find(id);
            Folder folder = _db.Folder.Find(folderId);
            item.Folders.Remove(folder);
            _db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Promote(long id, string Comment)
        {
            MasterFile item = _db.MasterFile.Find(id);
            if (item != null)
            {
                long curIssue = item.Issue == null ? 0 : (long)item.Issue;
                long nextIssue = curIssue + 1;
                long issue = nextIssue;

                FileRevision fr = _db.FileRevision.Where(a => a.MasterFileId == id).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();

                item.Issue = issue;
                item.Changelog = item.Changelog + string.Format("{0} - Promoted to Issue : {1} \n", DateTime.Now, issue);
                if (!string.IsNullOrEmpty(Comment))
                {
                    item.Comment = item.Comment + 
                                    "\n" + 
                                    Comment;
                }
                if (fr != null)
                {
                    fr.Draft = issue.ToString();
                    fr.Type = "issue";
                }

                _db.SaveChanges();
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult MoveFile(long Id, long[] folders)
        {
            MasterFile master = _db.MasterFile.Find(Id);
            var assigned = master.Folders.Select(a => a.Id).ToArray();


            if (folders==null || folders.Count()==0)
            {
                master.Folders = new List<Folder>();
                master.Changelog = master.Changelog + string.Format("{0} - Uncategorised \n", DateTime.Now);
                _db.SaveChanges();
                return Redirect(Request.UrlReferrer.ToString());
            }

            var toAdd = folders.Except(assigned);
            var toDel = assigned.Except(folders);


            foreach (int i in toDel)
            {
                Folder folder = _db.Folder.Find(i);
                master.Folders.Remove(folder);
            }
            _db.SaveChanges();

            foreach (int i in toAdd)
            {
                Folder folder = _db.Folder.Find(i);
                master.Folders.Add(folder);
            }

            master.Changelog = master.Changelog + string.Format("{0} - Document category change \n", DateTime.Now);
            _db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}
