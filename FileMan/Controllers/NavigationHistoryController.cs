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

        public PartialViewResult NavigationMenu()
        {
            var reqUrl = HttpContext.Request.Url.PathAndQuery;
            var func = ParseUrl(reqUrl);

            var model = new NavigationViewModel();

            if (Session["HistoryId"] == null)
            {
                model.BackClass = "disabled";
                model.ForthClass = "disabled";
            }
            else {
                long id = (long)Session["HistoryId"];

                bool hasNext = HasNext(id);
                bool hasPrev = HasPrev(id);

                model.BackClass = hasPrev ? "" : "disabled";
                model.ForthClass = hasNext ? "" : "disabled"; 
            }



            return PartialView(model);
        }

        [HttpPost]
        public async Task<bool> Create(string jsfun, bool manual)
        {            
            var history = new NavigationHistory(User.Identity.GetUserId(), HttpContext.Session.SessionID);
            history.JSFunction = jsfun;

            if (manual)
            {
                try
                {
                    if (Session["HistoryId"] != null)
                    {
                        await DeleteForthHistoryAsync((long)Session["HistoryId"]);
                    }
                    _db.History.Add(history);
                    _db.SaveChanges();

                    Session["HistoryId"] = history.Id;

                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return false;

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
        public string GetBackFunction()
        {
            if (Session["HistoryId"] == null)
            {
                return string.Empty;
            }

            var histID = (long)Session["HistoryId"];
            var histObj = GetPreviousHistoryItem(histID);

            if (histObj == null)
            {
                return string.Empty;
            }
            else
            {
                Session["HistoryId"] = histObj.Id;
                return histObj.JSFunction;
            }
        }

        [HttpGet]
        public string GetForthFunction()
        {
            if (Session["HistoryId"] == null)
            {
                return string.Empty;
            }

            var histID = (long)Session["HistoryId"];
            var histObj = GetNextHistoryItem(histID);

            if (histObj == null)
            {
                return string.Empty;
            }
            else
            {
                Session["HistoryId"] = histObj.Id;
                return histObj.JSFunction;
            }
        }

        [HttpGet]
        public string GetCurrentFunction()
        {
            if (Session["HistoryId"] == null)
            {
                return string.Empty;
            }

            var histID = (long)Session["HistoryId"];
            var histObj = GetCurrentHistoryItem(histID);

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
               
        private NavigationHistory GetPreviousHistoryItem(long id)
        {
            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;

            var history = _db.History
                                    .Where(a => a.Id < id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .Select(s => s.Id);

            if (history == null|| history.Count() < 1)
                return null;

            var histId = history.Max();

            var item = _db.History.Find(histId);


            return item;
        }

        private NavigationHistory GetNextHistoryItem(long id)
        {
            var userId = User.Identity.GetUserId();
            var sessID = HttpContext.Session.SessionID;

            var history = _db.History
                                    .Where(a => a.Id > id && a.UserId.Equals(userId) && a.SessionId.Equals(sessID))
                                    .Select(s => s.Id);

            if (history == null || history.Count() < 1)
                return null;

            var histId = history.Min();

            var item = _db.History.Find(histId);


            return item;
        }
        
        private NavigationHistory GetCurrentHistoryItem(long id)
        {
            var item = _db.History.Find(id);

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
            if (url.Equals("/") || url.Equals("/Home/Index"))
                return "goToFolder(undefined, true, false)";

            if (url.Contains("/Home/Index/"))
            {
                int length = "/Home/Index/".Length;
                

                string fun = "goToFolder({0}, true, false)";

                string itemId = url.Substring(length);

                return string.Format(fun, itemId);
            }


            if (url.Contains("/MasterFile/Details/") || url.Contains("/Share/"))
            {
                int length = 0;
                if (url.Contains("/MasterFile/Details/"))
                    length = "/MasterFile/Details/".Length;


                if (url.Contains("/Share/"))
                    length = "/Share/".Length;


                string fun = "goToFile({0},{1}, false)";

                int loc = url.IndexOf("?pid=");

                string itemId = "";
                string pid = "-1";

                if (loc == -1)
                {
                    itemId = url.Substring(length);

                } else
                {
                    itemId = url.Substring(length, loc - length);
                    pid = url.Substring(length + itemId.Length + "?pid=".Length);
                }
                return string.Format(fun, itemId, pid);
            }
            return "";
        }
    }
}