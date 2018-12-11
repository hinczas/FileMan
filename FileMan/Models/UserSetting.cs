using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileMan.Models
{
    [Table("UserSettings")]
    public class UserSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }
        
        public bool ShowChangelog { get; set; }
        public bool ShowUncategorisedRoot { get; set; }
        public bool UncategorisedVisible { get; set; }        
    }
}