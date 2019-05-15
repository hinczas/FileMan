using Raf.FileMan.Models;
using Raf.FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Raf.FileMan.Context;

namespace Raf.FileMan.Classes
{
    public class ItemService
    {
        private AppDbContext _db;

        public ItemService()
        {
            _db = new AppDbContext();
        }

        /// <summary>
        /// Get breadcrumbs (links for all parents of an Item)
        /// </summary>
        /// <param name="id">Current directory ID</param>
        /// <returns>List<Item> - list of all parent direcotries</returns>
        private List<BreadItem> GetBreadcrumbs(long id)
        {
            var item = _db.Folder.Find(id);
            List<BreadItem> bc = new List<BreadItem>();
            while (item != null)
            {
                var br = new BreadItem()
                {
                    Name = item.Name,
                    Id = item.Id,
                    HasChildren = false,
                    Current = id == item.Id
                };

                if (item.Children != null && item.Children.Count > 0)
                {
                    var tmp = item.Children.OrderBy(o => o.Name).Select(a => new BreadItem()
                    {
                        Name = a.Name,
                        Id = a.Id
                    })
                    .ToList();
                    br.HasChildren = true;
                    br.Children = tmp;
                }
                bc.Add(br);

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

        public MasterFileViewModel GetMasterFileViewModel(long id, string userId, SessionState ss, long? pid)
        {
            var item = _db.MasterFile.Find(id);
            if (item==null)
            {
                return new MasterFileViewModel();
            }
            var revisions = item.Revisions;
            var latRev = revisions.Count == 0 ? 0 : revisions.Select(a => a.Revision).Max();
            var revId = revisions.Count == 0 ? 0 : revisions.OrderByDescending(a => a.Revision).Take(1).Select(b => b.Id).FirstOrDefault();
            var revCmnt = revisions.Count == 0 ? "" : revisions.OrderByDescending(a => a.Revision).Take(1).Select(b => b.Comment).FirstOrDefault();
            var revNam = revisions.Count == 0 ? "N/A" : revisions.Where(a => a.Id == revId).FirstOrDefault().Name;
            var revCnt = revisions.Count == 0 ? 0 : revisions.Count();

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

            ApplicationUser user = GetASPUser(userId);
            bool changelog = user.UserSetting.ShowChangelog;

            bool promote = !issue.Equals(draft) && revisions.Count() != 0;

            var lockUser = _db.Users.Find(item.UserLock);

            string lu = lockUser == null ? "unknown" : string.Format("{0}, {1}", lockUser.Surname, lockUser.FirstName);

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
                FolderList = foldersList,
                Promote = promote,
                ShowChangelog = changelog,
                Author = string.Format("{0}, {1}", item.User.Surname, item.User.FirstName),
                Locked = item.Locked,
                LockedBy= lu,
                Lockable = !item.Locked || item.UserLock.Equals(userId),
                LatestRevisionComm = revCmnt
            };
                                 
            Folder par;
            if (pid!=null && pid > 0)
            {
                par = _db.Folder.Find(pid);
            } else
            {
                par = _db.Folder.Find(ss.CatId);
            }

            file.RedirectId = par.Id;
            file.RedirectLabel = par.Name;

            string retFun = "";

            //switch(ss.ReturnTo)
            //{
            //    case "folder":
            //        retFun = string.Format("goToFolder({0})", ss.ReturnId);
            //        break;
            //    case "file":
            //        retFun = string.Format("goToFile({0},{1})", ss.ReturnId, ss.CatId);
            //        break;
            //    case "search":
            //        retFun = string.Format("goToSearch({0},'{1}',{2})", ss.CatId, ss.Search, ss.Scope);
            //        file.RedirectLabel = "search";
            //        break;
            //    default:
            //        retFun = string.Format("goToFolder({0})", par.Id);
            //        break;
            //}

            retFun = string.Format("goToFolder({0})", ss.CatId);

            file.RedirectFun = retFun;

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
                item = GetRoot();
            }
            else
            {
                item = _db.Folder.Find(id);
            }
            
            ApplicationUser user = GetASPUser(userId);
            bool uncatVisible = user.UserSetting.UncategorisedVisible;
            bool showUncatRoot = user.UserSetting.ShowUncategorisedRoot;

            var chilDrs = _db.Folder.Where(a => a.Pid == item.Id);
            var childrenDir = chilDrs.Count() > 0 ? chilDrs.OrderBy(a=>a.Name).ToList() : new List<Folder>();

            var chilFls = item.Files;
            var childrenFil = chilFls !=null && chilFls.Count() > 0 ? chilFls.OrderBy(a => a.Name).ToList() : new List<MasterFile>();

            var uns = _db.MasterFile.Where(a => a.Folders.Count == 0);
            var unassigned = uns.Count() > 0 ? uns.ToList() : new List<MasterFile>();

            if (uncatVisible)
            {
                if(showUncatRoot)
                {
                    if(item.IsRoot)
                    {
                        childrenFil = childrenFil.Union(unassigned).OrderBy(a => a.Name).ToList();
                    }
                }                
            }

            
            var bc = GetBreadcrumbs(item.Id);

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
                UnassignedFiles = uncatVisible && !showUncatRoot ? unassigned : null,
                FolderList = foldersList
            };
            return ivm;
        }

