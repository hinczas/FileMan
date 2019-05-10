using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Models
{
    [Table("Folder")]
    public class Folder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }

        public Int64? Pid { get; set; }
        public virtual Folder Parent { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public string Changelog { get; set; }
        public DateTime? Added { get; set; }
        public DateTime? Edited { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual List<MasterFile> Files { get; set; }
        public virtual List<Folder> Children { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Folder);
        }

        public bool Equals(Folder other)
        {
            if (other == null)
                return false;

            return this.Id==other.Id && this.Name.Equals(other.Name) && this.Path.Equals(other.Path);
        }

        public override int GetHashCode()
        {

            int name = Name.GetHashCode();
            int path = Path.GetHashCode();
            int id = Id.GetHashCode();

            return name ^ path ^ id;
        }

        public bool IsRoot
        {
            get
            {
                if (Type.Equals("root"))
                    return true;

                return false;
            }
        }
    }
}