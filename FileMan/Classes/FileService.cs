using FileMan.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FileMan.Classes
{
    public class FileService
    {
        private string _path = ""; // Empty
        private char _delim = '\t'; // Default comma
        private bool _head = true; // Default expect header row
        private Dictionary<string, int> _headers;

        private ItemService _is;

        #region Constructors

        public FileService()
        {
            _is = new ItemService();
        }

        public FileService(string path, bool header = true)
        {
            _path = path;
            _head = header;
            _headers = new Dictionary<string, int>();
        }
        public FileService(char delimiter, bool header = true)
        {
            _delim = delimiter;
            _head = header;
            _headers = new Dictionary<string, int>();
        }
        public FileService(bool header)
        {
            _head = header;
            _headers = new Dictionary<string, int>();
        }
        public FileService(string path, char delimiter, bool header = true)
        {
            _path = path;
            _delim = delimiter;
            _head = header;
            _headers = new Dictionary<string, int>();
        }

        #endregion

        public async Task<FileResult> ImportDocsAsync(HttpPostedFileBase file, string typ, long? pid)
        {
            List<Folder> folders = new List<Folder>();
            
            if (pid != null)
            {
                var parent = _is.GetFolderById((long)pid);
                folders = new List<Folder>() { parent };
            }

            //

            using (var streamReader = new StreamReader(file.InputStream, Encoding.UTF8))
            {
                bool first = true;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    // prepare headers
                    if (first && _head)
                    {
                        bool headers = DoHeaders(line.ToLower(), FileResult.DocFileType);

                        if (!headers)
                            return FileResult.HeadersMissing;

                        first = false;
                        continue;
                    }

                    // Decide parenting
                    if (pid == null)
                        folders = GetParents(line);

                    // Get row data
                    string[] row = line.Split(_delim);

                    // Finally create folder
                    MasterFile item = new MasterFile()
                    {
                        Name = row[_headers["name"]],
                        Description = row[_headers["description"]],
                        Comment = row[_headers["comment"]]
                    };

                    await _is.CreateDocumentAsync(item, folders);

                }
            }

            return FileResult.Success;
        }

        public async Task<FileResult> ImportCatsAsync(HttpPostedFileBase file, string typ, long? pid)
        {
            long _pid;

            var root = _is.GetRoot();
            if (root == null)
                return FileResult.Failure;

            if (pid == null)
                _pid = root.Id;
            else
                _pid = (long)pid;

            //var parent = _is.GetFolderById((long)_pid);

            using (var streamReader = new StreamReader(file.InputStream, Encoding.UTF8))
            {
                bool first = true;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    // prepare headers
                    if(first && _head)
                    {
                        bool headers = DoHeaders(line.ToLower(), FileResult.CatFileType);

                        if (!headers)
                            return FileResult.HeadersMissing;

                        first = false;
                        continue;
                    }
                    
                    // Decide parenting
                    if (pid == null)
                        _pid = GetPid(line, _pid);

                    // Get row data
                    string[] row = line.Split(_delim);

                    // Finally create folder
                    Folder item = new Folder()
                    {
                        Pid = _pid,
                        Type = typ,
                        Name = row[_headers["name"]],
                        Description = row[_headers["description"]],
                        Comment = row[_headers["comment"]]
                    };

                    await _is.CreateFolderAsync(item);

                }
            }

            return FileResult.Success;
        }

        private long GetPid(string line, long pid)
        {
            string[] row = line.Split(_delim);

            // Check if Pid column in CSV file
            if (!_headers.ContainsKey("pid"))
            {
                return pid;
            }

            // Check if Pid value in CSV file
            string itm = row[_headers["pid"]];
            if (string.IsNullOrEmpty(itm))
            {
                return pid;
            }

            // try to find parent folder
            var parent = _is.GetFolderByPath(itm);
            if (parent == null)
                return pid;


            // Finally return parent Id
            return parent.Id;
        }

        private List<Folder> GetParents(string line)
        {
            string[] row = line.Split(_delim);

            // Check if Pid column in CSV file
            if (!_headers.ContainsKey("pid"))
            {
                return null;
            }

            // Check if Pid value in CSV file
            string itm = row[_headers["pid"]];
            if (string.IsNullOrEmpty(itm))
            {
                return null;
            }

            // try to find parent folders
            string[] ps = itm.ToLower().Split(',');
            List<Folder> results = new List<Folder>();

            foreach(string p in ps)
            {
                var parent = _is.GetFolderByPath(p.Trim());
                if (parent != null)
                    results.Add(parent);
            }      

            // Finally return parent Id
            return results;
        }

        private bool DoHeaders(string line, FileResult fileType)
        {
            string[] heads = line.Split(_delim);

            // check mandatory fields
            if (!heads.Contains("name"))
                return false;
            if (!heads.Contains("description"))
                return false;
            if (!heads.Contains("comment"))
                return false;

            // check headers unique
            var cnt = heads.Count();
            var cntDist = heads.Distinct().Count();
            if (cnt != cntDist)
                return false;

            // Parse headers into dictionary
            _headers = heads.ToArray().Select((value, index) => new { value, index })
                        .ToDictionary(pair => pair.value, pair => pair.index);

            return true;
        }
    }

    

    public enum FileResult
    {
        FileNotFound,
        DirectoryNotFound,
        CannotCreateFile,
        CannotCreateDirectory,
        FileCreated,
        Success,
        Failure,
        HeadersMissing,
        HeadersNonUnique,
        DocFileType,
        CatFileType
    }
}