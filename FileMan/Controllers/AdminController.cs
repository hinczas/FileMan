using System;
using System.Linq;
using System.Web.Mvc;
using Raf.FileMan.Models;
using Raf.FileMan.Context;
using Raf.FileMan.Classes;
using Raf.FileMan.Models.ViewModels;
using System.Globalization;
using System.Collections.Generic;

namespace Raf.FileMan.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            string retTo = "folder";
            long? retId = -1;

            string retFun = "";
            SessionState ss;

            if (Session["SessionState"] != null)
            {
                ss = (SessionState)Session["SessionState"];
                retTo = ss.ReturnTo;
                retId = ss.ReturnId;
            }
            else
            {
                ItemService _is = new ItemService();

                retId = _is.GetRoot().Id;
                ss = new SessionState("admin", -1, (long)retId, null);
            }

            switch (retTo)
            {
                case "folder":
                    retFun = string.Format("goToFolder({0})", ss.ReturnId);
                    break;
                case "file":
                    retFun = string.Format("goToFile({0},{1})", ss.ReturnId, ss.CatId);
                    break;
                case "manage":
                    retFun = string.Format("goToManage({0})", retId);
                    break;
                case "edit":
                    retFun = string.Format("goToEditFile({0},{1})", ss.ReturnId, ss.CatId);
                    break;
                case "search":
                    retFun = string.Format("goToSearch({0},'{1}',{2})", ss.CatId, ss.Search, ss.Scope);
                    break;
                default:
                    retFun = string.Format("goToFolder({0})", retId);
                    break;
            }

            AdminService _as = new AdminService(new AppDbContext());

            var ms = _as.GetMonthlyStats();
            var tu = _as.GetTopUsers();

            AdminIndexVM model = new AdminIndexVM() { ReturnFunction= retFun, MonthlyStats = ms, TopUsers = tu };

            

            model.MonthlyStats = model.MonthlyStats.OrderBy(o => o.MonthNum).ToList();
            return PartialView(model);
        }
    }
}