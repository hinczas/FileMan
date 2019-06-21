using Raf.FileMan.Context;
using Raf.FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Classes
{

    public class AdminService
    {
        private AppDbContext _db;

        public AdminService(AppDbContext db)
        {
            _db = db;
        }

        public List<MonthlyStatsVM> GetMonthlyStats()
        {
            List<MonthlyStatsVM> ms = new List<MonthlyStatsVM>();

            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            for (int i = 0; i < 6; i++)
            {
                int monthNum = firstDayOfMonth.Month;
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNum);
                int numDocs = _db.MasterFile.Where(w => w.Added >= firstDayOfMonth && w.Added <= lastDayOfMonth).Count();
                int numCats = _db.Folder.Where(w => w.Added >= firstDayOfMonth && w.Added <= lastDayOfMonth).Count();
                int numRevs = _db.FileRevision.Where(w => w.Added >= firstDayOfMonth && w.Added <= lastDayOfMonth).Count();

                ms.Add(new MonthlyStatsVM()
                {
                    MonthName = monthName,
                    MonthNum = monthNum,
                    NumDocs = numDocs,
                    NumCats = numCats,
                    NumRevs = numRevs
                });

                firstDayOfMonth = firstDayOfMonth.AddMonths(-1);
                lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            }

            return ms;
        }

        public List<TopUserVM> GetTopUsers()
        {
            List<TopUserVM> tu = new List<TopUserVM>();

            string[] userIds = _db.Users.Select(s => s.Id).ToArray();

            foreach (var id in userIds)
            {
                var user = _db.Users.Find(id);

                int numDocs = user.Documents.Count;
                int numCats = user.Categories.Count;
                int numRevs = user.Documents.SelectMany(sm => sm.Revisions).Count();
                int numTotal = numCats + numDocs + numRevs;

                tu.Add(new TopUserVM()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    NumCats = numCats,
                    NumDocs = numDocs,
                    NumRevs = numRevs,
                    NumTotal = numTotal
                });
            }

            return tu.OrderByDescending(o => o.NumTotal).Take(3).ToList();
        }
    }
}