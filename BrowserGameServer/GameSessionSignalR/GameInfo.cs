using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrowserGameServer.GameSessionSignalR
{
    public class GameInfo
    {
        public Side Side { get; set; }
        public PlayerStates PlayerState { get; set; }
        public string BoardState { get; set; }
    }
}