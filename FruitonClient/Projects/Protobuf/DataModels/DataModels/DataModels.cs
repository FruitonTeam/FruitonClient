using ProtoBuf;

namespace DataModels
{
    [ProtoContract]
    public class User
    {
        [ProtoMember(1)]
        public string login;
        [ProtoMember(2)]
        public string password;
        [ProtoMember(3)]
        public string email;

        public User(string login, string password, string email)
        {
            this.login = login;
            this.password = password;
            this.email = email;
        }
    }
}
