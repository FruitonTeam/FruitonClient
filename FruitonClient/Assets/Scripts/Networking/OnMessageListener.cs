using Cz.Cuni.Mff.Fruiton.Dto;

namespace Networking
{
    /// <summary>
    /// Interface for websocket message listeners
    /// </summary>
    public interface IOnMessageListener
    {

        void OnMessage(WrapperMessage message);

    }
}

