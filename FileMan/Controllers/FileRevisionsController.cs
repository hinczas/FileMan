using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileMan.Controllers
{
    [Authorize]
    public class FileRevisionsController : Controller
    {
        public ActionResult FileAction(long[] revisions, string action)
        {


            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}