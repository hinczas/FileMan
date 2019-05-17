using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Raf.FileMan.Models
{
    [Table("NavigationHistory")]
    public class NavigationHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public long? ItemId { get; set; }
        public string ItemIdStr { get; set; }
        public string Search { get; set; }
        public int? Scope { get; set; }
        public long? ParentId { get; set; }
        public string ParentIdStr { get; set; }
        public string JSFunction { get; set; }
        public DateTime ActionDate { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public string SessionId { get; set; }

        public NavigationHistory(string UserId, string SessionId)
        {
            this.UserId = UserId;
            this.SessionId = SessionId;
            this.ActionDate = DateTime.Now;
        }

        public NavigationHistory()
        {
        }
    }
}