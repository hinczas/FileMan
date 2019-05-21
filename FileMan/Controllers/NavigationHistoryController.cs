using Microsoft.AspNet.Identity;
using Raf.FileMan.Classes;
using Raf.FileMan.Context;
using Raf.FileMan.Models;
using Raf.FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Raf.FileMan.Controllers
{
    public class NavigationHistoryController : Controller
    {
        private ItemService _is;
        private AppDbContext _db;

        public NavigationHistoryController()
        {
            _is = new ItemService();
            _db = new AppDbContext();
        }

        [HttpGet]
        public async Task<PartialViewResult> NavigationMenu(string url = "")
        {
            var reqUrl = HttpContext.Request.Url.PathAndQuery;
            if (reqUrl.Contains("NavigationMenu"))
                reqUrl = url;

            var func = ParseUrl(reqUrl);

            var model = new NavigationViewModel();

            if (Session["HistoryId"] == null)
            {
                await DeleteHistoryAsync();

                model.BackClass = "disabled";
                model.BackDisabled = true;
                model.ForthClass = "disabled";
                model.ForthDisabled = true;

                await Create(func, true);
            }
            else {
                long id = (long)Session["HistoryId"];

                string sesFunc = await GetCurrentFunction();

                if(!sesFunc.Equals(func))
                {
                    await Create(func, true);
                    id = (long)Session["HistoryId"];
                }

                bool hasNext = HasNext(id);
                bool hasPrev = HasPrev(id);

                model.BackClass = hasPrev ? "" : "disabled";
                model.BackDisabled = !hasPrev;
                model.ForthClass = hasNext ? "" : "disabled";
                model.ForthDisabled = !hasNext;
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<JsonResult> Create(string jsfun, bool manual)
        {            
            var history = new NavigationHistory(User.Identity.GetUserId(), HttpContext.Session.SessionID);

            if (jsfun.Contains("undefined"))
            {
                var root = _is.GetRootId();
                jsfun = jsfun.Replace("undefined", root.ToString());
            }
            history.JSFunction = jsfun;

            if (manual)
            {
                try
                {
                    if (Session["HistoryId"] != null)
                    {
                        string sesFunc = await GetCurrentFunction();

                        if (!sesFunc.Equals(jsfun))
                        {
                            await DeleteForthHistoryAsync((long)Session["HistoryId"]);

                            _db.History.Add(history);
                            _db.SaveChanges();
                        }
                    } else
                    {
                        _db.History.Add(history);
                        _db.SaveChanges();
                    }

                    Session["HistoryId"] = history.Id;

                }
                catch (Exception e)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }


            bool hasNext = HasNext((long)Session["HistoryId"]);
            bool hasPrev = HasPrev((long)Session["HistoryId"]);

            return Json(new { success = true, back = hasPrev, forth = hasNext }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public void CreateFromUrl(string url)
        {

            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;
                        
            if (Session["HistoryId"] == null)
            {

            }

        }

        [HttpGet]
        public async Task<JsonResult> GetBackFunction()
        {
            if (Session["HistoryId"] == null)
            {
                //return string.Empty;
                return Json(new { success = false, func = "", dis = true }, JsonRequestBehavior.AllowGet);
            }

            var histID = (long)Session["HistoryId"];
            var histObj = await GetPreviousHistoryItem(histID);

            if (histObj == null)
            {
                return Json(new { success = false, func = "", dis = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Session["HistoryId"] = histObj.Id;
                bool hasPrev = HasPrev(histObj.Id);

                return Json(new { success = true, func = histObj.JSFunction, dis = !hasPrev }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetForthFunction()
        {
            if (Session["HistoryId"] == null)
            {
                return Json(new {success = false,  func = "", dis = true }, JsonRequestBehavior.AllowGet);
            }

            var histID = (long)Session["HistoryId"];
            var histObj = await GetNextHistoryItem(histID);

            if (histObj == null)
            {
                return Json(new { success = false, func = "", dis = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Session["HistoryId"] = histObj.Id;
                bool hasNext = HasNext(histObj.Id);

                return Json(new { success = true, func = histObj.JSFunction, dis = !hasNext }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<string> GetCurrentFunction()
        {
            if (Session["HistoryId"] == null)
            {
                return string.Empty;
            }

            var histID = (long)Session["HistoryId"];
            var histObj = await GetCurrentHistoryItem(histID);

            if (histObj == null)
            {
                return string.Empty;
            }
            else
            {
                return histObj.JSFunction;
            }
        }

        [HttpPost]
        public bool ClearHistory()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var sessID = HttpContext.Session.SessionID;

                var history = _db.History.Where(a => a.UserId.Equals(userId) && a.SessionId.Equals(sessID)).ToList();

                _db.History.RemoveRange(history);
                _db.SaveChanges();

                return true;
            } catch (Exception e)
            {
                return false;
            }
        } 
               
        private async Task<NavigationHistory> GetPreviousHistoryItem(long id)
        {
            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;

            var history = _db.History
                                    .Where(a => a.Id < id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .Select(s => s.Id);

            if (history == null|| history.Count() < 1)
                return null;

            var histId = history.Max();

            var item = await _db.History.FindAsync(histId);


            return item;
        }

        private async Task<NavigationHistory> GetNextHistoryItem(long id)
        {
            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;

            var history = _db.History
                                    .Where(a => a.Id > id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .Select(s => s.Id);

            if (history == null || history.Count() < 1)
                return null;

            var histId = history.Min();

            var item = await _db.History.FindAsync(histId);


            return item;
        }
        
        private async Task<NavigationHistory> GetCurrentHistoryItem(long id)
        {
            var item = await _db.History.FindAsync(id);

            return item;
        }

        private async Task<bool> DeleteForthHistoryAsync(long id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var sessID = HttpContext.Session.SessionID;

                var history = _db.History
                                    .Where(a => a.Id > id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .ToList();

                _db.History.RemoveRange(history);
                await _db.SaveChangesAsync();

                return true;
            } catch (Exception e)
            {
                return false;
            }
        }

        private async Task<bool> DeleteHistoryAsync()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var sessID = HttpContext.Session.SessionID;

                var history = _db.History
                                    .Where(a => a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .ToList();

                _db.History.RemoveRange(history);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool HasNext(long id)
        {
            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;

            var history = _db.History
                                    .Where(a => a.Id > id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .Select(s => s.Id);

            if (history != null && history.Count() > 0)
                return true;
            else
                return false;
        }

        private bool HasPrev(long id)
        {
            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;

            var history = _db.History
                                    .Where(a => a.Id < id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .Select(s => s.Id);

            if (history != null && history.Count() > 0)
                return true;
            else
                return false;
        }

        private string ParseUrl(string url)
        {
            string pidStr = "?pid=";
            string idStr = "?id=";
            string srchStr = "?search=";
            long rootId = _is.GetRootId();
            // goToManage()
            if (url.Equals("/Manage/Index"))
                return "goToManage("+rootId.ToString()+", false)";
            
            // goToFolder()
            if (url.Equals("/") || url.Equals("/Home/Index"))
                return "goToFolder(" + rootId.ToString() + ", true, false)";

            // goToSearch()
            if(url.Contains(srchStr))
            {
                int length = srchStr.Length;
                int loc = url.IndexOf(srchStr);
                string query = url.Substring(loc + length);

                string fun = "goToSearch('{0}', false)";
                return string.Format(fun, query);
            }

            // goToFolder()
            if (url.Contains("/Home/Index/"))
            {
                int length = "/Home/Index/".Length;

                if (url.Contains(idStr))
                {
                    length += idStr.Length;
                    pidStr = "&pid=";
                }

                string fun = "goToFolder({0}, true, false)";
                string itemId = url.Substring(length);
                return string.Format(fun, itemId);
            }

            // goToFile()
            if (url.Contains("/MasterFiles/Details/") || url.Contains("/Share/"))
            {
                int length = 0;
                if (url.Contains("/MasterFiles/Details/"))
                    length = "/MasterFiles/Details/".Length;
                
                if (url.Contains("/Share/"))
                    length = "/Share/".Length;


                if (url.Contains(idStr))
                {
                    length += idStr.Length;
                    pidStr = "&pid=";
                }

                string fun = "goToFile({0}, {1}, false)";
                int loc = url.IndexOf(pidStr);
                string itemId = "";
                string pid = "-1";

                if (loc == -1)
                {
                    itemId = url.Substring(length);

                } else
                {
                    itemId = url.Substring(length, loc - length);
                    pid = url.Substring(length + itemId.Length + pidStr.Length);
                }
                return string.Format(fun, itemId, pid);
            }

            // goToEditFile()
            if (url.Contains("/MasterFiles/Edit/"))
            {
                int length = "/MasterFiles/Edit/".Length;

                if (url.Contains(idStr))
                {
                    length += idStr.Length;
                    pidStr = "&pid=";
                }

                string fun = "goToEditFile({0}, {1}, false)";
                int loc = url.IndexOf(pidStr);
                string itemId = "";
                string pid = "-1";

                if (loc == -1)
                {
                    itemId = url.Substring(length);
                }
                else
                {
                    itemId = url.Substring(length, loc - length);
                    pid = url.Substring(length + itemId.Length + pidStr.Length);
                }
                return string.Format(fun, itemId, pid);
            }
            return "";
        }
    }
}