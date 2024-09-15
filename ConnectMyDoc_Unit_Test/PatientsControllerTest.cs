
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConnectMyDoc_API_Layer.Controllers;
using ConnectMyDoc_Domain_Layer.DTOs;
using ConnectMyDoc_Domain_Layer.Manager;
using ConnectMyDoc_Domain_Layer.Services;
using ConnectMyDoc_API_Layer.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

[TestClass]
public class PatientsControllerTests
{

    private Mock<IPatientManager> _mockPatientManager;
    private Mock<IMessageService> _mockMessageService;
    private PatientsController _controller;
    private Mock<Helper> _helper;

    private readonly IMessageService _exceptionMessageService = null;

    [TestInitialize]
    public void Setup()
    {
        _mockPatientManager = new Mock<IPatientManager>();
        _mockMessageService = new Mock<IMessageService>();
        _controller = new PatientsController(_mockPatientManager.Object, _mockMessageService.Object);
        //_helper = new Mock<Helper>(MockBehavior.Strict, _mockMessageService.Object, new HttpClient());
        _helper = new Mock<Helper>(_mockMessageService.Object, new HttpClient());

        /*_helper.Setup(h => h.CalculateAge(It.IsAny<DateTime>())).Returns<DateTime>(dob =>
		{
			var today = DateTime.Today;
			var age = today.Year - dob.Year;
			if (today < dob.AddYears(age))
			{
				age--;
			}
			return age;
		});*/
    }

    [TestMethod]

    public async Task GetPatientById_ReturnsOkResult_WithPatientDTO()
    {
        // Arrange
        var patientId = 1;
        var patientDto = new PatientDTO { PatientId = patientId, PatientName = "John Doe" };

        _mockPatientManager.Setup(m => m.GetPatientByIdAsync(patientId))
            .ReturnsAsync(patientDto);

        // Act
        var result = await _controller.GetPatientById(patientId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Result should be OkObjectResult.");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode, "Status code should be 200.");

        var returnedPatientDto = okResult.Value as PatientDTO;
        Assert.IsNotNull(returnedPatientDto, "Returned value should be of type PatientDTO.");
        Assert.AreEqual(patientDto.PatientId, returnedPatientDto.PatientId, "Patient ID should match.");
        Assert.AreEqual(patientDto.PatientName, returnedPatientDto.PatientName, "Patient Name should match.");
    }

    [TestMethod]
    public async Task GetPatientById_ReturnsNotFound_WhenPatientNotFound()
    {
        // Arrange
        var patientId = 5;
        _mockPatientManager.Setup(m => m.GetPatientByIdAsync(patientId))
            .ReturnsAsync((PatientDTO)null);

        // Act
        var result = await _controller.GetPatientById(patientId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult, "Result should be NotFoundObjectResult.");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.AreEqual($"Patient with id :{patientId} not found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task GetAllPatients_ReturnsPaginatedResult_WithPatientDTOs()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 20;
        var patients = new List<PatientDTO>
        {
            new PatientDTO { PatientId = 1, PatientName = "John Doe" },
            new PatientDTO { PatientId = 2, PatientName = "Jane Smith" }
        };
        var totalCount = patients.Count;
        _mockPatientManager.Setup(m => m.GetAllPatientsAsync(pageNumber, pageSize))
            .ReturnsAsync((patients, totalCount));

        // Act
        var result = await _controller.GetAllPatients(pageNumber, pageSize);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        var paginatedResult = okResult.Value as PaginatedResult<PatientDTO>;
        Assert.IsNotNull(paginatedResult);
        Assert.AreEqual(patients.Count, paginatedResult.Items.Count());
        Assert.AreEqual(totalCount, paginatedResult.TotalCountOfPatients);
        Assert.AreEqual(pageNumber, paginatedResult.PageNumber);
        Assert.AreEqual(pageSize, paginatedResult.PageSize);
    }

    [TestMethod]
    public async Task AddPatient_ReturnsOkResult_WithPatientDTO()
    {
        // Arrange
        var patientDto = new PatientDTO { PatientId = 1, PatientName = "John Doe" };
        //var profilePicture = new Mock<IFormFile>();
        //profilePicture.Setup(p => p.Length).Returns(0);
        //byte[] imageBytes = null;

        _mockPatientManager.Setup(m => m.AddPatientAsync(patientDto))
            .ReturnsAsync(patientDto);

        // Act
        var result = await _controller.AddPatient(patientDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(patientDto, okResult.Value);
    }
    [TestMethod]
    public async Task AddPatient_InvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var patientDto = new PatientDTO
        {
            PatientName = "John Doe",
            Email = "john.doe@example.com", // Assuming Email is required
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2)
        };
        _controller.ModelState.AddModelError("PatientId", "Required");

        // Act
        var result = await _controller.AddPatient(patientDto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.IsTrue(badRequestResult.Value is SerializableError);
    }

    [TestMethod]
    public async Task AddPatient_ExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var patientDto = new PatientDTO
        {
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2)
        };

        _mockPatientManager.Setup(m => m.AddPatientAsync(patientDto))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        var result = await _controller.AddPatient(patientDto);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.AreEqual("Test Exception", objectResult.Value);
    }

