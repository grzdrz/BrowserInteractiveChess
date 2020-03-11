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

        public int PORT;
        public IPAddress ip;
        public IPEndPoint endPoint;
        public Socket serverSocket;

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
                Task task = Task.Run(() =>
                {
                    Debug.WriteLine("\nNew WSHandler " + (++TESTCounter));
                    new WebSocketHandler(clientSocket, this);
                });
            }
        }

        public bool SwapActiveState()
        {
            PlayerStates temp;
            if (SessionInfo.FirstPlayer.PlayerState == PlayerStates.ActiveLeading || SessionInfo.FirstPlayer.PlayerState == PlayerStates.ActiveWaiting &&
                SessionInfo.SecondPlayer.PlayerState == PlayerStates.ActiveLeading || SessionInfo.SecondPlayer.PlayerState == PlayerStates.ActiveWaiting)
            {
                temp = SessionInfo.FirstPlayer.PlayerState;
                SessionInfo.FirstPlayer.PlayerState = SessionInfo.SecondPlayer.PlayerState;
                SessionInfo.SecondPlayer.PlayerState = temp;
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
                if (SessionInfo.FirstPlayer != null)
                    SessionInfo.FirstPlayer.PlayerState = PlayerStates.Disconnected;
                if (SessionInfo.SecondPlayer != null)
                    SessionInfo.SecondPlayer.PlayerState = PlayerStates.Disconnected;

                if (countOfCalls != 0)
                    return;

                countOfCalls++;
                MvcApplication.ActiveGameSessions.Remove(this);//удаляем объект сессии из колекции сессий, что бы его мог съесть мусорщик
                this.serverSocket.Close();//закрываем серверный сокет
                //клиентские сокеты/потоки закроются сами после выставления статуса Disconnected и/или разрыва сокета со стороны клиента

                Debug.WriteLine(">>Sessions count: " + MvcApplication.ActiveGameSessions.Count + "<<");
            }
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