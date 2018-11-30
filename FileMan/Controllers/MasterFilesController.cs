using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
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
            MasterFileViewModel file = _is.GetMasterFileViewModel(id);

            return View(file);
        }

        // GET: MasterFiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FolderId,Name,Description,Comment,Number")] MasterFile item, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                Folder parent = _db.Folder.Find(item.FolderId);

                var added = DateTime.Now;
                var changelog = string.Format("{0} - Item created \n", added);
                var changelogParent = string.Format("{0} - File created : {1} \n", added, item.Name);
                if (parent!=null)
                {
                    string oldChng = parent.Changelog;
                    parent.Changelog = oldChng + changelogParent;
                }
                 

                var rootPath = _db.Folder.Where(a=> a.Type.Equals("root")).FirstOrDefault().Path;
                var path = Path.Combine(rootPath, item.Name);
                string extension = "";
                Directory.CreateDirectory(path);

                item.Folder = parent;
                item.Path = path;
                item.Added = added;
                item.Changelog = changelog;
                item.Extension = extension;
                string number = item.GetHashCode().ToString();
                item.Number = number;

                _db.MasterFile.Add(item);
                _db.SaveChanges();

                if (file != null)
                {
                    extension = Path.GetExtension(file.FileName);
                    var revision = 1;
                    var filname = System.IO.Path.GetFileNameWithoutExtension(file.FileName)+"_v"+revision+extension;
                    var fullpath = Path.Combine(path, filname);

                    FileRevision rf = new FileRevision()
                    {
                        MasterFileId = item.Id,
                        Revision = revision,
                        Name = file.FileName,
                        FullPath = fullpath,
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
            return View();
        }

        // POST: MasterFiles/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // POST: MasterFiles/Delete/5
        public ActionResult Delete(int id)
        {
            MasterFile item = _db.MasterFile.Find(id);
            if (item != null)
            {
                _is.DeleteFile(item.Id);
            }

            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}
