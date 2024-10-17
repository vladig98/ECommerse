using AutoMapper;
using System.Globalization;

namespace UserManagementService.Utilities
{
    public class AutoMapperConfig
    {
        public static IMapper Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<string, DateTime?>().ConvertUsing<DateTimeTypeConverter>();
                cfg.CreateMap<CreateUserDTO, User>()
                    .ForMember(x => x.DateOfBirth, y => y.MapFrom(z => z.DateOfBirth));
                cfg.CreateMap<User, UserDTO>().ForMember(x => x.Roles, y => y.MapFrom(z => z.Roles.Select(r => r.Role.Name)));
                cfg.CreateMap<User, UserCreatedEvent>()
                    .ForMember(x => x.Roles, y => y.MapFrom(z => z.Roles.Select(r => r.Role.Name)))
                    .ForMember(x => x.DateOfBirth, y => y.MapFrom(z => z.DateOfBirth.Value.ToString(GlobalConstants.DateTimeFormat, CultureInfo.InvariantCulture)));
            });

            return config.CreateMapper();
        }
    }
}
