using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileMan.Models
{
    [Table("FileRevision")]
    public class FileRevision
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }

        [ForeignKey("MasterFile")]
        public Int64? MasterFileId { get; set; }
        public virtual MasterFile MasterFile { get; set; }
        [Required]
        public long Revision { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string Comment { get; set; }
        public DateTime? Added { get; set; }
        public string Draft { get; set; }
    }
}