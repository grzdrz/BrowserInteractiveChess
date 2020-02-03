using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace BrowserGameServer.GameSession
{
    public enum PlayerStates
    {
        WaitBegining,
        ActiveLeading,
        ActiveWaiting,
        Winner,
        Loser,
        Disconnected
    }

    public enum Side
    {
        Black,
        White
    }

    public class Player
    {
        public string PlayerLogin;
        public string PlayerAddress;

        public WebSocketHandler PlayerHandler;

        public PlayerStates PlayerState = PlayerStates.WaitBegining;
        public Side Side;
    }
}