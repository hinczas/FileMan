using Raf.FileMan.Classes;

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
        private DocumentService _ds;

        public MasterFilesController()
        {
            _is = new ItemService();
            _db = new AppDbContext();
            _ds = new DocumentService();
        }

        // GET: MasterFiles/Details/5
        public ActionResult Details(int id, long? pid)
        {
            // Get User ID
            string userId = User.Identity.GetUserId();

            // Get the Document details
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            // Save session
            Session["SessionState"] = new SessionState("file", file.RedirectId, id, string.Empty, null, "file", id);

            return View(file);
        }

        public PartialViewResult PartialDetails(int id, long? pid)
        {
            // Get User ID
            string userId = User.Identity.GetUserId();

            // Get the Document details
            MasterFileViewModel file = _is.GetMasterFileViewModel(id, userId, (SessionState)Session["SessionState"], pid);

            // Save session
            Session["SessionState"] = new SessionState("file", file.RedirectId, id, string.Empty, null, "file", id);

            return PartialView(file);
        }

        // GET: MasterFiles/Create
        //, HttpPostedFileBase file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,Description,Comment,Number")] MasterFile item, long FolderId)
        {
            if (ModelState.IsValid)
            {
                // Get current User ID
                string userId = User.Identity.GetUserId();

                // Try to create document
                var result = await _ds.CreateAsync(item, FolderId, userId);

                // Return results
                return Json(new { success = result.Success, responseText = result.Message, parentId = FolderId }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, responseText = "Invalid model state", parentId = FolderId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> UserLock(long id, bool isLocked)
        {
            var userId = User.Identity.GetUserId();

            var result = await _ds.LockAsync(id, isLocked, userId);

            return Json(new { success = result.Success, responseText = result.Message }, JsonRequestBehavior.AllowGet);            
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,Comment,Number")] MasterFile item, long pid)
        {
            if (ModelState.IsValid)
            {
                if (!Editable(item.Id))
                    return Json(new { success = false, responseText = "Document locked by another user", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);

                // Edit
                var result = await _ds.EditAsync(item);
                
                return Json(new { success = result.Success, responseText = result.Message, id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, responseText = "Cannot update document "+item.Number + ". Data issue.", id = item.Id, parentId = pid }, JsonRequestBehavior.AllowGet);
        }

        // POST: MasterFiles/Delete/5
        [HttpPost]
        public async Task<ActionResult> DeleteAsync(int id, int folderId)
        {
            if (!Editable(id))
            {
                return Json(new { success = false, responseText = "Document locked by another user", id = id, parentId = folderId }, JsonRequestBehavior.AllowGet);
            }

            // Delete
            var result = await _ds.DeleteAsync(id, folderId);
            
            return Json(new { success = result.Success, responseText = result.Message, parentId = folderId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Promote(long id, string Comment, long pid)
        {
            if (!Editable(id))
            {
                return Json(new { success = false, responseText = "Document locked by another user", id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }

            // Promote
            var result = await _ds.PromoteAsync(id, Comment);

            return Json(new { success = result.Success, responseText = result.Message, id = id, parentId = pid }, JsonRequestBehavior.AllowGet);
        }
               
        public async Task<ActionResult> MoveFile(long Id, long[] folders, long pid)
        {
            if (!Editable(Id))
            {
                return Json(new { success = false, responseText = "Document locked by another user", id = Id, parentId = pid }, JsonRequestBehavior.AllowGet);
            }

            // Move document
            var result = await _ds.MoveAsync(Id, folders);

            return Json(new { success = result.Success, responseText = result.Message, id = Id, parentId = pid }, JsonRequestBehavior.AllowGet);   
        }
                    
        public async Task<ActionResult> MoveFileAsync(long Id, long opid, long npid)
        {
            if (!Editable(Id))
            {
                return Json(new { success = false, responseText = "Document locked by another user", id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);
            }

            // Move single
            var result = await _ds.MoveOneAsync(Id, opid, npid);

            if (result.Success)
            {
                return Json(new { success = result.Success, responseText = result.Message, id = Id, parentId = npid, affFolIds = new long[2] { opid, npid } }, JsonRequestBehavior.AllowGet);

            } else
            {
                return Json(new { success = result.Success, responseText = result.Message, id = Id, parentId = opid }, JsonRequestBehavior.AllowGet);
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
