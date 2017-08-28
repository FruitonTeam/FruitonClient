using System;

namespace Networking
{
    public class PlayerHelper
    {
        private PlayerHelper()
        {
        }

        public static void Exists(string player, Action<bool> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Post(
                    "player/exists?login=" + player,
                    s => success.Invoke("true".Equals(s)),
                    error
                )
            );
        }
    }
}