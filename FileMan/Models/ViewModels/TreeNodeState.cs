using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Models.ViewModels
{
    public class TreeNodeState
    {
        public bool disabled { get; set; }
        public bool expanded { get; set; }
        public bool selected { get; set; }
        public bool expandedPath { get; set; }
    }
}