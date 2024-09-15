
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConnectMyDoc_Domain_Layer.DTOs;
using ConnectMyDoc_Domain_Layer.Entity;
using ConnectMyDoc_Domain_Layer.Exceptions;
using ConnectMyDoc_Domain_Layer.Repository;
using ConnectMyDoc_Domain_Layer.Services;
using static System.Net.Mime.MediaTypeNames;

namespace ConnectMyDoc_Domain_Layer.Manager
{
    public class PatientManager : IPatientManager
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientAddressRepository _patientAddressRepository;
        private readonly IPatientGuardianRepository _patientGuardianRepository;
        private readonly IMessageService _exceptionMessageService;
        private readonly HttpClient _httpClient;
        private readonly Helper _helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientManager"/> class with repository dependencies, helper, and message service.
        /// </summary>
        /// <param name="patientRepository">The repository for managing patient data.</param>
        /// <param name="patientAddressRepository">The repository for managing patient address data.</param>
        /// <param name="patientGuardianRepository">The repository for managing patient guardian data.</param>
        /// <param name="helper">Helper class for validation and utility methods.</param>
        /// <param name="messageService">Service for fetching exception messages.</param>
        public PatientManager(IPatientRepository patientRepository, IPatientAddressRepository patientAddressRepository, IPatientGuardianRepository patientGuardianRepository, Helper helper, IMessageService messageService)
        {
            _patientRepository = patientRepository;
            _patientAddressRepository = patientAddressRepository;
            _patientGuardianRepository = patientGuardianRepository;
            _exceptionMessageService = messageService;
            _helper = helper;
        }

        /// <summary>
        /// Adds a new patient to the system after validating inputs.
        /// </summary>
        /// <param name="patientDTO">The patient data to be added.</param>
        /// <returns>The added patient data transfer object.</returns>
        /// <exception cref="BusinessException">Thrown when business rules are violated (e.g., invalid phone number or doctor ID).</exception>
        /// <exception cref="Exception">Thrown when the age and DOB fields do not match or phone number length is invalid.</exception>
        public async Task<PatientDTO> AddPatientAsync(PatientDTO patientDTO)
        {
            // Validate phone number
            if (!ValidatePhoneNumber(patientDTO.Phone))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPhoneNumberFormat"));
            }

            PatientAddress patientAddress = null;
            PatientGuardian patientGuardian = null;
            Patient patient = null;

            _helper.ValidateRequiredFieldsForPatientAsync(patientDTO);

