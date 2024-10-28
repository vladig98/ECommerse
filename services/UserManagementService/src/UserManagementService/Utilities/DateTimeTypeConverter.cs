using AutoMapper;
using System.Globalization;

namespace UserManagementService.Utilities
{
    public class DateTimeTypeConverter : ITypeConverter<string, DateTime?>
    {
        private const string DateTimeFormat = "dd/MM/yyyy";

        public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
        {
            bool isValidDate = DateTime.TryParseExact(source, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob);

            return isValidDate ? dob : null;
        }
    }
}
