﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Models.ViewModels
{
    public class TreeNode
    {
        public Folder Branch { get; set; }
        public int Indent { get; set; }
    }
}