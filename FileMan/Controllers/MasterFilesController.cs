using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
        public ActionResult Details(int id, long? pid)
        {

            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("file", (long)pid, id, string.Empty, null, "file", id);

            return View(file);
        }

        public PartialViewResult PartialDetails(int id, long? pid)
        {

            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("file", (long)pid, id, string.Empty, null, "file", id);

            return PartialView(file);
        }

        // GET: MasterFiles/Create
        //, HttpPostedFileBase file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Description,Comment,Number")] MasterFile item, long FolderId)
        {
            if (ModelState.IsValid)
            {
                Folder parent = _db.Folder.Where(a=>a.Id == FolderId).FirstOrDefault();

                var added = DateTime.Now;
                var changelog = string.Format("{0} - Item created \n", added);


                item.Added = added;
                item.Changelog = changelog;
                _db.MasterFile.Add(item);
                _db.SaveChanges();


                var changelogParent = string.Format("{0} - File created : {1} \n", added, item.Name);
                string path = "";
                if (parent != null)
                {
                    string oldChng = parent.Changelog;
                    parent.Changelog = oldChng + changelogParent;
                    path = parent.IsRoot ? "" : parent.Path;
                }


                string number = item.Id.ToString().PadLeft(9, '0');

                item.Number = number;

                var rootPath = _is.GetRoot().Path;
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
                //_db.MasterFile.Add(item);
                _db.SaveChanges();

                //if (file != null)
                //{
                //    extension = Path.GetExtension(file.FileName).Replace(".", "").ToUpper();
                //    string icon = DataFeeder.GetIcon(extension);
                //    var revision = 1;
                //    var filname = System.IO.Path.GetFileNameWithoutExtension(file.FileName)+"_v"+revision+"."+extension.ToLower();
                //    var fullpath = Path.Combine(rootPath, number, filname);
                //    string draft = _is.Increment(string.Empty);

                //    FileRevision rf = new FileRevision()
                //    {
                //        MasterFileId = item.Id,
                //        Revision = revision,
                //        Name = file.FileName,
                //        Comment = item.Comment,
                //        FullPath = fullpath,
                //        Extension = extension,
                //        Draft = draft,
                //        Type = "draft",
                //        Added = added,
                //        Icon = icon
                //    };


                //    file.SaveAs(fullpath);

                //    var md5 = MD5.Create();
                //    string hash = _is.GetMd5Hash(md5,fullpath);
                //    rf.Md5hash = hash;

                //    item.Extension = extension;
                //    item.Changelog = item.Changelog + string.Format("{0} - Revision added : {1} \n", added, filname);

                //    _db.FileRevision.Add(rf);
                //    _db.SaveChanges();
                //}

                return Json(new { success = true, responseText = "Document created", parentId = FolderId }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, responseText = "Invalid model state", parentId = FolderId }, JsonRequestBehavior.AllowGet);
        }

        // GET: MasterFiles/Edit/5
        public ActionResult Edit(int id, long? pid)
        {
            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("edit", (long)pid, id, string.Empty, null, "edit", id);

            return View(file);
        }

        public ActionResult PartialEdit(int id, long? pid)
        {
            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("edit", (long)pid, id, string.Empty, null, "edit", id);

            return PartialView(file);
        }

        // POST: MasterFiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Comment,Number")] MasterFile item, long pid)
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
                
                return Json(new { success = true, responseText = "Document updated", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, responseText = "Cannot update Document. Data issue.", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        // POST: MasterFiles/Delete/5
        [HttpPost]
        public async Task<ActionResult> DeleteAsync(int id, int folderId)
        {
            try
            {
                MasterFile item =await _db.MasterFile.FindAsync(id);
                if (folderId < 0)
                {
                    var fols = item.Folders.Select(s => s.Id).ToArray();
                    foreach(int fol in fols)
                    {
                        Folder folder = _db.Folder.Find(fol);
                        item.Folders.Remove(folder);
                        await _db.SaveChangesAsync();
                    }
                } else
                {
                    Folder folder = _db.Folder.Find(folderId);
                    item.Folders.Remove(folder);
                    await _db.SaveChangesAsync();
                }

                return Json(new { success = true, responseText = "Document removed", parentId = folderId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.Message, parentId = folderId }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Promote(long id, string Comment, long pid)
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
            } else
            {
                return Json(new { success = false, responseText = "Could not fid document", id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, responseText = "Document promoted", id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MoveFile(long Id, long[] folders, long pid)
        {
            try {
                MasterFile master = _db.MasterFile.Find(Id);
                var assigned = master.Folders.Select(a => a.Id).ToArray();

                var affected = folders==null ? assigned : assigned.Union(folders).ToArray();

                if (folders == null || folders.Count() == 0)
                {
                    master.Folders = new List<Folder>();
                    master.Changelog = master.Changelog + string.Format("{0} - Uncategorised \n", DateTime.Now);
                    _db.SaveChanges();
                    return Json(new { success = true, responseText = "Document uncategorised", id = Id, parentId = pid, affFolIds = affected }, JsonRequestBehavior.AllowGet);
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

                return Json(new { success = true, responseText = "Document updated", id = Id, parentId = pid, affFolIds = affected }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.Message, id = Id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            
        }

    }
}
