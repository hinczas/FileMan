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
        public int NumDocs { get; set; }
        public int NumRevs { get; set; }
        public int NumCats { get; set; }
        public int NumUsers { get; set; }
        public double DriveSize { get; set; }
        public double DriveFree { get; set; }
        public double DriveUsed { get; set; }
        public int NumPhisDocs { get; set; }
        public int NumPhisCats { get; set; }
        public double FilesSize { get; set; }

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