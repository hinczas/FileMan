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
                var parentPath = parent == null ? "" : parent.Path;
                var path = Path.Combine(parent.Path, item.Name);

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
                _is.DeleteDirectory(item.Id);
            }

            return Redirect(Request.UrlReferrer.ToString());
        }        
    }
}
