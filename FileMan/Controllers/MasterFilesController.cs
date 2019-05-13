﻿using Raf.FileMan.Classes;

using Raf.FileMan.Models;
using Raf.FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Raf.FileMan.Context;

namespace Raf.FileMan.Controllers
{
    [Authorize]
    public class MasterFilesController : Controller
    {
        // GET: MasterFiles
        private ItemService _is;
        private AppDbContext _db;

        public MasterFilesController()
        {
            _is = new ItemService();
            _db = new AppDbContext();
        }

        // GET: MasterFiles/Details/5
        public ActionResult Details(int id, long? pid)
        {
            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("file", file.RedirectId, id, string.Empty, null, "file", id);

            return View(file);
        }

        public PartialViewResult PartialDetails(int id, long? pid)
        {

            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("file", file.RedirectId, id, string.Empty, null, "file", id);

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
                
                string userId = User.Identity.GetUserId();
                var user = _db.Users.Find(userId);

                item.Added = added;
                item.Changelog = changelog;
                item.User = user;
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

                return Json(new { success = true, responseText = "New document created", parentId = FolderId }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, responseText = "Invalid model state", parentId = FolderId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> UserLock(long id, bool isLocked)
        {
            MasterFile item = await _db.MasterFile.FindAsync(id);

            if (item==null)
                return Json(new { success = false, responseText = "Cannot find specified Document" }, JsonRequestBehavior.AllowGet);

            var userId = User.Identity.GetUserId();
            var user = _db.Users.Find(userId);

            if (item.Locked && !item.UserLock.Equals(userId))
            {
                var lUser = _db.Users.Find(item.UserLock);
                string name = lUser == null ? "unknown" : string.Format("{0}, {1}", lUser.Surname, lUser.FirstName);
                return Json(new { success = false, responseText = "Document locked by another user ("+name+")" }, JsonRequestBehavior.AllowGet);
            }

            if (item.Locked && isLocked)
                return Json(new { success = false, responseText = "Document already locked out" }, JsonRequestBehavior.AllowGet);

            if (!item.Locked && !isLocked)
                return Json(new { success = false, responseText = "Document already unlocked" }, JsonRequestBehavior.AllowGet);

            var date = DateTime.Now;
            if (isLocked)
            {
                item.UserLock = userId;
                item.Changelog = item.Changelog + string.Format("{0} - Document locked by {1}, {2} \n", date, user.Surname, user.FirstName);
                await _db.SaveChangesAsync();
                return Json(new { success = true, responseText = string.Format("{0} locked by {1}, {2}", item.Number, user.Surname, user.FirstName) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                item.UserLock = null;
                item.Changelog = item.Changelog + string.Format("{0} - Document unlocked by {1}, {2} \n", date, user.Surname, user.FirstName);
                await _db.SaveChangesAsync();
                return Json(new { success = true, responseText = string.Format("{0} unlocked by {1}, {2}", item.Number, user.Surname, user.FirstName) }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterFiles/Edit/5
        public ActionResult Edit(int id, long? pid)
        {
            if (!Editable(id))
            {
                return RedirectToAction("Details", new { id, pid });
            }
            string userId = User.Identity.GetUserId();
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            Session["SessionState"] = new SessionState("edit", (long)pid, id, string.Empty, null, "edit", id);

            return View(file);
        }

        public ActionResult PartialEdit(int id, long? pid)
        {
            if (!Editable(id))
            {
                return RedirectToAction("PartialDetails", new { id, pid });
            }
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
                if (!Editable(item.Id))
                    return Json(new { success = false, responseText = "Document locked by another user", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);

                var date = DateTime.Now;
                MasterFile mf = _db.MasterFile.Find(item.Id);
                mf.Name = item.Name;
                mf.Description = item.Description;
                mf.Comment = item.Comment;
                mf.Edited = date;
                mf.Changelog = mf.Changelog + string.Format("{0} - Document edited \n", date);

                _db.SaveChanges();
                
                return Json(new { success = true, responseText = item.Number + " document updated", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, responseText = "Cannot update document "+item.Number + ". Data issue.", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        // POST: MasterFiles/Delete/5
        [HttpPost]
        public async Task<ActionResult> DeleteAsync(int id, int folderId)
        {
            try
            {
                MasterFile item =await _db.MasterFile.FindAsync(id);
                if (!Editable(id))
                {
                    return Json(new { success = false, responseText = "Document locked by another user", id = id, parentId = folderId }, JsonRequestBehavior.AllowGet);
                }

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

                return Json(new { success = true, responseText = item.Number + " document removed", parentId = folderId }, JsonRequestBehavior.AllowGet);
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

            if (!Editable(id))
            {
                return Json(new { success = false, responseText = "Document locked by another user", id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
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
                    string comm = string.IsNullOrEmpty(item.Comment) ? Comment : item.Comment + "\n" + Comment;
                    item.Comment = comm;
                }
                if (fr != null)
                {
                    fr.Draft = issue.ToString();
                    fr.Type = "issue";
                }

                _db.SaveChanges();
            } else
            {
                return Json(new { success = false, responseText = "Could not find document "+id, id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, responseText = item.Number + " document promoted", id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MoveFile(long Id, long[] folders, long pid)
        {
            try {
                MasterFile master = _db.MasterFile.Find(Id);
                if (!Editable(Id))
                {
                    return Json(new { success = false, responseText = "Document locked by another user", id = Id, parentId = pid }, JsonRequestBehavior.AllowGet);
                }

                var assigned = master.Folders.Select(a => a.Id).ToArray();

                var affected = folders==null ? assigned : assigned.Union(folders).ToArray();

                if (folders == null || folders.Count() == 0)
                {
                    master.Folders = new List<Folder>();
                    master.Changelog = master.Changelog + string.Format("{0} - Uncategorised \n", DateTime.Now);
                    _db.SaveChanges();
                    return Json(new { success = true, responseText = master.Number + " document uncategorised", id = Id, parentId = pid, affFolIds = affected }, JsonRequestBehavior.AllowGet);
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

                return Json(new { success = true, responseText = master.Number + " document updated", id = Id, parentId = pid, affFolIds = affected }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.Message, id = Id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            
        }
               
        public async Task<ActionResult> MoveFileAsync(long Id, long opid, long npid)
        {
            try
            {
                MasterFile master = _db.MasterFile.Find(Id);
                Folder oldPa = _db.Folder.Find(opid);
                Folder newPa = _db.Folder.Find(npid);
                if (!Editable(Id))
                {
                    return Json(new { success = false, responseText = "Document locked by another user", id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);
                }

                if (master==null)
                    return Json(new { success = false, responseText = "Cannot find source document "+Id, id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);

                if (oldPa == null)
                    return Json(new { success = false, responseText = "Cannot find source category "+opid, id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);

                if (newPa == null)
                    return Json(new { success = false, responseText = "Cannot find target category "+npid, id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);


                if (master.Folders==null)
                {
                    master.Folders = new List<Folder>();
                    await _db.SaveChangesAsync();
                }

                master.Folders.Remove(oldPa);
                master.Folders.Add(newPa);
                await _db.SaveChangesAsync();
                return Json(new { success = true, responseText = master.Number+" document moved", id = Id, parentId = npid, affFolIds = new long[2] { opid, npid} }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.Message, id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);
            }

        }

        private bool Editable(long id)
        {
            MasterFile master = _db.MasterFile.Find(id);
            var userId = User.Identity.GetUserId();
            if (master.Locked && !master.UserLock.Equals(userId))
            {
                return false;
            }
            return true;
        }
    }
}
