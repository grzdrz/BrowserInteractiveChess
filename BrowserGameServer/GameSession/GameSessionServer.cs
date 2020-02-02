using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;

namespace BrowserGameServer.GameSession
{
    public enum GameState
    {
        WaitingAllPlayers,
        InProcess,
        End
    }

    public class GameSessionServer
    {
        public GameState GameState = GameState.WaitingAllPlayers;

        public Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public int MaxPlayersCount = 2;
        public int PlayersCount
        {
            get { return Players.Count; }
        }
        public Player FirstPlayer
        {
            get { return Players[1]; }
        }
        public Player SecondPlayer
        {
            get { return Players[2]; }
        }

        public int PORT;
        public IPAddress ip;
        public IPEndPoint endPoint;
        Socket serverSocket;

        public string CommonDataHub = "";
        public string CurInnerHeight = "";
        public string CurInnerWidth = "";

        //запускаем сервер
        public GameSessionServer()
        {
            while (!IsUnoccupiedPort(PORT = CreateRandomPort())) ;
            ip = IPAddress.Parse(MvcApplication.GetLocalIPAddress());
            endPoint = new IPEndPoint(ip, PORT);
        }

        int TESTCounter = 0;
        public void StartServer()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);
            Debug.WriteLine("\nSERVER STARTED");
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Task task = Task.Run(() =>
                {
                    Debug.WriteLine("\nNew WSHandler " + (++TESTCounter));
                    WebSocketHandler newController = new WebSocketHandler(clientSocket, this);
                });
            }
        }

        public bool IsGameBeginning()
        {
            if (Players.Count == MaxPlayersCount)
            {
                GameState = GameState.InProcess;
                return true;
            }
            else return false;
        }

        public bool SwapActiveState()
        {
            PlayerStates temp;
            if (FirstPlayer.PlayerStates == PlayerStates.ActiveLeading || FirstPlayer.PlayerStates == PlayerStates.ActiveWaiting &&
                SecondPlayer.PlayerStates == PlayerStates.ActiveLeading || SecondPlayer.PlayerStates == PlayerStates.ActiveWaiting)
            {
                temp = FirstPlayer.PlayerStates;
                FirstPlayer.PlayerStates = SecondPlayer.PlayerStates;
                SecondPlayer.PlayerStates = temp;
                return true;
            }
            return false;  
        }

        //public DateTime MoveStartTime = DateTime.Now;
        //public DateTime MoveCurrentTime = DateTime.Now;
        //public TimeSpan MoveDTime = TimeSpan.Zero;
        //public int CheckMoveTime(double minutes)
        //{
        //    MoveCurrentTime = DateTime.Now;
        //    MoveDTime = MoveCurrentTime - MoveStartTime;
        //    //if (MoveDTime.TotalMilliseconds > (1000d * 60d * minutes))
        //    //{
        //    //    //MoveStartTime = DateTime.Now;
        //    //    return true;
        //    //}
        //    return (int)Math.Truncate((double)(MoveDTime.Milliseconds) / (1000d * 60d * minutes));
        //}

        public void FinalizeSession()/////
        {
            
        }

        public int CreateRandomPort()
        {
            int iMin = 49152;
            int iMax = 65535;
            Random random = new Random();
            return random.Next(iMin, iMax);
        }

        public bool IsUnoccupiedPort(int port)
        {
            bool isAvailable = true;

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}