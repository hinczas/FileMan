using Microsoft.AspNet.Identity;
using Raf.FileMan.Classes;
using Raf.FileMan.Context;
using Raf.FileMan.Models;
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

        //[HttpPost]
        //public bool CreateFromUrl(string url)
        //{

        //    var userId = User.Identity.GetUserId();
        //    var sessID = HttpContext.Session.SessionID;

        //    return false;
        //}

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

    }
}