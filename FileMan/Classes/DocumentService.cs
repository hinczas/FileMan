using Raf.FileMan.Context;
using Raf.FileMan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Raf.FileMan.Classes
{
    public class DocumentService
    {
        private AppDbContext _db;
        private ItemService _is;

        public DocumentService()
        {
            _db = new AppDbContext();
            _is = new ItemService();
        }

        /// <summary>
        /// Create new Document asynchronously
        /// </summary>
        /// <param name="item">New document model</param>
        /// <param name="FolderId">Parent directory ID</param>
        /// <param name="userId">Current User ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> CreateAsync(MasterFile item, long FolderId, string userId)
        {
            Folder parent = _db.Folder.Where(a => a.Id == FolderId).FirstOrDefault();
            var user = _db.Users.Find(userId);
            var added = DateTime.Now;
            var changelog = string.Format("{0} - Item created \n", added);

            try
            {
                item.Added = added;
                item.Changelog = changelog;
                item.User = user;

                _db.MasterFile.Add(item);
                await _db.SaveChangesAsync();

            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }
                       
            var changelogParent = string.Format("{0} - File created : {1} \n", added, item.Name);
            string path = "";
            if (parent != null)
            {
                string oldChng = parent.Changelog;
                parent.Changelog = oldChng + changelogParent;
                path = parent.IsRoot ? "" : parent.Path;
            }


            string number = item.Id.ToString().PadLeft(9, '0');

            item.Number = number;

            var rootPath = _is.GetRoot().Path;
            path = Path.Combine(path, number);
            string extension = "";
            try
            {
                Directory.CreateDirectory(Path.Combine(rootPath, number));
            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.CannotCreateDirectory, e.InnerException.Message);
            }

            try
            {
                item.Path = path;
                item.Added = added;
                item.Changelog = changelog;
                item.Extension = extension;
                item.Folders = new List<Folder>() {
                    parent
                };
                //_db.MasterFile.Add(item);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }

            return new StatusResult(true, StatusCode.Success, "Document created");
        }

        /// <summary>
        /// Lock or unlock document for all users except user who locked
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="isLocked">Lock/Unlock flag (true for lock)</param>
        /// <param name="userId">Current user ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> LockAsync(long id, bool isLocked, string userId)
        {
            MasterFile item = await _db.MasterFile.FindAsync(id);

            if (item == null)
                return new StatusResult(false, StatusCode.DocumentNotFound, "Cannot find specified Document");

            var user = _db.Users.Find(userId);

            if (item.Locked && !item.UserLock.Equals(userId))
            {
                var lUser = _db.Users.Find(item.UserLock);
                string name = lUser == null ? "unknown" : string.Format("{0}, {1}", lUser.Surname, lUser.FirstName);
                return new StatusResult(false, StatusCode.DocumentLocked, "Document locked by another user (" + name + ")");
            }

            if (item.Locked && isLocked)
                return new StatusResult(false, StatusCode.DocumentLocked, "Document already locked out");

            if (!item.Locked && !isLocked)
                return new StatusResult(false, StatusCode.DocumentLocked, "Document already unlocked");

            var date = DateTime.Now;


            try
            {
                if (isLocked)
                {
                    item.UserLock = userId;
                    item.Changelog = item.Changelog + string.Format("{0} - Document locked by {1}, {2} \n", date, user.Surname, user.FirstName);
                    await _db.SaveChangesAsync();
                    return new StatusResult(true, StatusCode.Success, string.Format("{0} locked by {1}, {2}", item.Number, user.Surname, user.FirstName));
                }
                else
                {
                    item.UserLock = null;
                    item.Changelog = item.Changelog + string.Format("{0} - Document unlocked by {1}, {2} \n", date, user.Surname, user.FirstName);
                    await _db.SaveChangesAsync();
                    return new StatusResult(true, StatusCode.Success, string.Format("{0} unlocked by {1}, {2}", item.Number, user.Surname, user.FirstName));
                }
            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.Message);
            }
        }

        /// <summary>
        /// Edit document details
        /// </summary>
        /// <param name="item">Document model</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> EditAsync(MasterFile item)
        {
            try
            {
                var date = DateTime.Now;
                MasterFile mf = await _db.MasterFile.FindAsync(item.Id);
                mf.Name = item.Name;
                mf.Description = item.Description;
                mf.Comment = item.Comment;
                mf.Edited = date;
                mf.Changelog = mf.Changelog + string.Format("{0} - Document edited \n", date);

                await _db.SaveChangesAsync();
                return new StatusResult(true, StatusCode.Success, item.Number + " document updated");

            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, item.Number +": "+ e.Message);
            }

        }

        /// <summary>
        /// Remove document from selected category
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="folderId">Parent category ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> DeleteAsync(int id, int folderId)
        {
            try
            {
                MasterFile item = await _db.MasterFile.FindAsync(id);

                if (folderId < 0)
                {
                    var fols = item.Folders.Select(s => s.Id).ToArray();
                    foreach (int fol in fols)
                    {
                        Folder folder = _db.Folder.Find(fol);
                        item.Folders.Remove(folder);
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    Folder folder = _db.Folder.Find(folderId);
                    item.Folders.Remove(folder);
                    await _db.SaveChangesAsync();
                }

                return new StatusResult(true, StatusCode.Success, item.Number + " document removed");
            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.Message);
            }

        }

        /// <summary>
        /// Promote document Draft to Issue
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="Comment">Promote comment</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> PromoteAsync(long id, string Comment)
        {
            MasterFile item = await _db.MasterFile.FindAsync(id);

            if (item==null)
                return new StatusResult(false, StatusCode.DocumentNotFound, "Cannot find given document");


            long curIssue = item.Issue == null ? 0 : (long)item.Issue;
            long nextIssue = curIssue + 1;
            long issue = nextIssue;

            try
            {
                FileRevision fr = _db.FileRevision.Where(a => a.MasterFileId == id).OrderByDescending(b => b.Id).Take(1).FirstOrDefault();

                item.Issue = issue;
                item.Changelog = item.Changelog + string.Format("{0} - Promoted to Issue : {1} \n", DateTime.Now, issue);
                if (!string.IsNullOrEmpty(Comment))
                {
                    string comm = string.IsNullOrEmpty(item.Comment) ? Comment : item.Comment + "\n" + Comment;
                    item.Comment = comm;
                }
                if (fr != null)
                {
                    fr.Draft = issue.ToString();
                    fr.Type = "issue";
                }

                await _db.SaveChangesAsync();

                return new StatusResult(true, StatusCode.Success, "Document "+item.Number+" promoted.");
            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }            
        }

        /// <summary>
        /// Move document between categories
        /// </summary>
        /// <param name="Id">Document ID</param>
        /// <param name="folders">Categories to add to or remove from </param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> MoveAsync(long Id, long[] folders)
        {
            try
            {
                MasterFile master = await _db.MasterFile.FindAsync(Id);

                var assigned = master.Folders.Select(a => a.Id).ToArray();

                var affected = folders == null ? assigned : assigned.Union(folders).ToArray();

                // Remove from all categories
                if (folders == null || folders.Count() == 0)
                {
                    master.Folders = new List<Folder>();
                    master.Changelog = master.Changelog + string.Format("{0} - Uncategorised \n", DateTime.Now);

                    await _db.SaveChangesAsync();

                    return new StatusResult(true, StatusCode.Success, master.Number + " document uncategorised");
                }

                // Remove from selected categories
                var toAdd = folders.Except(assigned);
                var toDel = assigned.Except(folders);
                                 
                foreach (int i in toDel)
                {
                    Folder folder = _db.Folder.Find(i);
                    master.Folders.Remove(folder);
                }
                await _db.SaveChangesAsync();

                foreach (int i in toAdd)
                {
                    Folder folder = _db.Folder.Find(i);
                    master.Folders.Add(folder);
                }

                master.Changelog = master.Changelog + string.Format("{0} - Document category change \n", DateTime.Now);
                await _db.SaveChangesAsync();

                return new StatusResult(true, StatusCode.Success, master.Number + " document updated");
            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.Message);
            }

        }

        /// <summary>
        /// Moves document between 2 categories
        /// </summary>
        /// <param name="Id">Document ID</param>
        /// <param name="opid">Old parent directory ID</param>
        /// <param name="npid">New parent directory ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> MoveOneAsync(long Id, long opid, long npid)
        {
            try
            {
                MasterFile master = await _db.MasterFile.FindAsync(Id);
                Folder oldPa = await _db.Folder.FindAsync(opid);
                Folder newPa = await _db.Folder.FindAsync(npid);

                if (master == null)
                    return new StatusResult(false, StatusCode.DocumentNotFound, "Cannot find source document " + Id);

                if (oldPa == null)
                    return new StatusResult(false, StatusCode.CategoryNotFound, "Cannot find source category " + opid);

                if (newPa == null)
                    return new StatusResult(false, StatusCode.CategoryNotFound, "Cannot find target category " + npid);

                if (master.Folders == null)
                {
                    master.Folders = new List<Folder>();
                    await _db.SaveChangesAsync();
                }

                master.Folders.Remove(oldPa);
                master.Folders.Add(newPa);
                await _db.SaveChangesAsync();

                return new StatusResult(true, StatusCode.Success, "Document "+master.Number + " moved");

            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }

        }
    }
}