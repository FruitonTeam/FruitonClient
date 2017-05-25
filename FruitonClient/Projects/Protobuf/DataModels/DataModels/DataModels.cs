using ProtoBuf;
using System.Collections.Generic;

namespace DataModels
{

    [ProtoContract]
    public class SaladList
    {
        [ProtoMember(1)]
        public List<Salad> salads;

        public SaladList()
        {
            salads = new List<Salad>();
        }

        public void Add(Salad salad)
        {
            salads.Add(salad);
        }

        public int Count
        {
            get
            {
                return salads.Count;
            }
        }
    }

    [ProtoContract]
    public class Salad
    {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public List<string> fruitonIDs;

        public Salad()
        {
            fruitonIDs = new List<string>();
        }

        public Salad(string name, List<string> fruitonIDs)
        {
            this.name = name;
            this.fruitonIDs = fruitonIDs;
        }

        public Salad(string name)
        {
            this.name = name;
            fruitonIDs = new List<string>();
        }

        public Salad(List<string> ids)
        {
            fruitonIDs = ids;
        }

        public void Add(string fruitonID)
        {
            fruitonIDs.Add(fruitonID);
        }

    }

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
