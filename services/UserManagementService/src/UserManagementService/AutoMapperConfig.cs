using AutoMapper;

namespace UserManagementService
{
    public class AutoMapperConfig
    {
        public static IMapper Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateUserDTO, User>();
                cfg.CreateMap<User, UserDTO>();
            });

            return config.CreateMapper();
        }
    }
}
