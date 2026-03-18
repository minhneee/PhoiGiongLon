using System.ComponentModel.DataAnnotations;

namespace SwineBreedingManager.Models
{
    public class Pen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên chuồng")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Sức chứa tối đa")]
        public int Capacity { get; set; } = 10;

        [Display(Name = "Mô tả / Loại chuồng")]
        public string? Description { get; set; }

        public virtual ICollection<Pig>? Pigs { get; set; }
    }
}
