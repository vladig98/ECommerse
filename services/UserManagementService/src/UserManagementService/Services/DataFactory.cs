using AutoMapper;

namespace UserManagementService.Services
{
    public class DataFactory(ITokenService tokenService, IMapper mapper) : IDataFactory
    {
        private readonly ITokenService tokenService = tokenService;
        private readonly IMapper mapper = mapper;

        public async Task<RegisterDto> CreateRegisterDtoAsync(User user)
        {
            return new RegisterDto
            {
                TokenData = await this.CreateTokenDtoAsync(user),
                UserData = this.CreateUserDto(user)
            };
        }

        public Role CreateRoleInstance(string roleName)
        {
            return new Role
            {
                Name = roleName
            };
        }

        public UserCreatedEvent CreateUserCreatedEvent(User user)
        {
            return this.mapper.Map<UserCreatedEvent>(user);
        }

        public async Task<TokenDto> CreateTokenDtoAsync(User user)
        {
            return new TokenDto
            {
                Token = await this.tokenService.GenerateJWTToken(user)
            };
        }

        public UserDTO CreateUserDto(User user)
        {
            return this.mapper.Map<UserDTO>(user);
        }

        public User CreateUserInstance(CreateUserDTO registerData)
        {
            return this.mapper.Map<User>(registerData);
        }

        public User UpdateUser(EditUserDto data)
        {
            return this.mapper.Map<User>(data);
        }
    }
}
