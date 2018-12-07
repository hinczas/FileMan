using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (file == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (DataFeeder.DocuCompatible(file.Extension))
                {
                    return View(file);
                } else
                {
                    string mimeType = MimeTypeMap.GetMimeType(file.Extension);
                    Response.AddHeader("Content-Disposition", "inline; filename=" + file.Name);

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
                double tmp;
                var prev = _db.FileRevision.Where(a => a.MasterFileId == item.MasterFileId).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();
                string prefDraft = prev == null ? "" : prev.Draft;
                string draft = _is.Increment(prefDraft);

                var rootPath = _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault().Path;

                if (file != null)
                {
                    // vars 
                    var actionDate = DateTime.Now;
                    string extension = Path.GetExtension(file.FileName).Replace(".", "").ToUpper();
                    var filname = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + "_v" + item.Revision +"."+ extension.ToLower();

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
                    file.SaveAs(fullpath);



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
            string fileName = rev.Name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

        }
    }
}