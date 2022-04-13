using System;
using System.Runtime.InteropServices;
using AutoMapper;
using Domain.Dtos;
using Domain.Models;
using TimeZoneConverter;

namespace Application.Helpers
{
    public class AttachmentCreatedDateResolver : IMemberValueResolver<object, object, DateTime, DateTime>, IMemberValueResolver<Location, LocationDto, DateTime, DateTime?>
    {
        private readonly string _defaultTimeZone;
        public string _timeZone;

        public AttachmentCreatedDateResolver(string timeZone)
        {
            _defaultTimeZone = timeZone;
        }

        public void SetTimeZone(string timeZone)
        {
            _timeZone = string.IsNullOrEmpty(timeZone) ? _defaultTimeZone : timeZone;
        }

        public DateTime Resolve(object source, object destination, DateTime sourceMember, DateTime destMember, ResolutionContext context)
        {
            try
            {
                //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZone);
                TimeZoneInfo cstZone = TZConvert.GetTimeZoneInfo(_timeZone);
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(sourceMember, cstZone);
                return cstTime;
            }
            catch (TimeZoneNotFoundException)
            {
                //Console.WriteLine("The registry does not define the Central Standard Time zone.");
                return sourceMember;
            }
            catch (InvalidTimeZoneException)
            {
                //Console.WriteLine("Registry data on the Central Standard Time zone has been corrupted.");
                return sourceMember;
            }
        }

        public DateTime? Resolve(Location source, LocationDto destination, DateTime sourceMember, DateTime? destMember,
            ResolutionContext context)
        {
            try
            {
                //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZone);
                TimeZoneInfo cstZone = TZConvert.GetTimeZoneInfo(_timeZone);
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(sourceMember, cstZone);
                return cstTime;
            }
            catch (TimeZoneNotFoundException)
            {
                //Console.WriteLine("The registry does not define the Central Standard Time zone.");
                return sourceMember;
            }
            catch (InvalidTimeZoneException)
            {
                //Console.WriteLine("Registry data on the Central Standard Time zone has been corrupted.");
                return sourceMember;
            }
        }
    }
}