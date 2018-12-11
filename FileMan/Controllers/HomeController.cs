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

        public ActionResult Index(long? id)
        {

            var items = _db.Folder.Count();
            ItemViewModel ivm;

            string userId = User.Identity.GetUserId();

            if (items==0)
            {
                long newId = _is.CreateRoot();
                ivm = _is.GetItemViewModel(newId, userId);

            } else
            {
                ivm = _is.GetItemViewModel(id, userId);
            }
            
            
            return View(ivm);
        }                
    }
}