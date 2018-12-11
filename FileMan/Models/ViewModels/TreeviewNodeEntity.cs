using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Models.ViewModels
{
    [Serializable]
    public class TreeviewNodeEntity
    {
        public string text { get; set; }

        public string[] tags { get; set; }

        public TreeviewNodeEntity[] nodes { get; set; }
        public string href { get; set; }

        public TreeNodeState state { get; set; }
    }
}