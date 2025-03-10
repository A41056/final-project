using Shopping.Web.Models.User;

namespace Shopping.Web.Services
{
    public interface IUserService
    {
        [Post("/user-service/Register")]
        Task<UserResponse> Register(RegisterRequest request);

        [Post("/user-service/Login")]
        Task<UserResponse> Login(LoginRequest request);

        //[Put("/user-service/users/{id}")]
        //Task<UserResponse> UpdateUser(Guid id, Update request);
    }
}
