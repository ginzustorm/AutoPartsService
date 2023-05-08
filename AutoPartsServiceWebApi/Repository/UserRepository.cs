using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AutoPartsServiceWebApi.Repository
{
    public class UserRepository
    {
        private readonly MyDbContext _context;

        public UserRepository(MyDbContext context)
        {
            _context = context;
        }

        public User GetUserByIdAndDeviceId(int userId, string deviceId)
        {
            return _context.Users.Include(u => u.Address).FirstOrDefault(u => u.Id == userId && u.DeviceId == deviceId);
        }

        public User GetUserByPhoneNumber(string phoneNumber)
        {
            return _context.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
        }

        public User GetUserByPhoneNumberAndSmsCode(string phoneNumber, string smsCode)
        {
            Console.WriteLine($"Looking for user with PhoneNumber: {phoneNumber} and SmsCode: {smsCode}");
            var user = _context.Users.Include(u => u.SmsRecords)
                         .FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.SmsRecords.Any(s => s.Code == smsCode));

            if (user != null)
            {
                Console.WriteLine($"User found with Id: {user.Id}, PhoneNumber: {user.PhoneNumber}");
            }
            else
            {
                Console.WriteLine("No user found with the specified PhoneNumber and SmsCode.");
            }

            return user;
        }


        public void AddOrUpdateUser(User user)
        {
            if (user.Id == 0)
            {
                _context.Users.Add(user);
            }
            else
            {
                _context.Users.Update(user);
            }
            _context.SaveChanges();
        }


        public void AddSmsCode(Sms sms)
        {
            _context.SmsCodes.Add(sms);
            _context.SaveChanges();
        }


        public void UpdateUserDeviceId(int userId, string deviceId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.DeviceId = deviceId;
                _context.SaveChanges();
            }
        }

        public User GetUserById(int userId)
        {
            return _context.Users.Include(u => u.Address).FirstOrDefault(u => u.Id == userId);
        }

        public string GetHighestDeviceId()
        {
            var highestDeviceId = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.DeviceId))
                .OrderByDescending(u => u.DeviceId)
                .Select(u => u.DeviceId)
                .FirstOrDefault();

            return highestDeviceId;
        }


    }
}
