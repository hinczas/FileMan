using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using Microsoft.AspNet.Identity;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace FileMan.Controllers
{
    [Authorize]
    public class FileRevisionsController : Controller
    {
        // GET: MasterFiles
        private ItemService _is;
        private DatabaseCtx _db;

        public FileRevisionsController()
        {
            _is = new ItemService();
            _db = new DatabaseCtx();
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
            bool useDocu = user.UserSetting.UseDocuViewer;


            if (file == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (useDocu && DataFeeder.DocuCompatible(file.Extension))
                {
                    return View(file);
                }
                else
                {
                    string mimeType = MimeTypeMap.GetMimeType(file.Extension);
                    string fileName = file.MasterFile.Number + "-" + file.Draft + "-" + file.MasterFile.Name + "-" + file.Name;
                    Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);

                    return File(file.FullPath, mimeType);
                }                    
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MasterFileId,Revision,Name,Comment")] FileRevision item, HttpPostedFileBase file)
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

                }

                return Redirect(Request.UrlReferrer.ToString());
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult GetFile(long id)
        {
            FileRevision rev = _db.FileRevision.Find(id);

            string path = rev.FullPath;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = rev.MasterFile.Number + "-" + rev.Draft + "-" + rev.MasterFile.Name + "-" + rev.Name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

        }
    }
}