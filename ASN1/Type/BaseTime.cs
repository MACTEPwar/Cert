using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type
{
    public abstract class BaseTime : Element, ITypeTime
    {
        const string TZ_UTC = "UTC";
        protected DateTime dateTime;
        public BaseTime(DateTime dateTime)
        {
            this.dateTime = dateTime;

        }

        public override string ToString()
        {
            return String();
        }

        public static T FromString<T>(string time, string tz = null) where T : BaseTime
        {
            try
            {
                if (string.IsNullOrEmpty(tz))
                {
                    tz = TimeZoneInfo.Local.Id;
                }
                DateTime res;
                if (System.DateTime.TryParse(time, out res))
                {
                    var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    var dateTimeImmutable = TimeZoneInfo.ConvertTimeFromUtc(res, tzInfo);
                    return (T)Activator.CreateInstance(typeof(T), dateTimeImmutable);
                } else
                {
                    throw new Exception("Cant parse dateTime");
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to create DateTime. {ex.Message}.");
            }
        }

        public DateTime DateTime() => dateTime;

        public string String() => EncodedContentDER();

        protected static TimeZoneInfo CreateTimeZone(string tz)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            catch (Exception e)
            {
                throw new Exception("Invalid timezone.");
            }
        }

        protected static string GetLastDateTimeImmutableErrorsStr()
        {
            return "GetLastDateTimeImmutableErrorsStr not implemented";
        }
    }
}
