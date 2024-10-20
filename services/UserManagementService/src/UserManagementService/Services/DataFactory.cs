using AutoMapper;

namespace UserManagementService.Services
{
    public class DataFactory : IDataFactory
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public DataFactory(ITokenService tokenService, IMapper mapper)
        {
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<RegisterDto> CreateRegisterDtoAsync(User user)
        {
            return new RegisterDto
            {
                TokenData = await CreateTokenDtoAsync(user),
                UserData = await CreateUserDtoAsync(user)
            };
        }

        public Role CreateRoleInstance(string roleName)
        {
            return new Role
            {
                Name = roleName
            };
        }

        public UserCreatedEvent CreateSubscribeMessageEvent(User user)
        {
            return _mapper.Map<UserCreatedEvent>(user);
        }

        public async Task<TokenDto> CreateTokenDtoAsync(User user)
        {
            return new TokenDto
            {
                Token = await _tokenService.GenerateJWTToken(user)
            };
        }

        public async Task<UserDTO> CreateUserDtoAsync(User user)
        {
            return _mapper.Map<UserDTO>(user);
        }

        public User CreateUserInstance(CreateUserDTO registerData)
        {
            return _mapper.Map<User>(registerData);
        }

        public User UpdateUser(EditUserDto data)
        {
            return _mapper.Map<User>(data);
        }
    }
}
