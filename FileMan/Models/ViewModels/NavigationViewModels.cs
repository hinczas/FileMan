using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Models.ViewModels
{
    public class NavigationViewModels
    {
    }

    public class NavigationViewModel
    {
        public bool BackDisabled { get; set; }
        public bool ForthDisabled { get; set; }
        public string BackClass { get; set; }
        public string ForthClass { get; set; }
        public bool ShowButtons { get; set; }
        public string ButtonsClass { get; set; }

        public NavigationViewModel()
        {
            BackDisabled = false;
            ForthDisabled = false;
            BackClass = "";
            ForthClass = "";
            ShowButtons = true;
            ButtonsClass = "";
        }
    }
}