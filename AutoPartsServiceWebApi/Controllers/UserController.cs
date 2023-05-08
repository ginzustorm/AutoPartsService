using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using AutoPartsServiceWebApi.Models;
using AutoPartsServiceWebApi.Repository;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AutoPartsServiceWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserController> _logger;


        public UserController(UserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }


        private static readonly string JwtSecret = GenerateJwtSecretKey();

        public static string GenerateJwtSecretKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var key = new byte[32];
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        [HttpGet("checkjwt")]
        public IActionResult CheckJwtAndDeviceId(string jwt, string deviceId)
        {
            try
            {
                var decoded = DecodeJwt(jwt);
                var userId = int.Parse(decoded["userId"].ToString());

                var user = _userRepository.GetUserByIdAndDeviceId(userId, deviceId);

                if (user != null)
                {
                    return Ok(new
                    {
                        user.Name,
                        user.Email,
                        user.PhoneNumber,
                        user.Role
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while checking JWT and Device ID: {ex.Message}");
            }


            return BadRequest("Invalid JWT or Device ID.");
        }

        [HttpPost("sendsms")]
        public IActionResult SendSms([FromBody] string phoneNumber)
        {
            var user = _userRepository.GetUserByPhoneNumber(phoneNumber);

            if (user == null)
            {
                // Get the highest DeviceId from the existing users
                string highestDeviceId = _userRepository.GetHighestDeviceId();
                int highestDeviceNumber = 0;

                if (!string.IsNullOrEmpty(highestDeviceId))
                {
                    // Extract the numeric part from the highest DeviceId
                    int.TryParse(highestDeviceId.Substring("device".Length), out highestDeviceNumber);
                }

                // Increment the numeric part and create the new DeviceId
                string newDeviceId = $"device{highestDeviceNumber + 1}";

                // Create a new user with the given phone number and auto-incremented DeviceId
                user = new User
                {
                    PhoneNumber = phoneNumber,
                    RegistrationDate = DateTime.UtcNow,
                    DeviceId = newDeviceId,
                    SmsRecords = new List<Sms>()
                };

                _userRepository.AddOrUpdateUser(user);
            }
            else if (user.SmsRecords == null)
            {
                // Initialize the SmsRecords list for the existing user if it's null
                user.SmsRecords = new List<Sms>();
            }

            // Generate a random SMS code
            var smsCode = GenerateSmsCode();

            // Log the SMS code for testing purposes
            _logger.LogInformation($"SMS code for user {phoneNumber}: {smsCode}");

            // Add the SMS code to the user's SMS records
            user.SmsRecords.Add(new Sms { PhoneNumber = phoneNumber, Code = smsCode, UserId = user.Id });

            // Save changes to the database
            _userRepository.AddOrUpdateUser(user);

            return Ok("SMS code sent.");
        }




        [HttpPost("verifyphoneandsmscode")]
        public IActionResult VerifyPhoneNumberAndSmsCode(string phoneNumber, string smsCode)
        {
            var user = _userRepository.GetUserByPhoneNumberAndSmsCode(phoneNumber, smsCode);

            if (user != null)
            {
                Console.WriteLine($"User found with Id: {user.Id}, PhoneNumber: {user.PhoneNumber}");
                var jwt = EncodeJwt(new Dictionary<string, object> { { "userId", user.Id } });
                return Ok(new
                {
                    Jwt = jwt,
                    user.Name,
                    user.Email,
                    user.PhoneNumber,
                    user.Role
                });
            }

            var newJwt = EncodeJwt(new Dictionary<string, object> { { "userId", 0 } });
            return Ok(new
            {
                Jwt = newJwt,
                Message = "New user, please provide additional information."
            });
        }


        [HttpPost("updateuser")]
        public IActionResult UpdateUser(string jwt, [FromBody] UpdateUserRequest updateUserRequest)
        {
            try
            {
                var decoded = DecodeJwt(jwt);
                var userId = decoded.ContainsKey("userId") ? int.Parse(decoded["userId"].ToString()) : 0;

                if (userId != 0)
                {
                    var existingUser = _userRepository.GetUserById(userId);
                    if (existingUser != null)
                    {
                        existingUser.Name = updateUserRequest.Name;
                        existingUser.Email = updateUserRequest.Email;
                        existingUser.Role = updateUserRequest.Role;

                        if (existingUser.Address == null)
                        {
                            existingUser.Address = new Address();
                        }

                        existingUser.Address.City = updateUserRequest.City;
                        existingUser.Address.Country = updateUserRequest.Country;
                        existingUser.Address.Street = updateUserRequest.Street;

                        _userRepository.AddOrUpdateUser(existingUser);
                        return Ok("User updated successfully.");
                    }
                    else
                    {
                        return BadRequest("User not found.");
                    }
                }
                else
                {
                    var newUser = new User
                    {
                        Name = updateUserRequest.Name,
                        Email = updateUserRequest.Email,
                        Role = updateUserRequest.Role,
                        Address = new Address
                        {
                            City = updateUserRequest.City,
                            Country = updateUserRequest.Country,
                            Street = updateUserRequest.Street
                        }
                    };

                    _userRepository.AddOrUpdateUser(newUser);
                    var newJwt = EncodeJwt(new Dictionary<string, object> { { "userId", newUser.Id } });
                    return Ok(new
                    {
                        Jwt = newJwt,
                        newUser.Name,
                        newUser.Email,
                        newUser.PhoneNumber,
                        newUser.Role
                    });
                }
            }
            catch (Exception)
            {
                return BadRequest("Invalid JWT.");
            }
        }

        private static string GenerateSmsCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string EncodeJwt(Dictionary<string, object> payload)
        {
            var jwt = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(JwtSecret)
                .AddClaims(payload)
                .Encode();

            Console.WriteLine($"Encoded JWT: {jwt} with payload: {JsonConvert.SerializeObject(payload)}");

            return jwt;
        }


        private static IDictionary<string, object> DecodeJwt(string jwt)
        {
            var jsonSerializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var validator = new JwtValidator(jsonSerializer, provider);
            var urlEncoder = new JwtBase64UrlEncoder();
            var algorithm = new HMACSHA256Algorithm();
            var decoder = new JwtDecoder(jsonSerializer, validator, urlEncoder, algorithm);

            var decoded = decoder.DecodeToObject<IDictionary<string, object>>(jwt, JwtSecret, verify: true);

            Console.WriteLine($"Decoded JWT: {jwt} to payload: {JsonConvert.SerializeObject(decoded)}");

            return decoded;
        }
    }

    public class UpdateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
    }
}