    [TestMethod]
    public async Task AddPatient_ValidPatientDTO_WithProfilePicture_ShouldReturnOk()
    {
        // Arrange
        var patientDto = new PatientDTO
        {
            PatientId = 1,
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2),
            Image = new Mock<IFormFile>().Object // Simulating an uploaded image file
        };

        _mockPatientManager.Setup(m => m.AddPatientAsync(patientDto))
            .ReturnsAsync(patientDto);

        // Act
        var result = await _controller.AddPatient(patientDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(patientDto, okResult.Value);
    }



    [TestMethod]
    public async Task UpdatePatient_ReturnsOkResult_WithUpdatedPatientDTO()
    {
        // Arrange
        var patientId = 1;
        var patientDto = new PatientDTO { PatientId = patientId, PatientName = "John Doe" };
        //var profilePicture = new Mock<IFormFile>();
        //profilePicture.Setup(p => p.Length).Returns(0);
        //byte[] imageBytes = null;

        //_mockPatientManager.Setup(m => m.UpdatePatientAsync(patientDto, patientId, imageBytes))
        _mockPatientManager.Setup(m => m.UpdatePatientAsync(patientDto, patientId))
            .ReturnsAsync(patientDto);

        // Act
        var result = await _controller.UpdatePatientAsync(patientId, patientDto);
        //var result = await _controller.UpdatePatient(patientId, patientDto, profilePicture.Object);


        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(patientDto, okResult.Value);
    }

    [TestMethod]
    public async Task UpdatePatient_ReturnsNotFound_WhenPatientNotFound()
    {
        // Arrange
        var patientId = 1;
        var patientDto = new PatientDTO { PatientId = patientId, PatientName = "John Doe" };
        //var profilePicture = new Mock<IFormFile>();
        //profilePicture.Setup(p => p.Length).Returns(0);
        //byte[] imageBytes = null;

        _mockPatientManager.Setup(m => m.UpdatePatientAsync(patientDto, patientId))
            //_mockPatientManager.Setup(m => m.UpdatePatientAsync(patientDto, patientId, imageBytes))
            .ReturnsAsync((PatientDTO)null);

        // Act
        //var result = await _controller.UpdatePatientAsync(patientId, patientDto, profilePicture.Object);
        var result = await _controller.UpdatePatientAsync(patientId, patientDto);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.AreEqual("Patient was not found", notFoundResult.Value);
    }

