namespace UserManagementService.Services.Interfacces
{
    public interface IDataFactory
    {
        User CreateUserInstance(CreateUserDTO registerData);
        UserDTO CreateUserDto(User user);
        Task<RegisterDto> CreateRegisterDtoAsync(User user);
        Task<TokenDto> CreateTokenDtoAsync(User user);
        UserCreatedEvent CreateUserCreatedEvent(User user);
        Role CreateRoleInstance(string roleName);
        User UpdateUser(EditUserDto data);
    }
}
