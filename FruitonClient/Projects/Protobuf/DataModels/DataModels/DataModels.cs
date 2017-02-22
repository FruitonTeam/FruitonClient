using ProtoBuf;

namespace DataModels
{
    [ProtoContract]
    public class RegistrationForm
    {
        [ProtoMember(1)]
        public string login;
        [ProtoMember(2)]
        public string password;
        [ProtoMember(3)]
        public string email;

        public RegistrationForm(string login, string password, string email)
        {
            this.login = login;
            this.password = password;
            this.email = email;
        }
    }

    [ProtoContract]
    public class LoginForm
    {
        [ProtoMember(1)]
        public string login;
        [ProtoMember(2)]
        public string password;

        public LoginForm(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
    }
}
