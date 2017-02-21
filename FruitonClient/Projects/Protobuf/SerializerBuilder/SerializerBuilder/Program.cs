using DataModels;
using ProtoBuf.Meta;

namespace SerializerBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = TypeModel.Create();
            model.Add(typeof(User), true);

            model.AllowParseableTypes = true;
            model.AutoAddMissingTypes = true;

            model.Compile("ModelSerializer", "ModelSerializer.dll");
        }
    }
}
