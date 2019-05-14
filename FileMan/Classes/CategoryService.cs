using Raf.FileMan.Context;
using Raf.FileMan.Models;
using Raf.FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Raf.FileMan.Classes
{
    public class CategoryService
    {
        private AppDbContext _db;
        private ItemService _is;

        public CategoryService()
        {
            _db = new AppDbContext();
            _is = new ItemService();
        }

        /// <summary>
        /// Create categories (creates multiple if names separated with ';')
        /// </summary>
        /// <param name="item">New category model</param>
        /// <param name="userId">Current user ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> CreateAsync(Folder item, string userId)
        {
            string[] names = item.Name.Trim().Trim(';').Split(';').ToArray();
            var folders = new List<FolderJsonViewModel>();
            var clean = names.Where(a => !string.IsNullOrEmpty(a.Trim())).Select(s => s.Trim()).Distinct().ToList();
            var currentFolders = _db.Folder.Where(a => a.Pid == item.Pid).Select(b => b.Name.Trim().ToLower()).ToList();

            var user = _db.Users.Find(userId);

            try
            {
                foreach (string n in clean)
                {
                    string name = n.Trim();

                    if (currentFolders.Contains(name.ToLower()))
                    {
                        continue;
                    }

                    var added = DateTime.Now;
                    var changelog = string.Format("{0} - Category created \n", added);
                    var parent = _db.Folder.Find(item.Pid);
                    var parentPath = parent == null || parent.IsRoot ? "" : parent.Path;
                    var path = Path.Combine(parentPath, name);

                    var changelogParent = string.Format("{0} - Sub-category created : {1} \n", added, name);
                    if (parent != null)
                    {
                        string oldChng = parent.Changelog;
                        parent.Changelog = oldChng + changelogParent;
                    }

                    item.Name = name;
                    item.Parent = parent;
                    item.Path = path;
                    item.Added = added;
                    item.Changelog = changelog;
                    item.User = user;

                    _db.Folder.Add(item);
                    await _db.SaveChangesAsync();

                    folders.Add(new FolderJsonViewModel() { Id = item.Id, Name = item.Name });
                }

                return new StatusResult(true, StatusCode.Success, "Category(s) created", folders);
            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }
            
        }

        /// <summary>
        /// Move category and update all relative paths
        /// </summary>
        /// <param name="model">Category move model</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> MoveAsync(FolderMovelViewModel model)
        {
            var oldParent = _db.Folder.Find(model.OldParId);
            var current = _db.Folder.Find(model.Id);
            var newParent = _db.Folder.Find(model.NewParId);

            if (oldParent == null || current == null || newParent == null)
                return new StatusResult(false, StatusCode.CategoryNotFound, "Connot find all directories for the move");

            string oldParentPath = oldParent.Path;
            string newParentPath = newParent.IsRoot ? "" : newParent.Path;

            try
            {
                current.Parent = newParent;
                current.Pid = newParent.Id;
                await _db.SaveChangesAsync();
            } catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }

            try
            {
                await UpdatePathAsync(model.Id, newParentPath);
            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }

            return new StatusResult(true, StatusCode.Success, current.Name + " succesfully moved from " + oldParent.Name + " to " + newParent.Name);
        }

        /// <summary>
        /// Rename category and update all relative paths
        /// </summary>
        /// <param name="name">New Category name</param>
        /// <param name="id">Category ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> RenameAsync(string name, long id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new StatusResult(false, StatusCode.Error, "New name cannot be empty");
            }

            var folder = _db.Folder.Find(id);

            if (folder == null)
            {
                return new StatusResult(false, StatusCode.CategoryNotFound, "Cannot find category " + id);
            }
            try
            {
                string newName = name.Trim();
                string newPath = ReplaceLastOccurrence(folder.Path, folder.Name, "");
                folder.Name = newName;
                //folder.Path = newPath;
                await _db.SaveChangesAsync();

                if (!folder.IsRoot)
                {
                    await UpdatePathAsync(id, newPath);
                }

                return new StatusResult(true, StatusCode.Success, "Renamed to " + newName + " ok", newName);
            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }
        }
        
        /// <summary>
        /// Remove category and all its' children (if enabled)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="userId">Current user ID</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> DeleteAsync(int id, string userId)
        {
            var user = _db.Users.Find(userId);

            Folder item = await _db.Folder.FindAsync(id);
            long? pid = item.Pid;
            if (item==null)
                return new StatusResult(false, StatusCode.CategoryNotFound, "Cannot find specified category", pid);

            int files = item.Files.Count();
            int folders = _db.Folder.Where(a => a.Pid == id).Count();

            if (user.UserSetting.ForceDelete)
            {
                try
                {
                    await DeleteChildCategoriesAsync(item.Id);
                    return new StatusResult(true, StatusCode.Success, "Category and all content deleted", pid);
                }
                catch (Exception e)
                {
                    return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message, pid);
                }
            }
            else
            {
                if (files != 0 || folders != 0)
                {
                    return new StatusResult(false, StatusCode.Error, "Cannot delete non-empty directory", pid);
                }
                else
                {
                    try
                    {
                        _db.Folder.Remove(item);
                        await _db.SaveChangesAsync();
                        return new StatusResult(true, StatusCode.Success, "Category deleted", pid);
                    }
                    catch (Exception e)
                    {
                        return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message, pid);
                    }
                }
            }
        }

        /// <summary>
        /// Move documents inside selected category
        /// </summary>
        /// <param name="Id">Category ID</param>
        /// <param name="files">List of document IDs to be moved</param>
        /// <returns><param name="StatusResult">Simple result struct</param></returns>
        public async Task<StatusResult> MoveFilesAsync(int Id, int[] files)
        {
            Folder item = _db.Folder.Find(Id);

            if (files == null)
                return new StatusResult(false, StatusCode.Error, "Empty list of document");

            try
            {
                foreach (int i in files)
                {
                    MasterFile file = _db.MasterFile.Find(i);
                    file.Changelog = file.Changelog + string.Format("{0} - Document category change \n", DateTime.Now);
                    item.Files.Add(file);
                }
                await _db.SaveChangesAsync();

                return new StatusResult(true, StatusCode.Success, "Documents moved");
            }
            catch (Exception e)
            {
                return new StatusResult(false, StatusCode.ExceptionThrown, e.InnerException.Message);
            }
        }

        #region Privates

        private async Task UpdatePathAsync(long id, string newPath)
        {
            var folder = _db.Folder.Find(id);
            string path = Path.Combine(newPath, folder.Name);

            folder.Path = path;
            await _db.SaveChangesAsync();

            foreach (var ch in folder.Children)
                await UpdatePathAsync(ch.Id, path);
        }

        private string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        private async Task DeleteChildCategoriesAsync(long id)
        {
            var category = _db.Folder.Find(id);
            var children = category.Children.ToList();
            foreach (Folder subCategory in children)
            {
                if (subCategory.Children.Count() > 0)
                {
                    await DeleteChildCategoriesAsync(subCategory.Id);
                }
                else
                {
                    _db.Folder.Remove(subCategory);
                    await _db.SaveChangesAsync();
                }
            }
            _db.Folder.Remove(category);
            await _db.SaveChangesAsync();
        }
        #endregion
    }
}