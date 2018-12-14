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
using System.Security.Cryptography;
using System.Text;

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

        public TreeviewNodeEntity[] GetTree(Folder root, long id)
        {
            var fols = _db.Folder.Where(a => a.Pid == root.Id);
            
            if (fols.Count() > 0)
            {
                var folders = fols.OrderBy(a => a.Name).ToList();

                int cntr = 0;
                TreeviewNodeEntity[] tmpList = new TreeviewNodeEntity[folders.Count];

                foreach (Folder fol in folders)
                {
                    var nods = GetTree(fol, id);
                    bool exp = nods==null ? false : nods.Select(a => a.state.expandedPath).Max();

                    var files = fol.Files.Count.ToString();
                    var nState = new TreeNodeState()
                    {
                        disabled = false,
                        selected = id == fol.Id ? true : false,
                        expanded = exp,
                        expandedPath = exp || id == fol.Id

                    };
                    tmpList[cntr] = new TreeviewNodeEntity()
                    {
                        text = fol.Name,
                        tags = new string[1] { files },
                        href = "/Home/Index/" + fol.Id,
                        state = nState,
                        nodes = nods
                    };
                    cntr++;
                }
                return tmpList;
            }
            else
            {
                return null;
            }
        }

        public MasterFileViewModel GetMasterFileViewModel(long id, string userId)
        {
            var item = _db.MasterFile.Find(id);
            if (item==null)
            {
                return new MasterFileViewModel();
            }
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

            var chilDrs = _db.Folder.Where(a => a.Pid == item.Id);
            var childrenDir = chilDrs.Count() > 0 ? chilDrs.OrderBy(a=>a.Name).ToList() : new List<Folder>();

            var chilFls = item.Files;
            var childrenFil = chilFls.Count() > 0 ? chilFls.OrderBy(a => a.Name).ToList() : new List<MasterFile>();

            var uns = _db.MasterFile.Where(a => a.Folders.Count == 0);
            var unassigned = uns.Count() > 0 ? uns.ToList() : new List<MasterFile>();

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

        public ItemViewModel GetItemViewModel(string search, int scope)
        {
            Folder item = _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault();
            
            // Search scope 1
            var childrenDir = _db.Folder.Where(a => a.Name.Contains(search)).OrderBy(a => a.Name).ToList();
            var filesNumber = _db.MasterFile.Where(a=>a.Number.Contains(search)).OrderBy(a => a.Name).ToList();
            var filesName = _db.MasterFile.Where(a => a.Name.Contains(search)).OrderBy(a => a.Name).ToList();
            var childrenFil = filesNumber.Union(filesName);


            // Search scope 2 (includes comments and description)
            if (scope==2)
            {
                var filesDesc = _db.MasterFile.Where(a => a.Description.Contains(search)).OrderBy(a => a.Name).ToList();
                var filesComm = _db.MasterFile.Where(a => a.Comment.Contains(search)).OrderBy(a => a.Name).ToList();

                childrenFil = childrenFil.Union(filesDesc);
                childrenFil = childrenFil.Union(filesComm);
            }

            //var revFilesNam = _db.FileRevision.Where(a => a.Name.Contains(search)).Select(b => b.MasterFile).ToList();
            //var revFilesDraft = _db.FileRevision.Where(a => a.Draft.Contains(search)).Select(b => b.MasterFile).ToList();
            //var revFiles = revFilesNam.Union(revFilesDraft);
            //childrenFil = childrenFil.Union(revFiles);

            var fileList = childrenFil.ToList();

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
                ChildrenFiles = fileList,
                Breadcrumbs = bc,
                TreeNodes = list,
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

        public string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(System.IO.File.ReadAllBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}