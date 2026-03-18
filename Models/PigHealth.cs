using System.ComponentModel.DataAnnotations;

namespace SwineBreedingManager.Models
{
    public class PigHealth
    {
        public int Id { get; set; }
        public int PigId { get; set; }
        public virtual Pig? Pig { get; set; }
        
        [Display(Name = "Ngày kiểm tra")]
        [DataType(DataType.Date)]
        public DateTime CheckDate { get; set; }
        
        [Display(Name = "Chẩn đoán")]
        public string? Diagnosis { get; set; }
        
        [Display(Name = "Điều trị")]
        public string? Treatment { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }
}
