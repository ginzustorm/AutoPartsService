using AutoPartsServiceWebApi.Data;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseCreator _databaseCreator;

        public DatabaseController(DatabaseCreator databaseCreator)
        {
            _databaseCreator = databaseCreator;
        }

        [HttpPost("createlocaldb")]
        public IActionResult CreateLocalDatabase(string databaseName)
        {
            try
            {
                _databaseCreator.CreateLocalDatabase(databaseName);
                return Ok("Local database created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("createdb")]
        public IActionResult CreateDatabase(string ip, string login, string password, string databaseName)
        {
            try
            {
                _databaseCreator.CreateDatabase(ip, login, password, databaseName);
                return Ok("Database created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
