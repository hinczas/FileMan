using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpPost]
        public async Task<bool> Move([Bind(Include ="Id,OldParId,NewParId")] FolderMovelViewModel model)
        {
            if (!ModelState.IsValid)
                return false;

            //long c, o, n;

            //if (string.IsNullOrEmpty(model.Id) || string.IsNullOrEmpty(oldP) || string.IsNullOrEmpty(newP))
            //    return false;

            //bool cb = long.TryParse(currN, out c);
            //bool ob = long.TryParse(oldP, out o);
            //bool nb = long.TryParse(newP, out n);

            //if (!cb || !ob || !nb)
            //    return false;
            
            var oldParent = _db.Folder.Find(model.OldParId);
            var current = _db.Folder.Find(model.Id);
            var newParent = _db.Folder.Find(model.NewParId);

            if (oldParent == null || current == null || newParent == null)
                return false;

            string oldParentPath = oldParent.Path;
            string newParentPath = newParent.IsRoot ? "" : newParent.Path;

            current.Parent = newParent;
            current.Pid = newParent.Id;
            await _db.SaveChangesAsync();

            await UpdatePathAsync(model.Id, newParentPath);

            return true;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ImportFiles(HttpPostedFileBase fileImp, string fileType, long? intoCurrent)
        {
            if (string.IsNullOrEmpty(fileType))
                return Redirect(Request.UrlReferrer.ToString());
                       
            if (fileImp!=null)
            {
                FileService _fs = new FileService();

                if (fileType.Equals("dir"))
                {
                    await _fs.ImportCatsAsync(fileImp, fileType, intoCurrent);
                }

                if (fileType.Equals("doc"))
                {
                    await _fs.ImportDocsAsync(fileImp, fileType, intoCurrent);
                }
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        private async Task UpdatePathAsync(long id, string newPath)
        {
            var folder = _db.Folder.Find(id);
            string path = Path.Combine(newPath, folder.Name);

            folder.Path = path;
            await _db.SaveChangesAsync();

            foreach (var ch in folder.Children)
                await UpdatePathAsync(ch.Id, path);
        }
    }
}
