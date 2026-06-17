using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBenhVien.Models
{
    public class DoctorDepartment
    {
        [Key]
        public int Id { get; set; }
        
        public int DoctorId { get; set; }
        public int DepartmentId { get; set; }

        [ForeignKey("DoctorId")]
        public User? Doctor { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
    }
}
