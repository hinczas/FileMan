using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Models
{
    public class SessionState
    {
        public string Location { get; set; }
        public long CatId { get; set; }
        public long DocId { get; set; }
        public string Search { get; set; }
        public int? Scope { get; set; }
        public string ReturnTo { get; set; }
        public long? ReturnId { get; set; }

        public SessionState(string location, long catId, long docId, string search, int? scope = null, string returnTo = "", long? returnId = null)
        {
            Location = location;
            CatId = catId;
            DocId = docId;
            Search = search;
            Scope = scope;
            ReturnTo = returnTo;
            ReturnId = returnId;
        }
    }
}