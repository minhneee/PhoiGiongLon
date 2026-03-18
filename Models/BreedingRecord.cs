using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwineBreedingManager.Models
{
    public class BreedingRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Heo Đực")]
        public int BoarId { get; set; }
        [ForeignKey("BoarId")]
        public virtual Pig? Boar { get; set; }

        [Required]
        [Display(Name = "Heo Nái")]
        public int SowId { get; set; }
        [ForeignKey("SowId")]
        public virtual Pig? Sow { get; set; }

        [Required]
        [Display(Name = "Ngày phối giống")]
        [DataType(DataType.Date)]
        public DateTime BreedingDate { get; set; }

        [Display(Name = "Ngày dự kiến sinh")]
        [DataType(DataType.Date)]
        public DateTime? EstimatedBirthDate { get; set; }

        [Display(Name = "Ngày sinh thực tế")]
        [DataType(DataType.Date)]
        public DateTime? ActualBirthDate { get; set; }

        [Display(Name = "Ngày cai sữa")]
        [DataType(DataType.Date)]
        public DateTime? WeaningDate { get; set; }

        [Display(Name = "Số con sống")]
        public int? LitterSizeAlive { get; set; }

        [Display(Name = "Số con chết")]
        public int? LitterSizeDead { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Chuồng phối/đẻ")]
        public int? PenId { get; set; }
        [ForeignKey("PenId")]
        public virtual Pen? Pen { get; set; }
    }
}
