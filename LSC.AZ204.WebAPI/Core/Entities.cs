using System.ComponentModel.DataAnnotations;

namespace LSC.AZ204.WebAPI.Core
{
    public class UserAudit
    {
        [Required]
        [MinLength(128), MaxLength(128)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [MinLength(128), MaxLength(128)]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public class CustomerContactUploads : UserAudit
    {
        public int Id { get; set; }
        [Required]
        public string FilePath { get; set; }
        public bool IsProcessed { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

}
