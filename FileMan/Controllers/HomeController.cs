using FileMan.Classes;
using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


        [HttpGet]
        public JsonResult GetTree(long id)
        {

            //var root = _is.GetRoot();


            //var nods = _is.GetTree(root, id);
            //bool exp = nods == null ? false : nods.Select(a => a.state.expandedPath).Max();

            //var nState = new TreeNodeState()
            //{
            //    disabled = false,
            //    selected = id == root.Id ? true : false,
            //    expanded = exp
            //};

            //var list2 = new TreeviewNodeEntity[1]
            //{
            //    new TreeviewNodeEntity()
            //    {
            //        text = root.Name,
            //        tags = new string[1] { root.Files.Count.ToString() },
            //        href = "/Home/Index/" + root.Id,
            //        state = nState,
            //        nodes = nods
            //    }                
            //};

            var list2 = _is.JSTree(id);
            return Json(list2, JsonRequestBehavior.AllowGet);
        }
    }
}