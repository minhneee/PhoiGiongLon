using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwineBreedingManager.Models
{
    public class SaleRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Heo")]
        public int PigId { get; set; }
        
        [ForeignKey("PigId")]
        public virtual Pig? Pig { get; set; }

        [Required]
        [Display(Name = "Ngày bán")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Display(Name = "Cân nặng (kg)")]
        public decimal Weight { get; set; }

        [Required]
        [Display(Name = "Giá bán (VNĐ)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Khách hàng")]
        public string? CustomerName { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }
}
