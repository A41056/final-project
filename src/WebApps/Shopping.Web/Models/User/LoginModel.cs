namespace Shopping.Web.Models.User
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new LoginRequest();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _userService.Login(LoginRequest);
                if (!string.IsNullOrEmpty(response.Token))
                {
                    // Lưu token vào Session
                    HttpContext.Session.SetString("UserToken", response.Token);
                    return RedirectToPage("/Index");
                }
            }
            catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Invalid email or password.";
            }

            return Page();
        }
    }
}
