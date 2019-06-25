using System;
using System.Linq;
using System.Web.Mvc;
using Raf.FileMan.Models;
using Raf.FileMan.Context;
using Raf.FileMan.Classes;
using Raf.FileMan.Models.ViewModels;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

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
                case "admin":
                    retFun = string.Format("goToAdmin({0})", retId);
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
            AdminService _as = new AdminService(_db);

            var ms = _as.GetMonthlyStats();
            var tu = _as.GetTopUsers();
            var us = _as.GetUsers();

            AdminIndexVM model = new AdminIndexVM() { ReturnFunction= retFun, MonthlyStats = ms, TopUsers = tu, Users = us };         
            model.MonthlyStats = model.MonthlyStats.OrderBy(o => o.MonthNum).ToList();
            model.NumCats = _db.Folder.Count();
            model.NumDocs = _db.MasterFile.Count();
            model.NumRevs = _db.FileRevision.Count();
            model.NumUsers = _db.Users.Count();

            string dataPath = ConfigurationManager.AppSettings["ROOT_DIR"];
            FileInfo f = new FileInfo(dataPath);
            string driveLetter = Path.GetPathRoot(f.FullName);

            int files = System.IO.Directory.GetFiles(dataPath, "*.*", SearchOption.AllDirectories).Count();
            int dirs = System.IO.Directory.GetDirectories(dataPath, "*", SearchOption.AllDirectories).Count();

            model.NumPhisCats = dirs;
            model.NumPhisDocs = files;

            DriveInfo[] drives = DriveInfo.GetDrives();

            var drive = drives.Where(w => w.Name.Equals(driveLetter)).FirstOrDefault();

            if(drive != null)
            {
                model.DriveSize = drive.TotalSize / 1024 / 1024 / 1024;
                model.DriveFree = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                model.DriveUsed = (drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024 / 1024;
            }
            Session["SessionState"] = new SessionState("admin", ss.CatId, ss.DocId, ss.Search, ss.Scope, "admin", retId);

            return View(model);
        }

        public ActionResult PartialIndex()
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
                case "admin":
                    retFun = string.Format("goToAdmin({0})", retId);
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
            AdminService _as = new AdminService(_db);

            var ms = _as.GetMonthlyStats();
            var tu = _as.GetTopUsers();
            var us = _as.GetUsers();

            AdminIndexVM model = new AdminIndexVM() { ReturnFunction = retFun, MonthlyStats = ms, TopUsers = tu, Users = us };
            model.MonthlyStats = model.MonthlyStats.OrderBy(o => o.MonthNum).ToList();
            model.NumCats = _db.Folder.Count();
            model.NumDocs = _db.MasterFile.Count();
            model.NumRevs = _db.FileRevision.Count();
            model.NumUsers = _db.Users.Count();

            string dataPath = ConfigurationManager.AppSettings["ROOT_DIR"];
            FileInfo f = new FileInfo(dataPath);
            string driveLetter = Path.GetPathRoot(f.FullName);

            int files = System.IO.Directory.GetFiles(dataPath, "*.*", SearchOption.AllDirectories).Count();
            int dirs = System.IO.Directory.GetDirectories(dataPath, "*", SearchOption.AllDirectories).Count();

            model.NumPhisCats = dirs;
            model.NumPhisDocs = files;

            DriveInfo[] drives = DriveInfo.GetDrives();

            var drive = drives.Where(w => w.Name.Equals(driveLetter)).FirstOrDefault();

            if (drive != null)
            {
                model.DriveSize = drive.TotalSize / 1024 / 1024 / 1024;
                model.DriveFree = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                model.DriveUsed = (drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024 / 1024;
            }
            Session["SessionState"] = new SessionState("admin", ss.CatId, ss.DocId, ss.Search, ss.Scope, "admin", retId);

            return PartialView(model);
        }
    }
}