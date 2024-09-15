using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectMyDoc_Domain_Layer.Manager;
using Microsoft.AspNetCore.Http;

namespace ConnectMyDoc_Domain_Layer.DTOs
{
    public class PatientDTO
    {
        public int? PatientId { get; set; }

        [Required]
        [MinLength(3)]
        public string PatientName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }

        [Required]
        [Length(10, 15)]
        public string Phone { get; set; }


        [Range(0, 150)]
        public int? Age { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Preferred start time is required.")]
        public DateTime PreferredStartTime { get; set; }
        [Required(ErrorMessage = "Preferred end time is required.")]
        public DateTime PreferredEndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int LastModifiedBy { get; set; }

        [Required]
        public int PreferredClinicId { get; set; }

        //[CustomValidation(typeof(Helper), "ValidateImageFile")]
        public IFormFile? Image { get; set; }

        public string? HexImage { get; set; }

        public int? PatientAddressId { get; set; }

        [Required]

        public string StreetAddress { get; set; } //Street address of Patient
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } //City in which Patient is residing
        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; } //State in which Patient is residing 
        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; } //Country in which Patient is residing
        public string ZipCode { get; set; } //Zipcode of area where Patient is residing


        [Required]

        public int PreferredDoctorId { get; set; }

        public int? PatientGuardianId { get; set; }
        public string? PatientGuardianName { get; set; }
        public string? PatientGuardianPhoneNumber { get; set; }
        public string? PatientGuardianRelationship { get; set; }

    }

}
