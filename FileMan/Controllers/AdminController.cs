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
        private AppDbContext _db;

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

            _db = new AppDbContext();

            AdminIndexVM model = new AdminIndexVM() { ReturnFunction= retFun, MonthlyStats = new List<MonthlyStatsVM>() };

            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            for (int i =0; i< 6; i++)
            {
                int monthNum = firstDayOfMonth.Month;
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNum);
                int numDocs = _db.MasterFile.Where(w => w.Added >= firstDayOfMonth && w.Added <= lastDayOfMonth).Count();
                int numCats = _db.Folder.Where(w => w.Added >= firstDayOfMonth && w.Added <= lastDayOfMonth).Count();
                int numRevs = _db.FileRevision.Where(w => w.Added >= firstDayOfMonth && w.Added <= lastDayOfMonth).Count();

                model.MonthlyStats.Add(new MonthlyStatsVM()
                {
                    MonthName = monthName,
                    MonthNum = monthNum,
                    NumDocs = numDocs,
                    NumCats = numCats,
                    NumRevs = numRevs
                });

                firstDayOfMonth = firstDayOfMonth.AddMonths(-1);
                lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            }

            model.MonthlyStats = model.MonthlyStats.OrderBy(o => o.MonthNum).ToList();
            return PartialView(model);
        }
    }
}