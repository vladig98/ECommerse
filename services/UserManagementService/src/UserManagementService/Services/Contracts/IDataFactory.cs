namespace UserManagementService.Services.Contracts
{
    public interface IDataFactory
    {
        User CreateUserInstance(CreateUserDTO registerData);
        Task<UserDTO> CreateUserDtoAsync(User user);
        Task<RegisterDto> CreateRegisterDtoAsync(User user);
        Task<TokenDto> CreateTokenDtoAsync(User user);
        UserCreatedEvent CreateSubscribeMessageEvent(User user);
        Role CreateRoleInstance(string roleName);
    }
}
