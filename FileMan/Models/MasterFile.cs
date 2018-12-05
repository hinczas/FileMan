using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileMan.Models
{
    [Table("MasterFile")]
    public class MasterFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }

        [Required]
        public string Number { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public string Changelog { get; set; }
        public DateTime? Added { get; set; }
        public DateTime? Edited { get; set; }
        public long? Issue { get; set; }

        public virtual List<Folder> Folders { get; set; }
        public virtual List<FileRevision> Revisions { get; set; }
    }
}