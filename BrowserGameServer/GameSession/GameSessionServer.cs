using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.Threading;

namespace BrowserGameServer.GameSession
{
    public class GameSessionServer
    {
        public SessionInfo SessionInfo { get; set; }
        public SessionManager SM { get; set; } = new SessionManager();

        public int PORT { get; set; }
        public IPAddress ip { get; set; }
        public IPEndPoint endPoint { get; set; }
        public Socket serverSocket { get; set; }

        public List<WebSocketHandler> ClientHandlers { get; set; } = new List<WebSocketHandler>();

        public GameSessionServer(SessionInfo sessionInfo)
        {
            SessionInfo = sessionInfo;
            while (!IsUnoccupiedPort(PORT = CreateRandomPort())) ;//генерирует новый допустимый порт для сервера сессии
            ip = GetLocalIPAddress();
            endPoint = new IPEndPoint(ip, PORT);
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
        public static IPAddress GetLocalIPAddress()
        {
            return Dns.GetHostAddresses("").FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
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
                /*Task task = Task.Run*/
                new Thread(() =>
                {
                    Debug.WriteLine("\nNew WSHandler " + (++TESTCounter));
                    ClientHandlers.Add(new WebSocketHandler(clientSocket, this));
                }).Start();
            }
        }

        static object locker = new object();
        private int countOfCalls = 0;
        public void FinalizeSession()
        {//вызывается из 2х обработчиков веб сокетов по очереди, но срабатывает 1 раз
            lock (locker)
            {
                if (countOfCalls != 0)
                    return;
                countOfCalls++;

                var sessionToDelete = SessionManager.Sessions.Find(x => x.SessionId == this.SessionInfo.SessionId);
                SessionManager.Sessions.Remove(sessionToDelete);
                foreach (var e in sessionToDelete.Players)
                    e.PlayerState = PlayerStates.Disconnected;
                foreach (var e in ClientHandlers)
                {
                    e.ClientSocket.Close();
                    e.Stream.Close();
                }
                Debug.WriteLine(">>Sessions count: " + SessionManager.Sessions.Count + "<<");
            }
        }
    }
}