
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ConnectMyDoc_Domain_Layer.DTOs;
using ConnectMyDoc_Domain_Layer.Entity;
using ConnectMyDoc_Domain_Layer.Exceptions;
using ConnectMyDoc_Domain_Layer.Services;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Internal;



namespace ConnectMyDoc_Domain_Layer.Manager
{
    public class Helper
    {
        private readonly IMessageService _exceptionMessageService = null;
        private readonly HttpClient _httpClient = null;
        //private IPatientManager @object;

        public Helper(IMessageService _exceptionMessageService, HttpClient httpClient)
        {
            this._exceptionMessageService = _exceptionMessageService;
            this._httpClient = httpClient;
        }

        /*public Helper(IPatientManager @object)
		{
			this.@object = @object;
		}
        */


        /// <summary>
        /// Maps a Patient entity to a PatientDTO.
        /// </summary>
        /// <param name="patient">The Patient entity to be mapped.</param>
        /// <returns>Returns the mapped PatientDTO.</returns>
        /// <exception cref="BusinessException">Thrown when the patient is null.</exception>
        public PatientDTO MapPatientToPatientDTO(Patient patient)
        {
            if (patient == null)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("NullPatient"));
            }

            PatientDTO patientDTO = new PatientDTO
            {
                PatientId = patient.PatientId,
                PatientName = patient.PatientName,
                Email = patient.Email,
                Phone = patient.Phone,
                Age = patient.Age,
                Dob = patient.Dob,
                Gender = patient.Gender,
                PreferredStartTime = patient.PreferredStartTime,
                PreferredEndTime = patient.PreferredEndTime,
                CreatedDate = patient.CreatedDate,
                CreatedBy = patient.CreatedBy,
                LastModifiedDate = patient.LastModifiedDate,
                LastModifiedBy = patient.LastModifiedBy,
                PreferredClinicId = patient.PreferredClinicId,
                //Image = patient.Image != null ? Convert.ToBase64String(patient.Image) : null,
                Image = null,
                HexImage = patient.Image != null ? Convert.ToBase64String(patient.Image) : null, // Convert only if not null
                PatientAddressId = patient.PatientAddressId,
                StreetAddress = patient.PatientAddress?.StreetAddress ?? string.Empty,
                City = patient.PatientAddress?.City ?? string.Empty,
                State = patient.PatientAddress?.State ?? string.Empty,
                Country = patient.PatientAddress?.Country ?? string.Empty,
                ZipCode = patient.PatientAddress?.ZipCode ?? string.Empty,
                PreferredDoctorId = patient.PreferredDoctorId
            };
            if (patient.Age < 18 || patient.PatientGuardianId != null)
            {
                patientDTO.PatientGuardianId = patient.PatientGuardianId;
                patientDTO.PatientGuardianName = patient.PatientGuardian.PatientGuardianName;
                patientDTO.PatientGuardianPhoneNumber = patient.PatientGuardian.PatientGuardianPhoneNumber;
                patientDTO.PatientGuardianRelationship = patient.PatientGuardian.PatientGuardianRelationship;

            }
            return patientDTO;
        }

        /*public IFormFile ConvertByteArrayToFormFile(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            var stream = new MemoryStream(imageBytes);
            var formFile = new FormFile(stream, 0, imageBytes.Length, "file", "defaultfile")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream" // Default content type
            };
            stream.Position = 0;
            return formFile;
        }*/


        /// <summary>
        /// Maps a PatientDTO to a Patient entity.
        /// </summary>
        /// <param name="patientDTO">The PatientDTO to be mapped.</param>
        /// <param name="patientAddress">The patient's address entity.</param>
        /// <param name="patientGuardian">The patient's guardian entity.</param>
        /// <returns>Returns the mapped Patient entity.</returns>
        public Patient MapPatientDtoToPatient(PatientDTO patientDTO, PatientAddress patientAddress, PatientGuardian patientGuardian)
        {
            byte[] imageBytes = null;
            // Validate profile picture
            if (patientDTO.Image != null)
            {

                if (IsImageValid(patientDTO.Image))
                {
                    // Convert profile picture to byte array
                    if (patientDTO.Image != null && patientDTO.Image.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            patientDTO.Image.CopyToAsync(memoryStream);
                            imageBytes = memoryStream.ToArray();
                        }
                    }
                }
            }
            /*// Call the ValidateTimeRange method with PatientDTO
			if (ValidateTimeRange(patientDTO, new ValidationContext(patientDTO)) != ValidationResult.Success)
			{
				throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPreferredTimeRange"));
			}
`           */
            Patient patient = new Patient
            {
                PatientName = patientDTO.PatientName,
                Email = patientDTO.Email,
                Phone = patientDTO.Phone,
                Age = patientDTO.Age.Value,
                Dob = patientDTO.Dob,
                Gender = patientDTO.Gender,
                PreferredStartTime = patientDTO.PreferredStartTime,
                PreferredEndTime = patientDTO.PreferredEndTime,
                CreatedDate = patientDTO.CreatedDate,
                CreatedBy = patientDTO.CreatedBy,
                LastModifiedDate = patientDTO.LastModifiedDate,
                LastModifiedBy = patientDTO.LastModifiedBy,
                PreferredClinicId = patientDTO.PreferredClinicId,
                // Image = !string.IsNullOrEmpty(patientDTO.Image) ? Convert.FromBase64String(patientDTO.Image) : null,
                Image = imageBytes,
                PatientAddressId = patientAddress.PatientAddressId,
                PatientAddress = patientAddress,
                PreferredDoctorId = patientDTO.PreferredDoctorId
            };

            // If patient is below 18, assign the guardian details
            if (patient.Age < 18)
            {
                patient.PatientGuardianId = patientGuardian.PatientGuardianId;
                patient.PatientGuardian = patientGuardian;
            }
            return patient;
        }


        /// <summary>
        /// Calculates the patient's age based on the date of birth.
        /// </summary>
        /// <param name="dob">Date of birth of the patient.</param>
        /// <returns>Returns the calculated age as an integer.</returns>
        public int CalculateAge(DateTime dob)
        {
            // Get today's date
            DateTime today = DateTime.Now;

            // Calculate the initial age based on year difference
            int age = today.Year - dob.Year;

            // Adjust age if the birthday has not occurred yet this year
            if (today < dob.AddYears(age))
            {
                age--;
            }

            return age;
        }


        /// <summary>
        /// Validates if the patient guardian's information is present in the DTO.
        /// </summary>
        /// <param name="patientDTO">The PatientDTO containing guardian details.</param>
        /// <exception cref="BusinessException">Thrown when guardian details are missing.</exception>
        public void ValidatePatientGuardian(PatientDTO patientDTO)

        {
            if (string.IsNullOrWhiteSpace(patientDTO.PatientGuardianName))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("MissingGuardianName"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.PatientGuardianPhoneNumber))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("MissingGuardianPhone"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.PatientGuardianRelationship))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("MissingGuardianRelationship"));
            }

        }


        /// <summary>
        /// Validates the required fields of a PatientDTO.
        /// </summary>
        /// <param name="patientDTO">The PatientDTO to be validated.</param>
        /// <exception cref="BusinessException">Thrown when any required fields are invalid.</exception>
        public void ValidateRequiredFieldsForPatientAsync(PatientDTO patientDTO)
        {
            if (patientDTO == null)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("NullPatientDTO"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.PatientName))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPatientName"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.Email))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidEmail"));
            }
            if (!IsValidEmail(patientDTO.Email))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidEmailFormat"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.Phone))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPhone"));
            }
            if (patientDTO.Dob == default(DateTime))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("MissingDobOrAge"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.Gender))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidGender"));
            }
            if (patientDTO.PreferredStartTime == default(DateTime))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPreferredStartTime"));
            }
            if (patientDTO.PreferredEndTime == default(DateTime))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPreferredEndTime"));
            }
            var startTime = TimeOnly.FromDateTime(patientDTO.PreferredStartTime);
            var endTime = TimeOnly.FromDateTime(patientDTO.PreferredEndTime);

            if (startTime >= endTime)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPreferredTimeRange"));
            }
            if (patientDTO.CreatedBy <= 0)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidCreatedBy"));
            }
            if (patientDTO.LastModifiedBy <= 0)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidLastModifiedBy"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.StreetAddress))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidStreetAddress"));
            }
            if (patientDTO.PreferredClinicId <= 0)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidClinicId"));
            }
            if (patientDTO.PreferredDoctorId <= 0)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidDoctorId"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.State))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidState"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.City))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidCity"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.Country))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidCountry"));
            }
            if (string.IsNullOrWhiteSpace(patientDTO.ZipCode))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidZipCode"));
            }

        }

        /// <summary>
        /// Validates the format of the uploaded image file.
        /// </summary>
        /// <param name="file">The uploaded image file to be validated.</param>
        /// <param name="context">Validation context for the file.</param>
        /// <returns>Returns a ValidationResult indicating the validation outcome.</returns>
        public static ValidationResult ValidateImageFile(IFormFile file, ValidationContext context)
        {
            if (file == null || file.Length == 0)
            {
                return new ValidationResult("Image file is required.");
            }

            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!validExtensions.Contains(extension))
            {
                return new ValidationResult("Invalid image format");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates if the uploaded image meets size and extension criteria.
        /// </summary>
        /// <param name="Image">The uploaded image file to be validated.</param>
        /// <returns>Returns true if the image is valid, otherwise throws a BusinessException.</returns>
        /// <exception cref="BusinessException">Thrown if the image size exceeds the limit or if the extension is invalid.</exception>
        public bool IsImageValid(IFormFile Image)
        {
            // extension type validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(Image.FileName).ToLowerInvariant();
            // Size validation
            const long maxFileSize = 5 * 1024 * 1024;  // 5 MB in bytes
                                                       //const long maxFileSize = 20 * 1024;  // 5 MB in bytes
            if (!allowedExtensions.Contains(extension))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidImageType"));
            }
            if (Image.Length > maxFileSize)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("ImageSizeExceeded"));
            }

            return true;
        }
        // Custom validation for time range
        
        
        public static ValidationResult ValidateTimeRange(PatientDTO patientDTO, ValidationContext context)
        {
            if (patientDTO.PreferredStartTime >= patientDTO.PreferredEndTime)
            {
                return new ValidationResult("Preferred start time must be earlier than end time.");
            }
            return ValidationResult.Success;
        }




        /// <summary>
        /// Creates a new PatientAddress entity with the provided details.
        /// </summary>
        /// <param name="streetAddress">The street address of the patient.</param>
        /// <param name="city">The city of the patient.</param>
        /// <param name="state">The state of the patient.</param>
        /// <param name="country">The country of the patient.</param>
        /// <param name="zipcode">The zip code of the patient.</param>
        /// <param name="createdDate">The date the address was created.</param>
        /// <param name="createdBy">The user who created the address.</param>
        /// <param name="lastModifiedDate">The date the address was last modified.</param>
        /// <param name="lastModifiedBy">The user who last modified the address.</param>
        /// <returns>Returns a new PatientAddress entity.</returns>
        public PatientAddress CreatePatientAddress(string streetAddress, string city, string state, string country, string zipcode, DateTime createdDate, int createdBy, DateTime lastModifiedDate, int lastModifiedBy)
        {
            PatientAddress patientAddress = new PatientAddress
            {
                StreetAddress = streetAddress,
                City = city,
                State = state,
                Country = country,
                ZipCode = zipcode,
                CreatedDate = createdDate,
                CreatedBy = createdBy,
                LastModifiedDate = lastModifiedDate,
                LastModifiedBy = lastModifiedBy
            };
            return patientAddress;
        }

        /// <summary>
        /// Creates a new PatientGuardian entity with the provided details.
        /// </summary>
        /// <param name="guradianName">The guardian's name.</param>
        /// <param name="guardianPhone">The guardian's phone number.</param>
        /// <param name="relationship">The guardian's relationship to the patient.</param>
        /// <returns>Returns a new PatientGuardian entity.</returns>
        public PatientGuardian CreatePatientGuardian(string guradianName, string guardianPhone, string relationship)
        {
            PatientGuardian patientGuardian = new PatientGuardian
            {
                PatientGuardianName = guradianName,
                PatientGuardianPhoneNumber = guardianPhone,
                PatientGuardianRelationship = relationship
            };
            return patientGuardian;
        }

        /// <summary>
        /// Validates the format of the email address.
        /// </summary>
        /// <param name="email">The email address to be validated.</param>
        /// <returns>Returns true if the email is valid; otherwise, false.</returns>
        public bool IsValidEmail(string email)
        {
            // A more robust email regex pattern
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return emailRegex.IsMatch(email);
        }

        /// <summary>
        /// Validates the doctor ID by calling an external API.
        /// </summary>
        /// <param name="doctorId">The doctor ID to be validated.</param>
        /// <returns>Returns true if the doctor ID is valid; otherwise, false.</returns>
        /// <exception cref="BusinessException">Thrown when an unexpected response is received from the API.</exception>
        public async Task<bool> ValidateDoctorIdAsync(int doctorId)
        {
            var response = await _httpClient.GetAsync($"https://cmd-doctor-api.azurewebsites.net/api/Doctor/{doctorId}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false; // Doctor ID is invalid
            }

            // If another status code is received, throw an exception or handle accordingly
            throw new BusinessException(_exceptionMessageService.GetMessage("UnexpectedApiResponse") + ": " + response.StatusCode);
        }

    }

}

