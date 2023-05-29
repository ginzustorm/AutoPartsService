using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AutoDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AutoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("addCar")]
        public async Task<IActionResult> AddCar(int userCommonId, [FromBody] CarDto carDto)
        {
            var userCommon = await _context.UserCommons
                .Include(uc => uc.Cars)
                .FirstOrDefaultAsync(uc => uc.Id == userCommonId);

            if (userCommon == null)
            {
                return NotFound("User not found.");
            }

            var newCar = new Car
            {
                Make = carDto.Make,
                Model = carDto.Model,
                Color = carDto.Color,
                StateNumber = carDto.StateNumber,
                VinNumber = carDto.VinNumber,
                BodyNumber = carDto.BodyNumber,
                UserCommonId = userCommonId
            };

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            return Ok(newCar);
        }

        [HttpPut("UserCommon/{id}")]
        public async Task<ActionResult<UserCommonDto>> EditUserCommon(int id, [FromBody] EditUserCommonDto editUserCommonDto)
        {
            var userCommon = await _context.UserCommons.Include(uc => uc.Address)
                                                       .Include(uc => uc.Cars) 
                                                       .FirstOrDefaultAsync(uc => uc.Id == id);

            if (userCommon == null)
            {
                return NotFound("User not found.");
            }

            userCommon.Name = editUserCommonDto.Name;
            userCommon.Email = editUserCommonDto.Email;
            userCommon.PhoneNumber = editUserCommonDto.PhoneNumber;
            userCommon.Password = editUserCommonDto.Password;

            userCommon.Address.Country = editUserCommonDto.Address.Country;
            userCommon.Address.Region = editUserCommonDto.Address.Region;
            userCommon.Address.City = editUserCommonDto.Address.City;
            userCommon.Address.Street = editUserCommonDto.Address.Street;

            _context.Entry(userCommon.Address).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound("A concurrency error occurred. The User was not found in the database when attempting to update.");
            }

            var updatedUserCommonDto = new UserCommonDto
            {
                Id = userCommon.Id,
                Name = userCommon.Name,
                Email = userCommon.Email,
                PhoneNumber = userCommon.PhoneNumber,
                RegistrationDate = userCommon.RegistrationDate,
                Password = userCommon.Password,
                Address = userCommon.Address,
                Cars = userCommon.Cars.ToList()
            };

            return Ok(updatedUserCommonDto);
        }




        [HttpPut("UserBusiness/{id}")]
        public async Task<ActionResult<UserBusinessDto>> EditUserBusiness(int id, EditUserBusinessDto editUserBusinessDto)
        {
            var userBusiness = await _context.UserBusinesses.Include(ub => ub.Services).FirstOrDefaultAsync(ub => ub.Id == id);

            if (userBusiness == null)
            {
                return NotFound("User not found.");
            }

            userBusiness.Email = editUserBusinessDto.Email;
            userBusiness.Phone = editUserBusinessDto.Phone;
            userBusiness.Password = editUserBusinessDto.Password;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound("A concurrency error occurred. The User was not found in the database when attempting to update.");
            }

            var updatedUserBusinessDto = new UserBusinessDto
            {
                Id = userBusiness.Id,
                Email = userBusiness.Email,
                Phone = userBusiness.Phone,
                RegistrationDate = userBusiness.RegistrationDate,
                Password = userBusiness.Password,
                Services = userBusiness.Services.Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price
                }).ToList()
            };

            return Ok(updatedUserBusinessDto);
        }

        [HttpDelete("UserCommon/{userId}/Car/{carId}")]
        public async Task<IActionResult> DeleteCar(int userId, int carId)
        {
            var userCommon = await _context.UserCommons.Include(uc => uc.Cars)
                                                       .FirstOrDefaultAsync(uc => uc.Id == userId);

            if (userCommon == null)
            {
                return NotFound("User not found.");
            }

            var car = userCommon.Cars.FirstOrDefault(c => c.Id == carId);

            if (car == null)
            {
                return NotFound("Car not found.");
            }

            userCommon.Cars.Remove(car);
            _context.Cars.Remove(car);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("UserBusiness/{userId}/Service")]
        public async Task<ActionResult<ServiceDto>> AddService(int userId, ServiceDto serviceDto)
        {
            var userBusiness = await _context.UserBusinesses.FindAsync(userId);

            if (userBusiness == null)
            {
                return NotFound("User not found.");
            }

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Price = serviceDto.Price,
                UserBusinessId = userId
            };

            _context.Services.Add(service);
            userBusiness.Services.Add(service);
            await _context.SaveChangesAsync();

            serviceDto.Id = service.Id;

            return CreatedAtAction(nameof(GetService), new { id = service.Id }, serviceDto);
        }


        [HttpDelete("Service/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("Service/{id}")]
        public async Task<ActionResult<ServiceDto>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            var serviceDto = new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price
            };

            return serviceDto;
        }

        [HttpGet("GetDocuments/{userId}")]
        public async Task<ActionResult<IEnumerable<DocumentUser>>> GetDocuments(int userId)
        {
            var documents = await _context.Documents
                                          .Where(d => d.UserCommonId == userId)
                                          .ToListAsync();

            if (!documents.Any())
            {
                return NotFound();
            }

            return documents;
        }

        [HttpPost("CheckFines")]
        public ActionResult CheckFines(DocumentCheck documentCheck)
        {
            // Need to realize a method to check for fines (Victor)

            return Ok();
        }

        [HttpPost("AddDocument/{userId}")]
        public async Task<ActionResult> AddDocument(int userId, DocumentUserCreateDto documentDto)
        {
            var user = await _context.UserCommons.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var document = new DocumentUser
            {
                DocumentType = documentDto.DocumentType,
                CertificateNumber = documentDto.CertificateNumber,
                StateNumber = documentDto.StateNumber,
                DocumentNumber = documentDto.DocumentNumber,
                UinAccruals = documentDto.UinAccruals,
                UserCommonId = userId
            };

            _context.Documents.Add(document);

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost("AddReview/{userId}")]
        public async Task<ActionResult> AddReview(int userId, ReviewCreateDto reviewDto)
        {
            var user = await _context.UserBusinesses.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                UserBusinessId = userId,
                Content = reviewDto.Content,
                Rating = reviewDto.Rating
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
