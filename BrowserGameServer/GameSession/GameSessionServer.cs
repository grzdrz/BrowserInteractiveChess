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
            get 
            {
                if (Players.ContainsKey(1))
                    if (Players[1] != null)
                        return Players[1];
                return null;
            }
        }
        public Player SecondPlayer
        {
            get
            {
                if (Players.ContainsKey(2))
                    if (Players[2] != null)
                        return Players[2];
                return null;
            }
        }

        public int PORT;
        public IPAddress ip;
        public IPEndPoint endPoint;
        Socket serverSocket;

        public string CommonDataHub = "";

        public GameSessionServer()
        {
            while (!IsUnoccupiedPort(PORT = CreateRandomPort())) ;//генерирует новый допустимый порт для сервера сессии
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
            if (FirstPlayer.PlayerState == PlayerStates.ActiveLeading || FirstPlayer.PlayerState == PlayerStates.ActiveWaiting &&
                SecondPlayer.PlayerState == PlayerStates.ActiveLeading || SecondPlayer.PlayerState == PlayerStates.ActiveWaiting)
            {
                temp = FirstPlayer.PlayerState;
                FirstPlayer.PlayerState = SecondPlayer.PlayerState;
                SecondPlayer.PlayerState = temp;
                return true;
            }
            return false;  
        }

        static object locker = new object();//такая синхронизация в пуле не работает???
        public int countOfCalls = 0;
        public void FinalizeSession()
        {//вызывается из 2х обработчиков веб сокетов по очереди, но срабатывает 1 раз
            lock (locker)
            {
                if (FirstPlayer != null)
                    FirstPlayer.PlayerState = PlayerStates.Disconnected;
                if (SecondPlayer != null)
                    SecondPlayer.PlayerState = PlayerStates.Disconnected;

                if (countOfCalls != 0)
                    return;

                countOfCalls++;
                MvcApplication.ActiveGameSessions.Remove(this);//удаляем объект сессии из колекции сессий, что бы его мог съесть мусорщик
                this.serverSocket.Close();//закрываем серверный сокет
                //клиентские сокеты/потоки закроются сами после выставления статуса Disconnected и/или разрыва сокета со стороны клиента

                Debug.WriteLine(">>Sessions count: " + MvcApplication.ActiveGameSessions.Count + "<<");
            }
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