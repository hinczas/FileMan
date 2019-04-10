using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Models.ViewModels
{
    public class JSTNode
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public JSTState state { get; set; }
        public List<JSTNode> children { get; set; }
        //li_attr     : {}  // attributes for the generated LI node
        public JSTAAttr a_attr { get; set; }
    }

    public class JSTAAttr
    {
        public string href { get; set; }
    }

    public class JSTState
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
    }
}