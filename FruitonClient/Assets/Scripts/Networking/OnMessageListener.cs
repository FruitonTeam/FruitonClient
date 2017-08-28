using Cz.Cuni.Mff.Fruiton.Dto;

namespace Networking
{
    public interface IOnMessageListener
    {

        void OnMessage(WrapperMessage message);

    }
}

