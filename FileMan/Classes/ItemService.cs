using FileMan.Context;
using FileMan.Models;
using FileMan.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

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

        public MasterFileViewModel GetMasterFileViewModel(long id)
        {
            var item = _db.MasterFile.Find(id);
            MasterFileViewModel file = new MasterFileViewModel()
            {
                Current = item,
                Breadcrumbs = GetFileBreadcrumbs((long)item.FolderId)
            };
            return file;
        }
        /// <summary>
        /// Return Item View Model for index page
        /// </summary>
        /// <param name="id">Optional current item ID</param>
        /// <returns>ItemViewModel</returns>
        public ItemViewModel GetItemViewModel(long? id)
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
            var childrenDir = _db.Folder.Where(a => a.Pid == item.Id).ToList();
            var childrenFil = _db.MasterFile.Where(a => a.FolderId == item.Id).ToList();
            var bc = GetBreadcrumbs(item.Id);

            ItemViewModel ivm = new ItemViewModel()
            {
                Current = item,
                ChildrenDirs = childrenDir,
                ChildrenFiles = childrenFil,
                Breadcrumbs = bc
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
            List<MasterFile> files = _db.MasterFile.Where(a => a.FolderId == id).ToList();
            List<Folder> dirs = _db.Folder.Where(a=>a.Pid==id).ToList();

            foreach (MasterFile file in files)
            {
                DeleteFile(file.Id);
            }

            foreach (Folder dir in dirs)
            {
                DeleteDirectory(dir.Id);
            }

            var folder = _db.Folder.Find(id);

            _db.Folder.Remove(folder);
            _db.SaveChanges();
        }

        public void DeleteFile(long id)
        {

            var mf = _db.MasterFile.Find(id);
            var revisions = mf.Revisions.ToList();

            foreach (FileRevision fr in revisions)
            {
                System.IO.File.SetAttributes(fr.FullPath, FileAttributes.Normal);
                System.IO.File.Delete(fr.FullPath);
                _db.FileRevision.Remove(fr);
            }
            _db.SaveChanges();

            Directory.Delete(mf.Path);

            _db.MasterFile.Remove(mf);
            _db.SaveChanges();

        }
    }
}