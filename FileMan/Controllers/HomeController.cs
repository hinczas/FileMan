using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
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
    public class HomeController : Controller
    {
        private ItemService _is;
        private DatabaseCtx _db;

        public HomeController()
        {
            _is = new ItemService();
            _db = new DatabaseCtx();
        }

        public ActionResult Index(long? id, string search, int? scope)
        {
            TempData["folderId"] = id;
            var items = _db.Folder.Count();
            ItemViewModel ivm;

            string userId = User.Identity.GetUserId();


            if(string.IsNullOrEmpty(search))
            {
                if (items == 0)
                {
                    long newId = _is.CreateRoot();
                    ivm = _is.GetItemViewModel(newId, userId);

                }
                else
                {
                    ivm = _is.GetItemViewModel(id, userId);
                }
            } else
            {                
                ivm = _is.GetItemViewModel(search, (int)scope);
            }
                       
            
            return View(ivm);
        }

        public PartialViewResult TreeIndex(long? id, string search, int? scope)
        {
            TempData["folderId"] = id;
            var items = _db.Folder.Count();
            ItemViewModel ivm;

            string userId = User.Identity.GetUserId();


            if (string.IsNullOrEmpty(search))
            {
                if (items == 0)
                {
                    long newId = _is.CreateRoot();
                    ivm = _is.GetItemViewModel(newId, userId);

                }
                else
                {
                    ivm = _is.GetItemViewModel(id, userId);
                }
            }
            else
            {
                ivm = _is.GetItemViewModel(search, (int)scope);
            }


            return PartialView(ivm);
        }

        [HttpGet]
        public JsonResult GetTree(long id)
        {
            var list2 = _is.JSTree(id);
            return Json(list2, JsonRequestBehavior.AllowGet);
        }
    }
}