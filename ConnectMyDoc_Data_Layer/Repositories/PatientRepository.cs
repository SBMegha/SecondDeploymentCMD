
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectMyDoc_Data_Layer.Context;
using ConnectMyDoc_Domain_Layer.Entity;
using ConnectMyDoc_Domain_Layer.Exceptions;
using ConnectMyDoc_Domain_Layer.Repository;
using ConnectMyDoc_Domain_Layer.Services;
using Microsoft.EntityFrameworkCore;
namespace ConnectMyDoc_Data_Layer.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly PatientCMDDbContext _dbContext = null;
        private readonly IPatientAddressRepository _patientAddressRepository = null;
        private readonly IPatientGuardianRepository _patientGuardianRepository = null;
        private readonly IMessageService _exceptionMessageService = null;
        public PatientRepository(PatientCMDDbContext db, IPatientAddressRepository patientAddressRepository, IPatientGuardianRepository patientGuardianRepository, IMessageService messageService)
        {
            _dbContext = db;
            _patientAddressRepository = patientAddressRepository;
            _patientGuardianRepository = patientGuardianRepository;
            _exceptionMessageService = messageService;

        }
        public async Task<Patient> AddPatientAsync(Patient patient)
        {

            int patientAge = CalculateAge(patient.Dob);
            if (patientAge < 18)
            {
                if (patient.PatientGuardianId == null || patient.PatientGuardian == null)
                {
                    throw new Exception("Patient is under 18, guardian details must be provided.");
                }
            }
            await _dbContext.Patients.AddAsync(patient);
            await _dbContext.SaveChangesAsync();
            return patient;


        }
        public async Task<bool> DeletePatientAsync(int patientId)
        {
            try
            {
                Patient patient = await _dbContext.Patients.FindAsync(patientId);
                if (patient == null)
                    return false;
                _dbContext.Patients.Remove(patient);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                //log
                throw;
            }
        }
        private int CalculateAge(DateTime dob)
        {
            // Get today's date
            DateTime today = DateTime.Today;
            // Calculate the initial age based on year difference
            int age = today.Year - dob.Year;
            // Adjust age if the birthday has not occurred yet this year
            if (today < dob.AddYears(age))
            {
                age--;
            }
            return age;
        }




        public async Task<List<Patient>> GetAllPatientsAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.Patients
                .Include(p => p.PatientGuardian)
                .Include(p => p.PatientAddress)
                .Skip(pageNumber).Take(pageSize).ToListAsync();
        }
        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
           var patient = await _dbContext.Patients
                    .Include(p => p.PatientAddress)
                    .Include(p => p.PatientGuardian)
                    .SingleOrDefaultAsync(p => p.PatientId == patientId);
            return patient;
            
        }
        public async Task<int> GetTotalPatientsCountAsync()
        {
            return await _dbContext.Patients.CountAsync();
        }
        public async Task<bool> SetPrimaryClinicAsync(Patient patient, int clinic)
        {
            try
            {
                patient.PreferredClinicId = clinic;
                return true;
            }
            catch
            {
                //log
                return false;
            }
        }

        public async Task<bool> SetPrimaryDoctorAsync(Patient patient, int doctorId)
        {
            try
            {
                patient.PreferredDoctorId = doctorId;
                return true;
            }
            catch
            {
                //log
                return false;
            }
        }



        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
                var existingPatient = await GetPatientByIdAsync(patient.PatientId);
                if (existingPatient != null)
                {
                    _dbContext.Entry(existingPatient).CurrentValues.SetValues(patient);
                    await _dbContext.SaveChangesAsync();
                    return existingPatient;

                }
                else
                {
                    throw new BusinessException(_exceptionMessageService.GetMessage("PatientNotFound"));
                }
            
        }

        /*
        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
            try
            {
                var existingPatient = await GetPatientByIdAsync(patient.PatientId);
                if (existingPatient != null)
                {
                    if (existingPatient.PatientAddressId == patient.PatientAddressId &&
                        existingPatient.PatientAddress.PatientAddressId == patient.PatientAddress.PatientAddressId)
                    {
                        PatientAddress patientAddress = new PatientAddress
                        {
                            PatientAddressId = existingPatient.PatientAddressId,
                            City = patient.PatientAddress.City,
                            StreetAddress = patient.PatientAddress.StreetAddress,
                            Country = patient.PatientAddress.Country,
                            ZipCode = patient.PatientAddress.ZipCode,
                            State = patient.PatientAddress.State,
                            CreatedDate = patient.PatientAddress.CreatedDate,
                            LastModifiedDate = DateTime.Now,
                            CreatedBy = patient.PatientAddress.CreatedBy,
                            LastModifiedBy = patient.PatientAddress.CreatedBy,
                        };
                        if (_patientAddressRepository.UpdatePatientAddressAsync(patientAddress) == null)
                            throw new Exception("There was an error updating patient address");
                    }
                }
                else
                {
                    throw new Exception("Patient with id :" + patient.PatientId + " do not exsist");
                }
                if (patient.Age < 18)
                {
                    PatientGuardian patientGuardian = new PatientGuardian
                    {
                        PatientGuardianId = patient.PatientGuardianId.Value,
                        PatientGuardianName = patient.PatientGuardian.PatientGuardianName,
                        PatientGuardianPhoneNumber = patient.PatientGuardian.PatientGuardianPhoneNumber,
                        PatientGuardianRelationship = patient.PatientGuardian.PatientGuardianRelationship,
                    };
                    if (_patientGuardianRepository.UpdatePatientGuardianAsync(patientGuardian) == null)
                        throw new Exception("There was an error updating patient guardian");
                }

                _dbContext.Entry(existingPatient).CurrentValues.SetValues(patient);
                await _dbContext.SaveChangesAsync();
                return existingPatient;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
*/

    }
}