        public ItemViewModel GetPartialItemViewModel(long? id)
        {
            Folder item;
            if (id == null)
            {
                item = GetRoot();
            }
            else
            {
                item = _db.Folder.Find(id);
            }
            
            var chilDrs = _db.Folder.Where(a => a.Pid == item.Id);
            var childrenDir = chilDrs.Count() > 0 ? chilDrs.OrderBy(a => a.Name).ToList() : new List<Folder>();
                          
            ItemViewModel ivm = new ItemViewModel()
            {
                Current = item,
                ChildrenDirs = childrenDir
            };
            return ivm;
        }

        public ItemViewModel GetItemViewModel(string search, int scope)
        {
            Folder item = GetRoot();
            
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
                FolderList = foldersList
            };
            return ivm;
        }

        public long CreateRoot(string userId)
        {
            string path = ConfigurationManager.AppSettings["ROOT_DIR"];
            var user = _db.Users.Find(userId);
            Directory.CreateDirectory(path);
            Folder item = new Folder()
            {
                Name = "root",
                Type= "root",
                Path = path,
                Description = "Root directory",
                Added = DateTime.Now,
                User = user
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
            var rootPath = GetRoot().Path;
            var mf = _db.MasterFile.Find(id);
            var path = Path.Combine(rootPath, mf.Number);

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

        public Folder GetRoot()
        {
            return _db.Folder.Where(a => a.Type.Equals("root")).FirstOrDefault();
        }

        public UserSideBarVM GetSidebar(string userId)
        {
            var favs = GetFavs(userId);
            var locks = GetLocks(userId);

            var result = new UserSideBarVM()
            {
                Favs = favs,
                Locks = locks
            };

            return result;
        }


        private List<LinkItem> GetFavs(string userId)
        {
            var result = new List<LinkItem>();
            var folders = new List<LinkItem>();
            var files = new List<LinkItem>();
            var user = _db.Users.Find(userId);

            if (user == null)
                return result;

            if (user.Favourites == null || user.Favourites.Count < 1)
                return result;

            foreach (var fav in user.Favourites.OrderBy(a=>a.ItemType).ToList())
            {
                // Category
                if (fav.ItemType == ItemType.Category)
                {
                    var folder = _db.Folder.Find(fav.ItemId);

                    var item = new LinkItem()
                    {
                        Icon = "fa-folder", // use FontAwesome icon notation
                        Label = folder.Name,
                        Extra = folder.Path,
                        Action = "goToFolder(" + folder.Id + ")"
                    };
                    folders.Add(item);
                }

                // Document
                if (fav.ItemType == ItemType.Document)
                {
                    var file = _db.MasterFile.Find(fav.ItemId);

                    var item = new LinkItem()
                    {
                        Icon = "fa-file", // use FontAwesome icon notation
                        Label = file.Number,
                        Extra = file.Name,
                        Action = "goToFile(" + file.Id + ")"
                    };
                    files.Add(item);
                }
            }

            result.AddRange(folders.OrderBy(o => o.Label).ToList());
            result.AddRange(files.OrderBy(o => o.Label).ToList());

            return result;
        }

        private List<LinkItem> GetLocks(string userId)
        {
            var locks = _db.MasterFile.Where(a => a.UserLock.Equals(userId));

            if (locks.Count() < 1)
                return new List<LinkItem>();

            var result = locks.Select(a => new LinkItem()
            {
                Icon = "fa-file", // use FontAwesome icon notation
                Label = a.Number,
                Extra = a.Name,
                Action = "goToFile("+a.Id+")"
            })
            .OrderBy(o => o.Label)
            .ToList();

            return result;
        }

        //public Folder GetFolderById(long id)
        //{
        //    return _db.Folder.Find(id);
        //}

        //public Folder GetFolderByName(string name)
        //{
        //    var list = _db.Folder.Where(a => a.Name.ToLower().Equals(name.ToLower())).ToList();
        //    if (list==null || list.Count() != 1)
        //        return null;

        //    return list.First();
        //}

        //public Folder GetFolderByPath(string path)
        //{
        //    path = path.Replace('/', '\\');

        //    var list = _db.Folder.Where(a => a.Path.ToLower().Equals(path.ToLower())).ToList();
        //    if (list == null || list.Count() != 1)
        //        return null;

        //    return list.First();
        //}

        private ApplicationUser GetASPUser(string userId)
        {
            return _db.Users.Find(userId);
        }

        public JSTNode JSTree(long id)
        {
            var root = GetRoot();

            return JSTree(root.Id, id);
        }
        public JSTNode JSTree(long id, long curId)
        {
            var fol = _db.Folder.Find(id);

            var tree = new JSTNode()
            {
                id = fol.Id.ToString(),
                text = fol.Name,
                icon = "fas fa-folder",
                state = new JSTState()
                {
                    opened = false,
                    disabled = false,
                    selected = id == curId
                },
                a_attr = new JSTAAttr()
                {
                    href = "/Home/TreeIndex/"+id,
                    data_quantity = fol.Files == null || fol.Files.Count < 1 ? "" : string.Format("({0})",fol.Files.Count)
                }
                
            };

            if (fol.Children.Count>0)
            {
                List<JSTNode> childs = new List<JSTNode>();
                foreach(var c in fol.Children.OrderBy(a => a.Name).ToList())
                {
                    childs.Add(JSTree(c.Id, curId));
                }

                tree.children = childs;
            }

            return tree;
        }
    }
}