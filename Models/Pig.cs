using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwineBreedingManager.Models
{
    public enum PigGender
    {
        [Display(Name = "Đực")]
        Boar,
        [Display(Name = "Nái")]
        Sow
    }

    public enum PigStatus
    {
        [Display(Name = "Đang nuôi")]
        Active,
        [Display(Name = "Đã bán")]
        Sold,
        [Display(Name = "Để thịt")]
        Slaughtered,
        [Display(Name = "Loại thải")]
        Culled,
        [Display(Name = "Chết")]
        Dead
    }

    public class Pig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Số tai / Tag Number")]
        public string TagNumber { get; set; } = string.Empty;

        [Display(Name = "Tên")]
        public string? Name { get; set; }

        [Display(Name = "Ảnh Đại Diện")]
        public string? AvatarUrl { get; set; }

        [NotMapped]
        public string DisplayAvatarUrl => string.IsNullOrEmpty(AvatarUrl) 
            ? (Gender == PigGender.Boar ? "/img/LonDuc.png" : "/img/LonCai.png") 
            : AvatarUrl;

        [Required]
        [Display(Name = "Giới tính")]
        public PigGender Gender { get; set; }

        [Required]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Giống")]
        public string? Breed { get; set; }

        [Display(Name = "Trạng thái")]
        public PigStatus Status { get; set; } = PigStatus.Active;

        [Display(Name = "Cân nặng (kg)")]
        public decimal? Weight { get; set; }

        [NotMapped]
        public int AgeInDays => (DateTime.Now - BirthDate).Days;

        [NotMapped]
        public string FullDisplayInfo => $"{TagNumber} - {Breed} - ({AgeInDays} ngày)";

        [Display(Name = "Heo Cha")]
        public int? FatherId { get; set; }
        [ForeignKey("FatherId")]
        public virtual Pig? Father { get; set; }

        [Display(Name = "Heo Mẹ")]
        public int? MotherId { get; set; }
        [ForeignKey("MotherId")]
        public virtual Pig? Mother { get; set; }

        [Display(Name = "Chuồng Xếp")]
        public int? PenId { get; set; }
        [ForeignKey("PenId")]
        public virtual Pen? Pen { get; set; }

        [InverseProperty("Boar")]
        public virtual ICollection<BreedingRecord>? BreedingRecordsAsBoar { get; set; }
        [InverseProperty("Sow")]
        public virtual ICollection<BreedingRecord>? BreedingRecordsAsSow { get; set; }

        [InverseProperty("Pig")]
        public virtual SaleRecord? SaleRecord { get; set; }
    }
}
