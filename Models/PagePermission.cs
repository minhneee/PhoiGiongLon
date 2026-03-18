using System.ComponentModel.DataAnnotations;

namespace SwineBreedingManager.Models
{
    public class PagePermission
    {
        public int Id { get; set; }

        [Required]
        public string PageName { get; set; } = string.Empty;

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        public bool IsAllowed { get; set; } = true;

        [Required]
        public string ControllerName { get; set; } = string.Empty;

        public string? UserId { get; set; }
    }
}
