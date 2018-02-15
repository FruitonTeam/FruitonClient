using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Chat
{
    /// <summary>
    /// Stores chat history with a friend and additional chat status information
    /// </summary>
    class ChatRecord
    {
        /// <summary>
        /// List of previous chat messages
        /// One entry may contain more than one message, up to the character limit of single unity text gameObject
        /// </summary>
        public List<string> Messages = new List<string>();
        /// <summary>
        /// ID of oldest chat message that was loaded from server
        /// </summary>
        public string LastMessageId;
        /// <summary>
        /// Indicates whether the game is currently loading older mesagges for given friend
        /// </summary>
        public bool Loading;
        /// <summary>
        /// True if every chat message was already loaded from the server
        /// </summary>
        public bool LoadedEveryMessage;

        public ChatRecord()
        {
            Messages.Add("");
        }
    }
}
