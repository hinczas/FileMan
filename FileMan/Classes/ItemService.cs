using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace FileMan.Classes
{
    public class ItemService
    {
        private DatabaseCtx _db;

        public ItemService()
        {
            _db = new DatabaseCtx();
        }

        /// <summary>
        /// Get breadcrumbs (links for all parents of an Item)
        /// </summary>
        /// <param name="id">Current directory ID</param>
        /// <returns>List<Item> - list of all parent direcotries</returns>
        private List<Folder> GetBreadcrumbs(long id)
        {
            var item = _db.Folder.Find(id);
            List<Folder> bc = new List<Folder>();
            while (item.Parent != null)
            {
                bc.Add(item.Parent);
                item = item.Parent;
            }
            return bc;
        }
        private List<Folder> GetFileBreadcrumbs(long id)
        {
            var item = _db.Folder.Find(id);
            List<Folder> bc = new List<Folder>();
            bc.Add(item);
            while (item.Parent != null)
            {
                bc.Add(item.Parent);
                item = item.Parent;
            }
            return bc;
        }

        private List<TreeNode> GetTree(List<TreeNode> tree, Folder root, int indent)
        {
            TreeNode tmp = new TreeNode();

            tmp.Branch = root;
            tmp.Indent = indent;
            tree.Add(tmp);
            var folders = _db.Folder.Where(a => a.Pid == root.Id).OrderBy(a=>a.Name).ToList();
            if (folders.Count > 0)
            {
                foreach(Folder fol in folders)
                {
                    tree = GetTree(tree, fol, indent + 1);
                }
            }
            return tree;
        }

        private string GetTree(string tree, Folder root, int indent)
        {
            if (string.IsNullOrEmpty(tree))
                tree = "<ul id=\"myUL\">\n";
            
            var folders = _db.Folder.Where(a => a.Pid == root.Id).OrderBy(a => a.Name).ToList();
            if (folders.Count > 0)
            {
                foreach (Folder fol in folders)
                {
                    tree = GetTree(tree, fol, indent + 1);
                }
            }

            return tree;
        }

        public MasterFileViewModel GetMasterFileViewModel(long id, string userId)
        {
            var item = _db.MasterFile.Find(id);
            var revisions = item.Revisions;
            var latRev = revisions.Count == 0 ? 0 : revisions.Select(a => a.Revision).Max();
            var revId = revisions.Count == 0 ? 0 : revisions.OrderByDescending(a => a.Revision).Take(1).Select(b => b.Id).FirstOrDefault();
            var revNam = revisions.Count == 0 ? "N/A" : revisions.Where(a => a.Id == revId).FirstOrDefault().Name;
            var revCnt = revisions.Count == 0 ? 0 : revisions.Count();

            List<TreeNode> list = new List<TreeNode>();
            var root = _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault();
            list = GetTree(list, root, 0);

            FileRevision fr = _db.FileRevision.Where(a => a.MasterFileId == id).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();
            string issue = item.Issue == null ? "" : item.Issue.ToString();
            string draft = fr == null ? "" : fr.Draft;
            FileRevision lIssue = _db.FileRevision.Where(a => a.MasterFileId == id).Where(c=> c.Type.Equals("issue")).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();
            FileRevision lDraft = _db.FileRevision.Where(a => a.MasterFileId == id).Where(c => c.Type.Equals("draft")).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();
            var foldersList = _db.Folder.Select(a => new FolderPartialViewModel()
            {
                Id = a.Id,
                Name = a.Name,
                Path = a.Path
            })
            .OrderBy(a=>a.Name)
            .ThenBy(a=>a.Path)
            .ToList();

            ApplicationDbContext adb = new ApplicationDbContext();
            ApplicationUser user = adb.Users.Find(userId);
            bool changelog = user.UserSetting.ShowChangelog;

            bool promote = !issue.Equals(draft) && revisions.Count() != 0;
            MasterFileViewModel file = new MasterFileViewModel()
            {
                Current = item,
                //Breadcrumbs = GetFileBreadcrumbs((long)item.FolderId),
                LatestRevision = latRev,
                LatestRevisionId = revId,
                RevisionName = revNam,
                RevisionsCount = revCnt,
                DraftVersion = draft,
                LatestDraft = lDraft,
                LatestIssue = lIssue,
                TreeNodes = list,
                FolderList = foldersList,
                Promote = promote,
                ShowChangelog = changelog
            };
            return file;
        }

        /// <summary>
        /// Return Item View Model for index page
        /// </summary>
        /// <param name="id">Optional current item ID</param>
        /// <returns>ItemViewModel</returns>
        public ItemViewModel GetItemViewModel(long? id, string userId)
        {
            Folder item;
            if (id == null)
            {
                item = _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault();
            }
            else
            {
                item = _db.Folder.Find(id);
            }

            ApplicationDbContext adb = new ApplicationDbContext();
            ApplicationUser user = adb.Users.Find(userId);
            bool uncatVisible = user.UserSetting.UncategorisedVisible;
            bool showUncatRoot = user.UserSetting.ShowUncategorisedRoot;

            var childrenDir = _db.Folder.Where(a => a.Pid == item.Id).OrderBy(a=>a.Name).ToList();
            var childrenFil = item.Files.OrderBy(a => a.Name).ToList();
            var unassigned = _db.MasterFile.Where(a => a.Folders.Count == 0).ToList();

            if (uncatVisible)
            {
                if(showUncatRoot)
                {
                    if(item.Type.Equals("root"))
                    {
                        childrenFil = childrenFil.Union(unassigned).OrderBy(a => a.Name).ToList();
                    }
                }                
            }

            
            var bc = GetBreadcrumbs(item.Id);
            List<TreeNode> list = new List<TreeNode>();
            var root = _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault();
            list = GetTree(list, root, 0);
            var foldersList = _db.Folder.Select(a => new FolderPartialViewModel()
            {
                Id = a.Id,
                Name = a.Name,
                Path = a.Path
            }).ToList();


            ItemViewModel ivm = new ItemViewModel()
            {
                Current = item,
                ChildrenDirs = childrenDir,
                ChildrenFiles = childrenFil,
                Breadcrumbs = bc,
                TreeNodes = list,
                UnassignedFiles = uncatVisible && !showUncatRoot ? unassigned : null,
                FolderList = foldersList
            };
            return ivm;
        }

        public long CreateRoot()
        {
            string path = ConfigurationManager.AppSettings["ROOT_DIR"];
            Directory.CreateDirectory(path);
            Folder item = new Folder()
            {
                Name = "root",
                Type= "root",
                Path = path,
                Description = "Root directory",
                Added = DateTime.Now
            };
            _db.Folder.Add(item);
            _db.SaveChanges();
            
            return item.Id;
        }

        public void DeleteDirectory(long id)
        {
            Folder folder = _db.Folder.Find(id);
            List<Folder> dirs = _db.Folder.Where(a=>a.Pid==id).ToList();

            foreach (Folder dir in dirs)
            {
                DeleteDirectory(dir.Id);
            }

            _db.Folder.Remove(folder);
            _db.SaveChanges();
        }

        public void DeleteFile(long id)
        {
            var root = _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault().Path;
            var mf = _db.MasterFile.Find(id);
            var path = Path.Combine(root, mf.Number);

            var revisions = mf.Revisions.ToList();

            foreach (FileRevision fr in revisions)
            {
                System.IO.File.SetAttributes(fr.FullPath, FileAttributes.Normal);
                System.IO.File.Delete(fr.FullPath);
                _db.FileRevision.Remove(fr);
            }
            _db.SaveChanges();

            Directory.Delete(path);

            _db.MasterFile.Remove(mf);
            _db.SaveChanges();

        }

        public string Increment(string s)
        {

            // first case - string is empty: return "a"

            if ((s == null) || (s.Length == 0))

                return "A";

            // case if is numeric
            double tmp;
            if (Double.TryParse(s, out tmp))
                return s + "A";

            // next case - last char is less than 'z': simply increment last char

            char lastChar = s[s.Length - 1];

            string fragment = s.Substring(0, s.Length - 1);

            if (lastChar < 'Z')

            {

                ++lastChar;

                return fragment + lastChar;

            }

            // next case - last char is 'z': roll over and increment preceding string

            return Increment(fragment) + 'A';

        }
    }
}