namespace GenericBackoffice.models.auth
{
    public class AuthenticationStatus
    {
        public bool IsAuthenticated { get; set; }
        public User User { get; set; }

        public static implicit operator AuthenticationStatus(User user)
        {
            if (null == user)
                return Fail;
            return new AuthenticationStatus
            {
                IsAuthenticated = true,
                User = user
            };
        }

        public static AuthenticationStatus Fail => new AuthenticationStatus();
    }
}