    [TestMethod]
    public async Task AddPatient_ValidPatientDTO_ShouldReturnOk()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            Phone = "1234567890",
            Dob = new DateTime(1994, 9, 1),
            Age = 30,
            // Initialize other properties as needed
        };

        _mockPatientManager.Setup(m => m.AddPatientAsync(patientDTO))
            .ReturnsAsync(patientDTO);

        // Act
        var result = await _controller.AddPatient(patientDTO) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public async Task AddPatient_InvalidPhoneNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            Phone = "12345", // Invalid phone number
            Dob = new DateTime(1994, 9, 1)
            // Initialize other properties as needed
        };

        _controller.ModelState.AddModelError("Phone", "Phone number must be exactly 10 digits.");

        // Act
        var result = await _controller.AddPatient(patientDTO) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
    }


    [TestMethod]

    public void CalculateAge_ValidDateOfBirth_ShouldReturnCorrectAge()
    {
        // Arrange
        var dob = new DateTime(2001, 9, 17);
        var today = DateTime.Today;

        // Act
        var age = _helper.Object.CalculateAge(dob);

        // Calculate the expected age based on today's date
        var expectedAge = today.Year - dob.Year;
        if (today < dob.AddYears(expectedAge))
        {
            expectedAge--; // Subtract 1 if the birthday has not occurred this year yet
        }

        // Assert
        Assert.AreEqual(expectedAge, age);
    }


    [TestMethod]
    public void CalculateAge_MockedHelper_ShouldReturnExpectedAge()
    {
        // Arrange
        var dob = new DateTime(2001, 9, 17);

        // Act
        var age = _helper.Object.CalculateAge(dob);

        // Calculate the expected age based on today's date
        var today = DateTime.Today;
        var expectedAge = today.Year - dob.Year;
        if (today < dob.AddYears(expectedAge))
        {
            expectedAge--; // Subtract 1 if the birthday has not occurred this year yet
        }

        // Assert
        Assert.AreEqual(expectedAge, age);
    }

    public static List<ValidationResult> ValidateModel(PatientDTO patientDTO)
    {
        var context = new ValidationContext(patientDTO);
        var results = new List<ValidationResult>();

        // Validate all properties
        bool isValid = Validator.TryValidateObject(patientDTO, context, results, true);

        // Explicitly check for invalid default values
        if (patientDTO.PreferredStartTime == default(DateTime))
        {
            results.Add(new ValidationResult("Preferred start time is required."));
        }

        if (patientDTO.PreferredEndTime == default(DateTime))
        {
            results.Add(new ValidationResult("Preferred end time is required."));
        }

        return results;
    }

    [TestMethod]
    public void EmailAddress_InvalidFormat_ShouldFail()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            PatientName = "John Doe",
            Email = "john.doeexample.com", // Invalid email format
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            Image = null,
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2)
        };

        // Act
        var validationResults = ValidateModel(patientDTO);

        // Assert
        // Validate the presence of the expected error message for email format
        var emailValidationErrors = validationResults.Where(v => v.ErrorMessage == "The Email field is not a valid e-mail address.").ToList();
        Assert.AreEqual(1, emailValidationErrors.Count, "Expected exactly one validation error for Email.");
    }



    [TestMethod]
    public void PreferredStartTime_InvalidDate_ShouldFail()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            PreferredStartTime = default(DateTime), // Invalid value
            PreferredEndTime = DateTime.Now.AddHours(2),
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            Image = null
        };



        // Act
        var validationResults = ValidateModel(patientDTO);


        // Assert
        var preferredStartTimeValidationErrors = validationResults.Where(v => v.ErrorMessage == "Preferred start time is required.").ToList();
        Assert.AreEqual(1, preferredStartTimeValidationErrors.Count, "Expected exactly one validation error for Preferred start time.");
    }







    [TestMethod]
    public void PreferredEndTime_InvalidDate_ShouldFail()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            PreferredEndTime = default(DateTime), // Invalid value
            PreferredStartTime = DateTime.Now.AddHours(1),
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            Image = null
        };

        // Act
        var validationResults = ValidateModel(patientDTO);

        // Assert
        var preferredEndTimeValidationErrors = validationResults.Where(v => v.ErrorMessage == "Preferred end time is required.").ToList();
        Assert.AreEqual(1, preferredEndTimeValidationErrors.Count, "Expected exactly one validation error for Preferred end time.");
    }

    /*[TestMethod]
	public void Image_InvalidFile_ShouldFail()
	{
		// Arrange
		var fileMock = new Mock<IFormFile>();
		fileMock.Setup(f => f.FileName).Returns("invalid.txt");
		fileMock.Setup(f => f.Length).Returns(1024);

		var patientDTO = new PatientDTO
		{
			Image = fileMock.Object,
			PatientName = "John Doe",
			Email = "john.doe@example.com",
			Phone = "7019071381",
			Age = 30,
			Dob = new DateTime(1994, 9, 1),
			City = "Wadi",
			State = "Bihar",// Empty City (invalid)
			Country = "India",
			Gender = "male",
			StreetAddress = "123",
			PreferredStartTime = DateTime.Now.AddHours(1),
			PreferredEndTime = DateTime.Now.AddHours(2)// Invalid image file
		};
		var validationResults = ValidateModel(patientDTO);
		// Debugging: Print out all the validation errors
		foreach (var validationResult in validationResults)
		{
			Console.WriteLine(validationResult.ErrorMessage);
		}
		// Act


		// Assert
		Assert.AreEqual(1, validationResults.Count);
		Assert.AreEqual("Invalid image format", validationResults[0].ErrorMessage);
	}*/
    [TestMethod]
    public void Country_EmptyValue_ShouldFail()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "Bihar",// Empty City (invalid)
            Country = "",
            Gender = "male",
            StreetAddress = "123",
            Image = null,
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2)
        };

        // Act
        var validationResults = ValidateModel(patientDTO);

        // Check the actual validation errors
        foreach (var result in validationResults)
        {
            Console.WriteLine(result.ErrorMessage); // This will help identify the additional errors
        }

        // Assert
        var countryValidationErrors = validationResults.Where(v => v.ErrorMessage == "Country is required.").ToList();
        Assert.AreEqual(1, countryValidationErrors.Count, "Expected exactly one validation error for Country.");
    }
    [TestMethod]
    public void State_EmptyValue_ShouldFail()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "7019071381",
            Age = 30,
            Dob = new DateTime(1994, 9, 1),
            City = "Wadi",
            State = "",// Empty City (invalid)
            Country = "India",
            Gender = "male",
            StreetAddress = "123",
            Image = null,
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2)
        };

        // Act
        var validationResults = ValidateModel(patientDTO);

        // Check the actual validation errors
        foreach (var result in validationResults)
        {
            Console.WriteLine(result.ErrorMessage); // This will help identify the additional errors
        }

        // Assert
        var stateValidationErrors = validationResults.Where(v => v.ErrorMessage == "State is required.").ToList();
        Assert.AreEqual(1, stateValidationErrors.Count, "Expected exactly one validation error for State.");
    }
    [TestMethod]
    public void City_EmptyValue_ShouldFail()
    {
        // Arrange
        var patientDTO = new PatientDTO
        {
            City = string.Empty, // Empty city field
                                 // Fill other required fields with valid values
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            Gender = "Male",
            StreetAddress = "123 Main St",
            State = "State",
            Country = "Country",
            Image = null // or valid file if required
        };

        // Act
        var validationResults = ValidateModel(patientDTO);

        // Debugging: Print out all the validation errors
        foreach (var validationResult in validationResults)
        {
            Console.WriteLine(validationResult.ErrorMessage);
        }

        // Assert
        // Check if there is exactly one validation error for the City property
        var cityValidationErrors = validationResults.Where(v => v.ErrorMessage == "City is required.").ToList();
        Assert.AreEqual(1, cityValidationErrors.Count, "Expected exactly one validation error for City.");
    }


    [TestMethod]
    public async Task UpdatePatient_ReturnsOkResult_WhenUpdateIsSuccessful()
    {
        // Arrange
        int patientId = 1;
        var patientDto = new PatientDTO
        {
            PatientId = patientId,
            PatientName = "John Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Age = 30,
            Dob = DateTime.Now.AddYears(-30),
            Gender = "Male",
            PreferredStartTime = DateTime.Now.AddHours(1),
            PreferredEndTime = DateTime.Now.AddHours(2),
            CreatedBy = 1,
            LastModifiedBy = 1,
            StreetAddress = "123 Main St"
        };

        _mockPatientManager
            .Setup(service => service.UpdatePatientAsync(patientDto, patientId))
            .ReturnsAsync(patientDto);

        // Act
        var result = await _controller.UpdatePatientAsync(patientId, patientDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode, "Expected status code 200");

        var returnedPatient = okResult.Value as PatientDTO;
        Assert.IsNotNull(returnedPatient, "Expected PatientDTO");
        Assert.AreEqual(patientId, returnedPatient.PatientId, "Patient ID mismatch");
        Assert.AreEqual("John Doe", returnedPatient.PatientName, "Patient name mismatch");
    }

    [TestMethod]
    public async Task UpdatePatient_ReturnsBadRequest_WhenPatientDtoIsInvalid()
    {
        // Arrange
        int patientId = 1;
        PatientDTO patientDto = null; // Invalid DTO

        // Act
        var result = await _controller.UpdatePatientAsync(patientId, patientDto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(400, badRequestResult.StatusCode, "Expected status code 400");
        Assert.AreEqual("Patient data is null.", badRequestResult.Value, "Expected error message mismatch");
    }

    [TestMethod]
    public async Task UpdatePatient_ReturnsOkResult_WhenValidPatientDTO()
    {
        // Arrange
        int patientId = 1;
        var patientDTO = new PatientDTO
        {
            PatientId = patientId,
            PatientName = "John Doe",
            Dob = new DateTime(1998, 1, 1),
            PreferredDoctorId = 123,
            PatientGuardianId = null, // Assuming the patient is an adult
            PatientAddressId = 456,
            StreetAddress = "123 Main St",
            City = "New York",
            State = "NY",
            Country = "USA",
            ZipCode = "10001",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            Gender = "Male",
            PreferredClinicId = 789,
        };

        _mockPatientManager.Setup(service => service.UpdatePatientAsync(patientDTO, patientId))
            .ReturnsAsync(patientDTO);

        // Act
        var result = await _controller.UpdatePatientAsync(patientId, patientDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(patientDTO, okResult.Value);
        _mockPatientManager.Verify(service => service.UpdatePatientAsync(patientDTO, patientId), Times.Once);
    }


    [TestMethod]
    public async Task UpdatePatient_ReturnsOkResult_WhenPatientGuardianChangesFromMinorToAdult()
    {
        // Arrange
        int patientId = 1;
        var patientDTO = new PatientDTO
        {
            PatientId = patientId,
            PatientName = "John Doe",
            Dob = new DateTime(2005, 1, 1), // Patient was minor previously
            PreferredDoctorId = 123,
            PatientGuardianId = null, // Transitioning to adult, so no guardian
            PatientAddressId = 456,
            StreetAddress = "123 Main St",
            City = "New York",
            State = "NY",
            Country = "USA",
            ZipCode = "10001",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            Gender = "Male",
            PreferredClinicId = 789
        };

        _mockPatientManager.Setup(service => service.UpdatePatientAsync(patientDTO, patientId))
            .ReturnsAsync(patientDTO);

        // Act
        var result = await _controller.UpdatePatientAsync(patientId, patientDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(patientDTO, okResult.Value);
        _mockPatientManager.Verify(service => service.UpdatePatientAsync(patientDTO, patientId), Times.Once);
    }

    [TestMethod]
    public async Task UpdatePatient_ReturnsOkResult_WhenPatientGuardianChangesFromAdultToMinor()
    {
        // Arrange
        int patientId = 1;
        var patientDTO = new PatientDTO
        {
            PatientId = patientId,
            PatientName = "John Doe",
            Dob = new DateTime(2010, 1, 1), // Patient is now a minor
            PreferredDoctorId = 123,
            PatientGuardianId = 456, // Guardian is now required
            PatientAddressId = 456,
            StreetAddress = "123 Main St",
            City = "New York",
            State = "NY",
            Country = "USA",
            ZipCode = "10001",
            Email = "john.doe@example.com",
            Phone = "1234567890",
            Gender = "Male",
            PreferredClinicId = 789
        };

        _mockPatientManager.Setup(service => service.UpdatePatientAsync(patientDTO, patientId))
            .ReturnsAsync(patientDTO);

        // Act
        var result = await _controller.UpdatePatientAsync(patientId, patientDTO);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(patientDTO, okResult.Value);
        _mockPatientManager.Verify(service => service.UpdatePatientAsync(patientDTO, patientId), Times.Once);
    }



}

