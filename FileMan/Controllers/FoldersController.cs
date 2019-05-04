using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using Newtonsoft.Json;
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
                string[] names = item.Name.Trim().Trim(';').Split(';').ToArray();
                var folders = new List<FolderJsonViewModel>();
                foreach(string n in names)
                {
                    string name = n.Trim();

                    var added = DateTime.Now;
                    var changelog = string.Format("{0} - Folder created \n", added);
                    var parent = _db.Folder.Find(item.Pid);
                    var parentPath = parent == null || parent.IsRoot ? "" : parent.Path;
                    var path = Path.Combine(parentPath, name);

                    var changelogParent = string.Format("{0} - Subfolder created : {1} \n", added, name);
                    if (parent != null)
                    {
                        string oldChng = parent.Changelog;
                        parent.Changelog = oldChng + changelogParent;
                    }

                    item.Name = name;
                    item.Parent = parent;
                    item.Path = path;
                    item.Added = added;
                    item.Changelog = changelog;

                    _db.Folder.Add(item);
                    _db.SaveChanges();

                    folders.Add(new FolderJsonViewModel() { Id = item.Id, Name = item.Name });
                }

                //var convertedJson = JsonConvert.SerializeObject(folders, Formatting.Indented);

                return Json(new { success = true, responseText = "Node successfully added", name = item.Name, id = item.Id, parentId = item.Pid, folders = folders }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, responseText = "Invalid data passed", reload = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<bool> Move([Bind(Include ="Id,OldParId,NewParId")] FolderMovelViewModel model)
        {
            if (!ModelState.IsValid)
                return false;
                        
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

        [HttpPost]
        public async Task<JsonResult> Rename(string name, long id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false, responseText = "name cannot be empty", id = id, parentId = id }, JsonRequestBehavior.AllowGet);
            }

            var folder = _db.Folder.Find(id);

            if (folder==null)
            {
                return Json(new { success = false, responseText = "Cannot find category", id = id, parentId = id }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                string newName = name.Trim();
                string newPath = ReplaceLastOccurrence(folder.Path, folder.Name, "");
                folder.Name = newName;
                //folder.Path = newPath;
                await _db.SaveChangesAsync();

                if (!folder.IsRoot)
                {
                    await UpdatePathAsync(id, newPath);
                }

                return Json(new { success = true, responseText = "Renamed ok", id = id, parentId = id, name = newName }, JsonRequestBehavior.AllowGet);
            } catch (Exception e)
            {
                return Json(new { success = false, responseText = e.Message, id = id, parentId = id }, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: Folders/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            Folder item = _db.Folder.Find(id);
            long? pid = item.Pid;
            if (item != null)
            {

                int files = item.Files.Count();
                int folders = _db.Folder.Where(a => a.Pid == id).Count();

                //if (files != 0 || folders != 0)
                //{
                //    TempData["Error"] = true;
                //    TempData["Message"] = "Cannot delete non-empty directory.";
                //    return Json(new { success = false, responseText = "Cannot delete non-empty directory." }, JsonRequestBehavior.AllowGet);

                //} else
                //{
                //_db.Folder.Remove(item);
                await DeleteChildCategoriesAsync(item.Id);
                //await _db.SaveChangesAsync();
                //}
                return Json(new { success = true, responseText = "Category deleted.", parentId = pid }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, responseText = "Error occured" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MoveFiles(int Id, int[] files)
        {
            Folder item = _db.Folder.Find(Id);
            
            if (files==null)
                return Json(new { success = false, responseText = "Empty list of document", id = Id, parentId = Id }, JsonRequestBehavior.AllowGet);

            foreach (int i in files)
            {
                MasterFile file = _db.MasterFile.Find(i);
                file.Changelog = file.Changelog + string.Format("{0} - Document category change \n", DateTime.Now);
                item.Files.Add(file);
            }
            _db.SaveChanges();

            return Json(new { success = true, responseText = "Documents moved", id = Id, parentId = Id }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public string GetChildCount(long id)
        {
            var fol = _db.Folder.Find(id);
            if (fol == null)
                return "";

            string ret = fol.Files == null || fol.Files.Count < 1 ? "" : string.Format("({0})", fol.Files.Count);

            return ret;
        }

        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        private async Task DeleteChildCategoriesAsync(long id)
        {
            var category = _db.Folder.Find(id);
            var children = category.Children.ToList();
            foreach (Folder subCategory in children)
            {
                if (subCategory.Children.Count() > 0)
                {
                    await DeleteChildCategoriesAsync(subCategory.Id);
                }
                else
                {
                    _db.Folder.Remove(subCategory);
                    await _db.SaveChangesAsync();
                }
            }
            _db.Folder.Remove(category);
            await _db.SaveChangesAsync();
        }
    }
}
