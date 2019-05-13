using Raf.FileMan.Classes;

using Raf.FileMan.Models;
using Microsoft.AspNet.Identity;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Raf.FileMan.Context;
using System.Threading.Tasks;

namespace Raf.FileMan.Controllers
{
    [Authorize]
    public class FileRevisionsController : Controller
    {
        // GET: MasterFiles
        private ItemService _is;
        private AppDbContext _db;

        public FileRevisionsController()
        {
            _is = new ItemService();
            _db = new AppDbContext();
        }

        public ActionResult FileAction(long[] revisions, string action)
        {


            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Details (long? id)
        {
            FileRevision file = _db.FileRevision.Find(id);
            string userId = User.Identity.GetUserId();
            ApplicationUser user = _is.GetASPUser(userId);


            if (file == null)
            {
                return HttpNotFound();
            }
            else
            {
                //if (useDocu && DataFeeder.DocuCompatible(file.Extension))
                //{
                //    return View(file);
                //}
                //else
                //{
                    string mimeType = MimeTypeMap.GetMimeType(file.Extension);
                    string fileName = file.MasterFile.Number + "-" + file.Draft + "-" + file.MasterFile.Name + "-" + file.Name;
                    Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);

                    return File(file.FullPath, mimeType);
                //}                    
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MasterFileId,Revision,Name,Comment")] FileRevision item, HttpPostedFileBase file, long pid)
        {
            if (ModelState.IsValid)
            {
                //double tmp;
                var prev = _db.FileRevision.Where(a => a.MasterFileId == item.MasterFileId).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();
                string prefDraft = prev == null ? "" : prev.Draft;
                string draft = _is.Increment(prefDraft);

                var rootPath = _is.GetRoot().Path;

                if (file != null)
                {
                    // vars 
                    var actionDate = DateTime.Now;
                    string extension = Path.GetExtension(file.FileName).Replace(".", "").ToUpper();
                    var filname = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + "_v" + item.Revision +"."+ extension.ToLower();
                    string icon = DataFeeder.GetIcon(extension);
                    // MasterFile setup
                    MasterFile parent = _db.MasterFile.Find(item.MasterFileId);                    
                    var fullpath = Path.Combine(rootPath, parent.Number, filname);
                    var changelog = parent.Changelog + string.Format("{0} - Revision added : {1} \n", actionDate, filname);
                    parent.Changelog = changelog;
                    parent.Edited = actionDate;

                    //Revision setup
                    item.Added = actionDate;
                    item.Name = file.FileName;
                    item.FullPath = fullpath;
                    item.Draft = draft;
                    item.Type = "draft";
                    item.Extension = extension;
                    item.Icon = icon;
                    file.SaveAs(fullpath);

                    var md5 = MD5.Create();
                    string hash = _is.GetMd5Hash(md5, fullpath);
                    item.Md5hash = hash;

                    _db.FileRevision.Add(item);
                    _db.SaveChanges();

                    return Json(new { success = true, responseText = "Draft added", id = item.MasterFileId, parentId = pid }, JsonRequestBehavior.AllowGet);

                } else
                {
                    return Json(new { success = false, responseText = "File could not be loaded", id = item.MasterFileId, parentId = pid }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, responseText = "Invalid model", id = item.MasterFileId, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDraftCommentAsync(long id, string comment, long fid, long pid)
        {
            if (ModelState.IsValid)
            {
                var draft = await _db.FileRevision.FindAsync(id);
                
                if (draft != null)
                {
                    var parent = draft.MasterFile;
                    if (!Editable(parent.Id))
                    {
                        return Json(new { success = false, responseText = "Document locked by another user", id = fid, parentId = pid }, JsonRequestBehavior.AllowGet);
                    }

                    var date = DateTime.Now;

                    draft.Comment = comment;
                    parent.Changelog = parent.Changelog + string.Format("{0} - Draft [{1}] updated \n", date, draft.Draft);

                    await _db.SaveChangesAsync();

                    return Json(new { success = true, responseText = "Draft "+draft.Draft+" updated", id = fid, parentId = pid }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "Could not find the requested revision", id = fid, parentId = pid }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, responseText = "Invalid model", id = fid, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFile(long id)
        {
            FileRevision rev = _db.FileRevision.Find(id);

            string path = rev.FullPath;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = rev.MasterFile.Number + "-" + rev.Draft + "-" + rev.MasterFile.Name + "-" + rev.Name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

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