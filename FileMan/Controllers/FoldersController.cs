using Raf.FileMan.Classes;

using Raf.FileMan.Models;
using Raf.FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Raf.FileMan.Context;


namespace Raf.FileMan.Controllers
{
    [Authorize]
    public class FoldersController : Controller
    {
        private ItemService _is;
        private AppDbContext _db;
        private CategoryService _cs;
        private ApplicationUserManager _userManager;

        public FoldersController()
        {
            _is = new ItemService();
            _db = new AppDbContext();
            _cs = new CategoryService();
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Pid,Type,Name,Description,Comment")] Folder item)
        {
            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();

                // Create
                var result = await _cs.CreateAsync(item, userId);

                return Json(new { success = result.Success, responseText = result.Message, name = item.Name, id = item.Id, parentId = item.Pid, folders = (List<FolderJsonViewModel>)result.ExtraData }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, responseText = "Invalid data passed", reload = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> Move([Bind(Include ="Id,OldParId,NewParId")] FolderMovelViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, responseText = "Invalid model state" }, JsonRequestBehavior.AllowGet);

            // Move
            var result = await _cs.MoveAsync(model);

            return Json(new { success = result.Success, responseText = result.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> Rename(string name, long id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false, responseText = "Name cannot be empty", id = id, parentId = id }, JsonRequestBehavior.AllowGet);
            }

            // Rename
            var result = await _cs.RenameAsync(name, id);

            return Json(new { success = result.Success, responseText = result.Message, id = id, parentId = id, name = (string)result.ExtraData }, JsonRequestBehavior.AllowGet);

        }
            
        // GET: Folders/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var userId = User.Identity.GetUserId();

            // Delete
            var results = await _cs.DeleteAsync(id, userId);

            return Json(new { success = results.Success, responseText = results.Message, parentId = (long?)results.ExtraData }, JsonRequestBehavior.AllowGet);           
        }
        
        public async Task<ActionResult> MoveFiles(int Id, int[] files)
        {

            Folder item = _db.Folder.Find(Id);
            
            if (files==null)
                return Json(new { success = false, responseText = "Empty list of document", id = Id, parentId = Id }, JsonRequestBehavior.AllowGet);

            // Move files
            var result = await _cs.MoveFilesAsync(Id, files);

            return Json(new { success = result.Success, responseText = result.Message, id = Id, parentId = Id }, JsonRequestBehavior.AllowGet);
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
          
    }
}
