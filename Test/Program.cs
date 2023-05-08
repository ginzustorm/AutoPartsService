using System;
using AutoPartsServiceWebApi.Controllers;
using AutoPartsServiceWebApi.Models;
using AutoPartsServiceWebApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of UserRepository
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            optionsBuilder.UseSqlServer("\"Server=(localdb)\\\\mssqllocaldb;Database=AutoPartsDb;Trusted_Connection=True;MultipleActiveResultSets=true\"");
            MyDbContext dbContext = new MyDbContext(optionsBuilder.Options);
            UserRepository userRepository = new UserRepository(dbContext);


            // Create an instance of UserController
            UserController userController = new UserController(userRepository);

            string phoneNumber = "1234567890";
            string deviceId = "test_device_id";

            userController.SendSms(phoneNumber);

            string smsCode = "1234";

            IActionResult jwtResult = userController.VerifyPhoneNumberAndSmsCode(phoneNumber, smsCode);
            var jwtResultValue = ((ObjectResult)jwtResult).Value;

            Console.WriteLine(jwtResultValue);

            IActionResult userInfoResult = userController.CheckJwtAndDeviceId(jwtResultValue.ToString(), deviceId);
            var userInfoResultValue = ((ObjectResult)userInfoResult).Value;

            Console.WriteLine(userInfoResultValue);
        }
    }
}
