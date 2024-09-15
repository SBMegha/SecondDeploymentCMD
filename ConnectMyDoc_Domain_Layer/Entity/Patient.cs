
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ConnectMyDoc_Domain_Layer.Manager;




namespace ConnectMyDoc_Domain_Layer.Entity
{
    public class Patient
    {
        [Required]
        [Key]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Patient name is required.")]
        [MinLength(3, ErrorMessage = "Patient name must be at least 3 characters long.")]
        [MaxLength(100, ErrorMessage = "Patient name can't exceed 100 characters.")]
        public string PatientName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]

        [StringLength(15, ErrorMessage = "Phone number must be between 4 and 15 digits.", MinimumLength = 4)]
        [RegularExpression(@"^\+\d{1,3}[- ]?\d{1,14}$", ErrorMessage = "Phone number must start with a '+' followed by the country code and valid digits.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]

        public int Age { get; set; }
        [Required]
        public DateTime Dob { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime PreferredStartTime { get; set; }
        [Required]
        [CustomValidation(typeof(Helper), "ValidateTimeRange")]  // Apply custom validation
        public DateTime PreferredEndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public int PreferredClinicId { get; set; }

        public int PreferredDoctorId { get; set; }
        //For image
        public byte[]? Image { get; set; }

        [Required]
        [ForeignKey(nameof(PatientAddress))]
        public int PatientAddressId { get; set; }
        public virtual PatientAddress PatientAddress { get; set; }


        [ForeignKey(nameof(PatientGuardian))]
        public int? PatientGuardianId { get; set; }
        public virtual PatientGuardian? PatientGuardian { get; set; }


    }
}
