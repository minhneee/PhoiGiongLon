using SwineBreedingManager.Models;
using System.ComponentModel.DataAnnotations;

namespace SwineBreedingManager.Models
{
    public class FarrowingViewModel
    {
        public int BreedingRecordId { get; set; }
        public BreedingRecord? BreedingRecord { get; set; }

        [Required]
        [Display(Name = "Ngày đẻ thực tế")]
        [DataType(DataType.Date)]
        public DateTime ActualBirthDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Số con sống")]
        [Range(0, 30, ErrorMessage = "Số con sống phải từ 0 đến 30")]
        public int LitterSizeAlive { get; set; }

        [Required]
        [Display(Name = "Số con chết/tật")]
        [Range(0, 30, ErrorMessage = "Số con chết phải từ 0 đến 30")]
        public int LitterSizeDead { get; set; }

        [Display(Name = "Ghi chú thêm")]
        public string? Notes { get; set; }

        // Data for dynamic piglets
        public List<PigletCreateModel> Piglets { get; set; } = new List<PigletCreateModel>();
    }

    public class PigletCreateModel
    {
        [Required]
        public string TagNumber { get; set; } = string.Empty;
        
        [Required]
        public PigGender Gender { get; set; } = PigGender.Boar;
        
        public string? Breed { get; set; }

        [Required]
        public int PenId { get; set; }

        public decimal? Weight { get; set; }
    }
}
