using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace BrowserGameServer.GameSessionSignalR
{
    public enum PlayerStates
    {
        WaitBegining,
        ActiveLeading,
        ActiveWaiting
    }

    public enum Side
    {
        Black,
        White
    }

    public class Player
    {
        public string ConnectionId { get; set; }//id в контексте хаба signalr
        public int PlayerNumber { get; set; }//формальный номер игрока, нужный для операции коннекта
        public string PlayerLogin { get; set; }
        public string PlayerAddress { get; set; }

        public PlayerStates PlayerState { get; set; } = PlayerStates.WaitBegining;
        public Side Side { get; set; }

        public SessionInfo PlayerSession { get; set; }
    }
}