            var isDoctorValid = await _helper.ValidateDoctorIdAsync(patientDTO.PreferredDoctorId);
            if (!isDoctorValid)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("DoctorNotFound") + " : " + patientDTO.PreferredDoctorId);
            }

            int age = _helper.CalculateAge(patientDTO.Dob);
            if (patientDTO.Age.HasValue)
            {
                if (patientDTO.Age != age)
                {
                    throw new Exception("Age and DOB input fields don't match");
                }
                else
                {
                    patientDTO.Age = age;
                    if (patientDTO.Age < 18)
                    {
                        _helper.ValidatePatientGuardian(patientDTO);
                        patientGuardian = _helper.CreatePatientGuardian(patientDTO.PatientGuardianName, patientDTO.PatientGuardianPhoneNumber, patientDTO.PatientGuardianRelationship);
                        patientGuardian = await _patientGuardianRepository.AddPatientGuardian(patientGuardian);
                    }
                }
            }

            // Ensure phone number length is valid
            if (patientDTO.Phone.Length >= 10)
            {

                patientAddress = _helper.CreatePatientAddress(
                                        patientDTO.StreetAddress,
                                        patientDTO.City,
                                        patientDTO.State,
                                        patientDTO.Country,
                                        patientDTO.ZipCode,
                                        patientDTO.CreatedDate,
                                        patientDTO.CreatedBy,
                                        patientDTO.LastModifiedDate,
                                        patientDTO.LastModifiedBy);

                patientAddress = await _patientAddressRepository.AddPatientAddressAsync(patientAddress);

                patient = _helper.MapPatientDtoToPatient(patientDTO, patientAddress, patientGuardian);
                patient = await _patientRepository.AddPatientAsync(patient);
                patientDTO = _helper.MapPatientToPatientDTO(patient);
                return patientDTO;
            }
            else
            {
                throw new Exception("Phone number length is less than 10 digits");
            }


        }

        /// <summary>
        /// Deletes a patient's address by its ID.
        /// </summary>
        /// <param name="addressId">The ID of the address to delete.</param>
        /// <returns>A boolean indicating if the deletion was successful.</returns>
        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            return await _patientAddressRepository.DeletePatientAddressAsync(addressId);
        }

        /// <summary>
        /// Deletes a patient's guardian by its ID.
        /// </summary>
        /// <param name="guardianId">The ID of the guardian to delete.</param>
        /// <returns>A boolean indicating if the deletion was successful.</returns>
        public async Task<bool> DeleteGuardianAsync(int guardianId)
        {
            return await _patientGuardianRepository.DeletePatientGuardianAsync(guardianId);
        }
        
        /// <summary>
        /// Deletes a patient by their ID.
        /// </summary>
        /// <param name="id">The ID of the patient to delete.</param>
        /// <returns>A boolean indicating if the deletion was successful.</returns>
        public async Task<bool> DeletePatientByIdAsync(int id)
        {
            return await _patientRepository.DeletePatientAsync(id);
        }

        /// <summary>
        /// Retrieves a paginated list of patients along with the total count of patients.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of patients to retrieve per page.</param>
        /// <returns>A tuple containing a collection of patient DTOs and the total count of patients.</returns>
        public async Task<(IEnumerable<PatientDTO> Patients, int TotalCount)> GetAllPatientsAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var patients = await _patientRepository.GetAllPatientsAsync(skip, pageSize);
            var totalCountOfPatients = await _patientRepository.GetTotalPatientsCountAsync();
            List<PatientDTO> patientDTOs = patients.Select(patient => _helper.MapPatientToPatientDTO(patient)).ToList();

            return (patientDTOs, totalCountOfPatients);
        }

        /// <summary>
        /// Retrieves a patient by their ID.
        /// </summary>
        /// <param name="id">The ID of the patient to retrieve.</param>
        /// <returns>The patient DTO if found, otherwise null.</returns>
        public async Task<PatientDTO> GetPatientByIdAsync(int id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return null;
            }
            return _helper.MapPatientToPatientDTO(patient);
        }

        /// <summary>
        /// Updates an existing patient record.
        /// </summary>
        /// <param name="patientDTO">The updated patient data.</param>
        /// <param name="patientId">The ID of the patient to update.</param>
        /// <returns>The updated patient data transfer object.</returns>
        /// <exception cref="BusinessException">Thrown when business rules are violated (e.g., invalid phone number, doctor ID, or missing guardian).</exception>
        /// <exception cref="Exception">Thrown when profile picture validation or update fails.</exception>
        public async Task<PatientDTO> UpdatePatientAsync(PatientDTO patientDTO, int patientId)
        {

            PatientGuardian newPatientGuardian = null;
            PatientGuardian existingGuardian = null;

            if (patientDTO == null)
                throw new BusinessException(_exceptionMessageService.GetMessage("NullPatientDTO"));

            // Validate phone number
            if (!ValidatePhoneNumber(patientDTO.Phone))
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("InvalidPhoneNumberFormat"));
            }

            _helper.ValidateRequiredFieldsForPatientAsync(patientDTO);

            Patient existingPatient = await _patientRepository.GetPatientByIdAsync(patientId);

            if (existingPatient == null)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("NoExistingPatient"));
            }
            if (patientDTO.PatientAddressId == null)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("NoExistingPatientAddress"));
            }
            
            bool wasMinor = false;
            bool nowMinor = false;
            if (existingPatient.Age < 18)
            {
                wasMinor = true;
            }
            int newAge = _helper.CalculateAge(patientDTO.Dob);
            if (newAge < 18)
            {
                nowMinor = true;
            }

            byte[] imageBytes = null;
            // Validate profile picture
            if (patientDTO.Image != null)
            {

                if (_helper.IsImageValid(patientDTO.Image))
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

            var isDoctorValid = await _helper.ValidateDoctorIdAsync(patientDTO.PreferredDoctorId);
            if (!isDoctorValid)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("DoctorNotFound") + " : " + patientDTO.PreferredDoctorId);
            }

            // Update non-key properties
            existingPatient.PatientName = patientDTO.PatientName;
            existingPatient.Email = patientDTO.Email;
            existingPatient.Phone = patientDTO.Phone;
            existingPatient.Age = _helper.CalculateAge(patientDTO.Dob);
            existingPatient.Dob = patientDTO.Dob;
            existingPatient.Gender = patientDTO.Gender;
            existingPatient.PreferredStartTime = patientDTO.PreferredStartTime;
            existingPatient.PreferredEndTime = patientDTO.PreferredEndTime;
            existingPatient.CreatedDate = patientDTO.CreatedDate;
            existingPatient.CreatedBy = patientDTO.CreatedBy;
            existingPatient.LastModifiedDate = patientDTO.LastModifiedDate;
            existingPatient.LastModifiedBy = patientDTO.LastModifiedBy;
            existingPatient.PreferredClinicId = patientDTO.PreferredClinicId;

            existingPatient.Image = imageBytes;
            existingPatient.PreferredDoctorId = patientDTO.PreferredDoctorId;

            // Handle the PatientAddress and PatientGuardian if necessary


            PatientAddress existingPatientAddress = await _patientAddressRepository.GetPatientAddressByIdAsync(patientDTO.PatientAddressId.Value);
            if (existingPatientAddress == null)
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("PatientAddressNotFound"));
            }
            if (patientDTO.PatientAddressId.Value == existingPatient.PatientAddressId)
            {
                existingPatientAddress.StreetAddress = patientDTO.StreetAddress;
                existingPatientAddress.City = patientDTO.City;
                existingPatientAddress.State = patientDTO.State;
                existingPatientAddress.Country = patientDTO.Country;
                existingPatientAddress.ZipCode = patientDTO.ZipCode;
                existingPatientAddress = await _patientAddressRepository.UpdatePatientAddressAsync(existingPatientAddress);
                existingPatient.PatientAddressId = existingPatientAddress.PatientAddressId;
            }
            else
            {
                throw new BusinessException(_exceptionMessageService.GetMessage("AddressIdDoNotMatch"));
            }
            // major . now minor . and check id he has guardian
            if (existingPatient.Age < 18)
            {
                if (wasMinor)
                {
                    if(patientDTO.PatientGuardianId == null)
                    {
                        throw new BusinessException(_exceptionMessageService.GetMessage("MissingGuardianId"));
                    }
                    if (patientDTO.PatientGuardianId != existingPatient.PatientGuardianId)
                    {
                        throw new BusinessException(_exceptionMessageService.GetMessage("GuardianWasNotFound"));
                    }
                    else if ( (wasMinor && nowMinor) || (!nowMinor && existingPatient.PatientGuardianId!=null)) // was minor and is minor
                    {
                        _helper.ValidatePatientGuardian(patientDTO);
                        existingGuardian = await _patientGuardianRepository.GetPatientGuardianByIdAsync(patientDTO.PatientGuardianId.Value);
                        existingGuardian.PatientGuardianName = patientDTO.PatientGuardianName;
                        existingGuardian.PatientGuardianPhoneNumber = patientDTO.PatientGuardianPhoneNumber;
                        existingGuardian.PatientGuardianRelationship = patientDTO.PatientGuardianRelationship;
                        existingGuardian = await _patientGuardianRepository.UpdatePatientGuardianAsync(existingGuardian);
                        existingPatient.PatientGuardianId = existingGuardian.PatientGuardianId;
                    }
                }
                else if ( existingPatient.PatientGuardianId == null) // was not previoulsy minor.. but now minor // maor .. minor 
                {

                    _helper.ValidatePatientGuardian(patientDTO);
                    newPatientGuardian = _helper.CreatePatientGuardian(patientDTO.PatientGuardianName, patientDTO.PatientGuardianPhoneNumber, patientDTO.PatientGuardianRelationship);
                    newPatientGuardian = await _patientGuardianRepository.AddPatientGuardian(newPatientGuardian);
                    existingPatient.PatientGuardianId = newPatientGuardian.PatientGuardianId;
                }
            }

            existingPatient = await _patientRepository.UpdatePatientAsync(existingPatient);

            return _helper.MapPatientToPatientDTO(existingPatient);
        }

        /// <summary>
        /// Validates if a phone number contains only digits.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>True if the phone number is valid, otherwise false.</returns>
        public bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false; // Phone number cannot be null or empty
            }

            // Regular expression to check for only digits
            var regex = new Regex(@"^\d+$");

            return regex.IsMatch(phoneNumber);
        }
    }
}
