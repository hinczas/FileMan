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

        public virtual ApplicationUser User { get; set; }

        public virtual List<Folder> Folders { get; set; }
        public virtual List<FileRevision> Revisions { get; set; }


        public override bool Equals(object obj)
        {
            return this.Equals(obj as MasterFile);
        }

        public bool Equals(MasterFile other)
        {
            if (other == null)
                return false;

            return this.Id == other.Id && this.Name.Equals(other.Name) && this.Number.Equals(other.Number);
        }

        public override int GetHashCode()
        {

            int name = Name.GetHashCode();
            int number = Number.GetHashCode();
            int id = Id.GetHashCode();

            return name ^ number ^ id;
        }
    }
}