using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectMyDoc_Domain_Layer.DTOs;

namespace ConnectMyDoc_Domain_Layer.Manager
{
    public interface IPatientManager
    {
        public Task<bool> DeletePatientByIdAsync(int id);
        Task<(IEnumerable<PatientDTO> Patients, int TotalCount)> GetAllPatientsAsync(int pageNumber, int pageSize);
        Task<PatientDTO> GetPatientByIdAsync(int id);
        Task<PatientDTO> AddPatientAsync(PatientDTO patientDTO);
        //Task<PatientDTO> AddPatientAsync(PatientDTO patientDTO, byte[] imageBytes);
        Task<PatientDTO> UpdatePatientAsync(PatientDTO patientDTO, int patientId);
        //Task<PatientDTO> UpdatePatientAsync(PatientDTO patientDTO, int patientId, byte[] imageBytes);

        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> DeleteGuardianAsync(int guardianId);
    }
}
