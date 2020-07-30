using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrowserChess.GameSessionSignalR
{
    public class SessionInfo
    {
        public int SessionId { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public Player FirstPlayer
        {
            get
            {
                if (Players.Count > 0)
                    if (Players[0] != null)
                        return Players[0];
                return null;
            }
        }
        public Player SecondPlayer
        {
            get
            {
                if (Players.Count > 1)
                    if (Players[1] != null)
                        return Players[1];
                return null;
            }
        }

        public string CommonDataHub { get; set; } = "";

        public bool IsGameActive 
        {
            get 
            {
                return FirstPlayer != null && SecondPlayer != null;
            } 
        }
    }
}