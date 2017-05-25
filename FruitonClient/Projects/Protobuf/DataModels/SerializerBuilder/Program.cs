
using DataModels;
using ProtoBuf.Meta;

namespace SerializerBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = TypeModel.Create();
            model.Add(typeof(RegistrationForm), true);
            model.Add(typeof(LoginForm), true);
            model.Add(typeof(Salad), true);
            model.Add(typeof(SaladList), true);

            model.AllowParseableTypes = true;
            model.AutoAddMissingTypes = true;

            model.Compile("ModelSerializer", "ModelSerializer.dll");
        }
    }
}
