using AutoMapper;
using System.Globalization;

namespace UserManagementService.Utilities
{
    public class DateTimeTypeConverter : ITypeConverter<string, DateTime?>
    {
        public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
        {
            bool success = DateTime.TryParseExact(source, GlobalConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob);

            if (!success)
            {
                return null;
            }

            return dob;
        }
    }
}
