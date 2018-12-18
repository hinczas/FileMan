using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace FileMan.Controllers
{
    [Authorize]
    public class FoldersController : Controller
    {
        private ItemService _is;
        private DatabaseCtx _db;

        public FoldersController()
        {
            _is = new ItemService();
            _db = new DatabaseCtx();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Pid,Type,Name,Description,Comment")] Folder item)
        {
            if (ModelState.IsValid)
            {
                var added = DateTime.Now;
                var changelog = string.Format("{0} - Folder created \n", added);
                var parent = _db.Folder.Find(item.Pid);
                var parentPath = parent == null || parent.IsRoot ? "" : parent.Path;
                var path = Path.Combine(parentPath, item.Name);

                var changelogParent = string.Format("{0} - Subfolder created : {1} \n", added, item.Name);
                if (parent != null)
                {
                    string oldChng = parent.Changelog;
                    parent.Changelog = oldChng + changelogParent;
                }

                item.Parent = parent;
                item.Path = path;
                item.Added = added;
                item.Changelog = changelog;

                _db.Folder.Add(item);
                _db.SaveChanges();

                return Redirect(Request.UrlReferrer.ToString());
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        // GET: Folders/Delete/5
        public ActionResult Delete(int id)
        {
            Folder item = _db.Folder.Find(id);
            if (item != null)
            {

                int files = item.Files.Count();
                int folders = _db.Folder.Where(a => a.Pid == id).Count();

                if (files != 0 || folders != 0)
                {
                    TempData["Error"] = true;
                    TempData["Message"] = "Cannot delete non-empty directory.";

                } else
                {
                    _db.Folder.Remove(item);
                    _db.SaveChanges();
                }
                //_is.DeleteDirectory(item.Id);
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult MoveFiles(int Id, int[] files)
        {
            Folder item = _db.Folder.Find(Id);
            
            if (files==null)
                return Redirect(Request.UrlReferrer.ToString());

            foreach(int i in files)
            {
                MasterFile file = _db.MasterFile.Find(i);
                file.Changelog = file.Changelog + string.Format("{0} - Document category change \n", DateTime.Now);
                item.Files.Add(file);
            }
            _db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}
