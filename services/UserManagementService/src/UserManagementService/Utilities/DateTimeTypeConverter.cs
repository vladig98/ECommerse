using AutoMapper;
using System.Globalization;

namespace UserManagementService.Utilities
{
    public class DateTimeTypeConverter : ITypeConverter<string, DateTime?>
    {
        public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
        {
            bool isValidDate = DateTime.TryParseExact(source, GlobalConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob);

            return isValidDate ? dob : null;
        }
    }
}
