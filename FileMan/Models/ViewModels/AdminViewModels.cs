using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Models.ViewModels
{
    public class AdminIndexVM
    {
        public string ReturnFunction { get; set; }
        public List<MonthlyStatsVM> MonthlyStats { get; set; }
        public List<TopUserVM> TopUsers { get; set; }
    }

    public class MonthlyStatsVM
    {
        public string MonthName { get; set; }
        public int MonthNum { get; set; }
        public int NumDocs { get; set; }
        public int NumRevs { get; set; }
        public int NumCats { get; set; }
    }

    public class TopUserVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int NumDocs { get; set; }
        public int NumRevs { get; set; }
        public int NumCats { get; set; }
        public int NumTotal { get; set; }
    }
}