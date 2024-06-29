using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserManagementService.Tests
{
    public class UserManagementServiceUnitTests
    {
        private DbContextOptions<ECommerceDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<ECommerceDbContext>().UseInMemoryDatabase(databaseName: "UnitTestsDB").Options;
        }

        private Mock<UserManager<User>> GetMockedUserManager(User user, List<string> roles, bool validPassword = true, bool skip = false, bool skipEmail = false)
        {
            var store = new Mock<IUserStore<User>>();
            var mockedUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            mockedUserManager.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mockedUserManager.Setup(s => s.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(skip ? (User)null! : user);
            mockedUserManager.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(skip ? (User)null! : user);
            mockedUserManager.Setup(s => s.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(skipEmail ? (User)null! : user);
            mockedUserManager.Setup(s => s.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(roles);
            mockedUserManager.Setup(s => s.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mockedUserManager.Setup(s => s.AddClaimsAsync(It.IsAny<User>(), It.IsAny<IEnumerable<Claim>>())).ReturnsAsync(IdentityResult.Success);
            mockedUserManager.Setup(s => s.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(validPassword);
            mockedUserManager.Setup(s => s.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            mockedUserManager.Setup(s => s.SetAuthenticationTokenAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            return mockedUserManager;
        }

        private Mock<RoleManager<Role>> GetMockedRoleManager()
        {
            var store = new Mock<IRoleStore<Role>>();
            var mockedRoleManager = new Mock<RoleManager<Role>>(store.Object, null!, null!, null!, null!);

            mockedRoleManager.Setup(s => s.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            mockedRoleManager.Setup(s => s.CreateAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Success);

            return mockedRoleManager;
        }

        private Mock<IMapper> GetMockedMapper(UserDTO userdto, User user)
        {
            var mockedMapper = new Mock<IMapper>();

            mockedMapper.Setup(s => s.Map<UserDTO>(It.IsAny<User>())).Returns(userdto);
            mockedMapper.Setup(s => s.Map<User>(It.IsAny<CreateUserDTO>())).Returns(user);

            return mockedMapper;
        }

        private Mock<ITokenService> GetMockedToken(string token)
        {
            var mockedTokenService = new Mock<ITokenService>();
            mockedTokenService.Setup(s => s.GenerateJWTToken(It.IsAny<User>())).ReturnsAsync(token);

            return mockedTokenService;
        }

        private IRegisterService GetRegisterService(User user, User userMap, UserDTO userDto, bool skip = false)
        {
            var mockedLogger = new Mock<ILogger<RegisterService>>();

            var roles = new List<string>() { "User" };

            var mockedUserManager = GetMockedUserManager(user, roles, skip: skip);
            var mockedRoleManager = GetMockedRoleManager();
            var mockedMapper = GetMockedMapper(userDto, userMap);
            var mockedTokenService = GetMockedToken("USER_TOKEN");

            var registerService = new RegisterService(mockedUserManager.Object, mockedRoleManager.Object, mockedLogger.Object, mockedMapper.Object, mockedTokenService.Object);

            return registerService;
        }

        private ILoginService GetLoginService(User user, bool validPassword = true)
        {
            var mockedLogger = new Mock<ILogger<LoginService>>();
            var roles = new List<string>() { "User" };

            var mockedUserManager = GetMockedUserManager(user, roles, validPassword);
            var mockedTokenService = GetMockedToken("USER_TOKEN");

            var loginService = new LoginService(mockedUserManager.Object, mockedTokenService.Object, mockedLogger.Object);

            return loginService;
        }

        private User GenerateUserData()
        {
            var dob = new DateTime(2024, 01, 01, 0, 0, 0, 0, 0, DateTimeKind.Utc);

            var user = new User
            {
                UserName = "testUserName",
                City = "City",
                Country = "Country",
                Email = "user@domain.com",
                DateOfBirth = dob,
                FirstName = "Name",
                LastName = "Surname",
                LoyaltyPoints = 0,
                Street = "Street",
                MembershipLevel = MembershipLevels.Silver.ToString(),
                PhoneNumber = "1234",
                PostalCode = "1234",
                PreferredCurrency = "EUR",
                PreferredLanguage = "EN",
                State = "State"
            };

            return user;
        }

        private UserDTO GenerateUserDtoData()
        {
            var userDto = new UserDTO()
            {
                Username = "testUserName",
                City = "City",
                Country = "Country",
                Email = "user@domain.com",
                DateOfBirth = "01/01/2024",
                FirstName = "Name",
                LastName = "Surname",
                LoyaltyPoints = 0,
                Street = "Street",
                MembershipLevel = MembershipLevels.Silver.ToString(),
                PhoneNumber = "1234",
                PostalCode = "1234",
                PreferredCurrency = "EUR",
                PreferredLanguage = "EN",
                State = "State",
                Role = "User"
            };

            return userDto;
        }

        private CreateUserDTO GenerateCreateUserDtoData()
        {
            var createUserDto = new CreateUserDTO()
            {
                Username = "testUserName",
                City = "City",
                Country = "Country",
                Email = "user@domain.com",
                DateOfBirth = "01/01/2024",
                FirstName = "Name",
                LastName = "Surname",
                Street = "Street",
                PhoneNumber = "1234",
                PostalCode = "1234",
                PreferredCurrency = "EUR",
                PreferredLanguage = "EN",
                State = "State",
                Password = "123",
                ConfirmPassword = "123"
            };

            return createUserDto;
        }

        private JwtSecurityToken DecryptJWT(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = jsonToken as JwtSecurityToken;

            return tokenS!;
        }

        [Fact]
        public async Task Register_NonMatchingPasswords_ShouldFail()
        {
            var registerService = GetRegisterService(null!, null!, null!);

            var createUserDto = new CreateUserDTO()
            {
                Password = "123",
                ConfirmPassword = "1234"
            };

            var result = await registerService.RegisterUser(createUserDto);

            Assert.False(result.Succeeded);
            Assert.Equal(GlobalConstants.PasswordsDoNotMatch, result.Message);
        }

        [Fact]
        public async Task Register_UserWithExistingEmail_ShouldFail()
        {
            var user = new User
            {
                Email = "user@emailDomain.com"
            };

            var registerService = GetRegisterService(user, user, null!, true);

            var createUserDto = new CreateUserDTO()
            {
                Email = "user@emailDomain.com"
            };

            var result = await registerService.RegisterUser(createUserDto);

            Assert.False(result.Succeeded);
            Assert.Equal(GlobalConstants.EmailAlreadyExists, result.Message);
        }

        [Fact]
        public async Task Register_UserWithExistingUsername_ShouldFail()
        {
            var user = new User
            {
                UserName = "testUserName"
            };

            var registerService = GetRegisterService(user, user, null!);

            var createUserDto = new CreateUserDTO()
            {
                Username = "testUserName"
            };

            var result = await registerService.RegisterUser(createUserDto);

            Assert.False(result.Succeeded);
            Assert.Equal(GlobalConstants.UsernameAlreadyExists, result.Message);
        }

        [Fact]
        public async Task Register_Valid_ShouldPass()
        {
            var user = GenerateUserData();
            var userDto = GenerateUserDtoData();

            var registerService = GetRegisterService(null!, user, userDto);

            var createUserDto = GenerateCreateUserDtoData();

            var result = await registerService.RegisterUser(createUserDto);
            var createdUser = result.Data.UserData;

            Assert.True(result.Succeeded);
            Assert.Equal(createdUser.State, user?.State);
            Assert.Equal(createdUser.Street, user?.Street);
            Assert.Equal(createdUser.Country, user?.Country);
            Assert.Equal(createdUser.FirstName, user?.FirstName);
            Assert.Equal(createdUser.Email, user?.Email);
            Assert.Equal(createdUser.LastName, user?.LastName);
            Assert.Equal(createdUser.DateOfBirth, user?.DateOfBirth!.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
            Assert.Equal(createdUser.LoyaltyPoints, user?.LoyaltyPoints);
            Assert.Equal(createdUser.MembershipLevel, user?.MembershipLevel);
            Assert.Equal(createdUser.PreferredCurrency, user?.PreferredCurrency);
            Assert.Equal(createdUser.PreferredLanguage, user?.PreferredLanguage);
            Assert.Equal(createdUser.Username, user?.UserName);
            Assert.Equal(createdUser.City, user?.City);
            Assert.Equal(createdUser.PhoneNumber, user?.PhoneNumber);
            Assert.Equal(createdUser.PostalCode, user?.PostalCode);
            Assert.Equal("User", createdUser.Role);
            Assert.Equal("USER_TOKEN", result.Data.TokenData.Token);
            Assert.Equal(string.Format(GlobalConstants.UserCreatedSuccessfully, createdUser.Username), result.Message);
        }

        [Fact]
        public async Task Login_Valid_ShouldPass()
        {
            var loginDto = new LoginDto()
            {
                Password = "123",
                Username = "testUser"
            };

            var user = GenerateUserData();

            var loginService = GetLoginService(user, true);

            var result = await loginService.LoginUser(loginDto);

            Assert.True(result.Succeeded);
            Assert.Equal("USER_TOKEN", result.Data.Token);
            Assert.Equal(string.Format(GlobalConstants.UserLoggedInSuccessfully, user.UserName), result?.Message);
        }

        [Fact]
        public async Task Login_NonExistentUser_ShouldFail()
        {
            var loginDto = new LoginDto() { };

            var loginService = GetLoginService(null!);

            var result = await loginService.LoginUser(loginDto);

            Assert.False(result.Succeeded);
            Assert.Equal(GlobalConstants.UserNotFound, result.Message);
        }

        [Fact]
        public async Task Login_IncorrectPassword_ShouldFail()
        {
            var loginDto = new LoginDto()
            {
                Password = "123",
                Username = "testUser"
            };

            var user = GenerateUserData();

            var loginService = GetLoginService(user, false);

            var result = await loginService.LoginUser(loginDto);

            Assert.False(result.Succeeded);
            Assert.Equal(GlobalConstants.UserEnteredWrongPassword, result.Message);
        }

        [Fact]
        public async Task Token_GenerateAValidToken_ShouldPass()
        {
            var user = GenerateUserData();
            var userManager = GetMockedUserManager(user, new List<string> { "User" });
            var logger = new Mock<ILogger<TokenService>>();

            string issuer = "testIssuer";
            string key = "testKey-testKey-testKey-testKey-testKey";

            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "UserManagement:JWT:Issuer", issuer },
                { "UserManagement:JWT:Key", key }
            }).Build();

            var tokenService = new TokenService(userManager.Object, logger.Object, config);

            var token = await tokenService.GenerateJWTToken(user);
            var decryptedToken = DecryptJWT(token);

            var issuedAt = long.Parse(decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Iat)?.Value!);
            var issuedAtDateTime = DateTimeOffset.FromUnixTimeSeconds(issuedAt).UtcDateTime;
            var expiresAtDateTime = issuedAtDateTime.AddMinutes(60);
            var expiresAtUnixTime = new DateTimeOffset(expiresAtDateTime).ToUnixTimeSeconds().ToString();

            Assert.False(string.IsNullOrEmpty(token));
            Assert.Equal(issuer, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Iss)?.Value);
            Assert.Equal(user.UserName, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value);
            Assert.Equal(user.FirstName, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.GivenName)?.Value);
            Assert.Equal(user.LastName, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.FamilyName)?.Value);
            Assert.Equal(user.Email, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value);
            Assert.Equal(issuer, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);
            Assert.Equal(expiresAtUnixTime, decryptedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value);

            userManager.Verify(x => x.AddClaimsAsync(It.IsAny<User>(), It.IsAny<IEnumerable<Claim>>()), Times.Once);
            userManager.Verify(x => x.SetAuthenticationTokenAsync(It.IsAny<User>(), GlobalConstants.LoginProvider, GlobalConstants.JWT, token), Times.Once);
        }

        [Fact]
        public async Task Profile_GetAUserInvalidId_ShouldFail()
        {
            var user = GenerateUserData();
            var userDto = GenerateUserDtoData();
            var userManager = GetMockedUserManager(user, new List<string> { "User" }, true, true);
            var mapper = GetMockedMapper(userDto, user);
            var logger = new Mock<ILogger<ProfileService>>();

            var profileService = new ProfileService(userManager.Object, mapper.Object, logger.Object);

            var result = await profileService.GetUser("2");

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Profile_GetAUser_ShouldPass()
        {
            var user = GenerateUserData();
            var userDto = GenerateUserDtoData();
            var userManager = GetMockedUserManager(user, new List<string> { "User" });
            var mapper = GetMockedMapper(userDto, user);
            var logger = new Mock<ILogger<ProfileService>>();

            var profileService = new ProfileService(userManager.Object, mapper.Object, logger.Object);

            var result = await profileService.GetUser("1");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Profil_UpdateUserInvalidId_ShouldFail()
        {
            var user = GenerateUserData();
            var userDto = GenerateUserDtoData();
            var userManager = GetMockedUserManager(user, new List<string> { "User" }, true, true);
            var mapper = GetMockedMapper(userDto, user);
            var logger = new Mock<ILogger<ProfileService>>();

            var profileService = new ProfileService(userManager.Object, mapper.Object, logger.Object);

            var editData = new EditUserDto
            {

            };

            var result = await profileService.UpdateUser("2", editData);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Profil_UpdateUserExistingEmail_ShouldFail()
        {
            var user = GenerateUserData();
            var userDto = GenerateUserDtoData();
            var userManager = GetMockedUserManager(user, new List<string> { "User" });
            var mapper = GetMockedMapper(userDto, user);
            var logger = new Mock<ILogger<ProfileService>>();

            var profileService = new ProfileService(userManager.Object, mapper.Object, logger.Object);

            var editData = new EditUserDto
            {
                Email = user.Email
            };

            var result = await profileService.UpdateUser("1", editData);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Profil_UpdateUser_ShouldPass()
        {
            var user = GenerateUserData();
            var userDto = new UserDTO
            {
                City = "updatedCity",
                Country = "updatedCountry",
                Email = "updatedEmail",
                FirstName = "updatedFirstName",
                LastName = "updatedSurname",
                PhoneNumber = "updatedPhoneNumber",
                PostalCode = "updatedPostalCode",
                PreferredCurrency = "USD",
                PreferredLanguage = "DE",
                State = "updatedState",
                Street = "updatedStreet",
                DateOfBirth = "01/01/2020",
                Id = "1",
                LoyaltyPoints = 0,
                MembershipLevel = MembershipLevels.Silver.ToString(),
                Role = "User",
                Username = "TestUser"
            };
            var userManager = GetMockedUserManager(user, new List<string> { "User" }, true, false, true);
            var mapper = GetMockedMapper(userDto, user);
            var logger = new Mock<ILogger<ProfileService>>();

            var profileService = new ProfileService(userManager.Object, mapper.Object, logger.Object);

            var editData = new EditUserDto
            {
                City = "updatedCity",
                Country = "updatedCountry",
                Email = "updatedEmail",
                FirstName = "updatedFirstName",
                LastName = "updatedSurname",
                PhoneNumber = "updatedPhoneNumber",
                PostalCode = "updatedPostalCode",
                PreferredCurrency = "USD",
                PreferredLanguage = "DE",
                State = "updatedState",
                Street = "updatedStreet",
                DateOfBirth = "01/01/2020"
            };

            var result = await profileService.UpdateUser("1", editData);

            var updatedUser = result.Data;

            Assert.True(result.Succeeded);
            Assert.Equal(editData.City, updatedUser.City);
            Assert.Equal(editData.Country, updatedUser.Country);
            Assert.Equal(editData.Email, updatedUser.Email);
            Assert.Equal(editData.FirstName, updatedUser.FirstName);
            Assert.Equal(editData.LastName, updatedUser.LastName);
            Assert.Equal(editData.PhoneNumber, updatedUser.PhoneNumber);
            Assert.Equal(editData.PostalCode, updatedUser.PostalCode);
            Assert.Equal(editData.PreferredLanguage, updatedUser.PreferredLanguage);
            Assert.Equal(editData.PreferredCurrency, updatedUser.PreferredCurrency);
            Assert.Equal(editData.State, updatedUser.State);
            Assert.Equal(editData.Street, updatedUser.Street);
            Assert.Equal(editData.DateOfBirth, updatedUser.DateOfBirth);
        }
    }
}