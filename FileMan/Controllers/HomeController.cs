﻿using Raf.FileMan.Classes;

using Raf.FileMan.Models;
using Raf.FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
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
    public class HomeController : Controller
    {
        private ItemService _is;
        private AppDbContext _db;

        public HomeController()
        {
            _is = new ItemService();
            _db = new AppDbContext();
        }

        public ActionResult Index(long? id, string search, int? scope)
        {
            TempData["folderId"] = id;
            var items = _db.Folder.Count();
            ItemViewModel ivm;

            string userId = User.Identity.GetUserId();

            string location = "";
            if(string.IsNullOrEmpty(search))
            {
                if (items == 0)
                {
                    long newId = _is.CreateRoot(userId);
                    ivm = _is.GetItemViewModel(newId, userId);

                }
                else
                {
                    ivm = _is.GetItemViewModel(id, userId);
                }
                location = "folder";
            } else
            {                
                ivm = _is.GetItemViewModel(search, (int)scope);
                location = "search";
            }

            Session["SessionState"] = new SessionState(location, ivm.Current.Id,-1,search, scope, location, ivm.Current.Id); 

            return View(ivm);
        }

        public PartialViewResult TreeIndex(long? id, string search, int? scope)
        {
            TempData["folderId"] = id;
            var items = _db.Folder.Count();
            ItemViewModel ivm;

            string userId = User.Identity.GetUserId();

            string location = "";

            if (string.IsNullOrEmpty(search))
            {
                if (items == 0)
                {
                    long newId = _is.CreateRoot(userId);
                    ivm = _is.GetItemViewModel(newId, userId);

                }
                else
                {
                    ivm = _is.GetItemViewModel(id, userId);
                }
                location = "folder";
            }
            else
            {
                ivm = _is.GetItemViewModel(search, (int)scope);
                location = "search";
            }

            Session["SessionState"] = new SessionState(location, ivm.Current.Id, -1, search, scope, location, ivm.Current.Id);

            return PartialView(ivm);
        }

        [HttpGet]
        public ActionResult GetDocTable(long? id)
        {
            TempData["folderId"] = id;
            ItemViewModel ivm;

            string userId = User.Identity.GetUserId();
            
            ivm = _is.GetPartialItemViewModel(id);


            return PartialView("_DocTable", ivm);
        }
        
        [HttpGet]
        public JsonResult GetTree(long id)
        {
            var list2 = _is.JSTree(id);
            return Json(list2, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Redirect()
        {
            if (Session["SessionState"] == null )
            {
                return RedirectToAction("Index");
            }

            SessionState ss = (SessionState)Session["SessionState"];

            switch(ss.Location)
            {
                case "folder":
                    return RedirectToAction("Index", new { id = ss.CatId });
                case "file":
                    return RedirectToAction("Details", "MasterFiles", new { id = ss.DocId, pid = ss.CatId });
                case "edit":
                    return RedirectToAction("Edit", "MasterFiles", new { id = ss.DocId, pid = ss.CatId });
                case "search":
                    return RedirectToAction("Index", new { id = ss.CatId, search = ss.Search, scope = ss.Scope });
                case "manage":
                    return RedirectToAction("Index", "Manage");
                default:
                    return RedirectToAction("Index");
            }
        }
    }
}