using ConnectMyDoc_API_Layer.Models;
using Microsoft.AspNetCore.Mvc;
using ConnectMyDoc_Domain_Layer.Entity;
using ConnectMyDoc_Domain_Layer.DTOs;
using ConnectMyDoc_Domain_Layer.Manager;
using ConnectMyDoc_Domain_Layer.Services;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConnectMyDoc_API_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {


        private readonly IPatientManager _patientManager = null;
        private readonly IMessageService _messageService = null;

        public PatientsController(IPatientManager patientManager, IMessageService messageService)
        {
            _patientManager = patientManager;
            _messageService = messageService;
        }

        [HttpGet]
        [Route("{id:int}")]
        //[Authorize(Roles = "Admin,Doctor,User")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PatientDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientDTO>> GetPatientById(int id)
        {
            try
            {
                var result = await _patientManager.GetPatientByIdAsync(id);
                if (result == null)
                {
                    return NotFound("Patient with id :" + id + " not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        //[Authorize(Roles = "Admin")] 
        public async Task<ActionResult<PaginatedResult<PatientDTO>>> GetAllPatients([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var (patients, totalCountOfPatients) = await _patientManager.GetAllPatientsAsync(pageNumber, pageSize);
                var paginatedResult = new PaginatedResult<PatientDTO>
                {
                    Items = patients,
                    TotalCountOfPatients = totalCountOfPatients,
                    PageNumber = pageNumber,
                    PageSize = pageSize

                };
                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }



        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PatientDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPatient([FromForm] PatientDTO patientDTO)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //var result = await _patientService.AddPatientAsync(patientDTO);
                var result = await _patientManager.AddPatientAsync(patientDTO);
                if (result == null)
                {
                    await _patientManager.DeleteAddressAsync(patientDTO.PatientAddressId.Value);
                    if (patientDTO.Age < 18)
                        await _patientManager.DeleteGuardianAsync(patientDTO.PatientGuardianId.Value);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

/*

        [HttpDelete]
        [Route("{patientId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePatientById(int patientId)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Invalid patient ID.");
                }
                if (await _patientManager.DeletePatientByIdAsync(patientId))
                {
                    return Ok("Succesfully deleted");
                }
                else
                {
                    return NotFound("Patient with id :" + patientId + " not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal server error occurred.");
            }
        }

*/

        [HttpPut]
        [Route("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PatientDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdatePatientAsync([FromRoute] int id, [FromForm] PatientDTO patientDTO)
        {
            try
            {
                if (patientDTO == null)
                {
                    return BadRequest("Patient data is null.");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var updatedPatient = await _patientManager.UpdatePatientAsync(patientDTO, id);
                if (updatedPatient != null)
                {
                    return Ok(updatedPatient);
                }
                return NotFound("Patient was not found");
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}

