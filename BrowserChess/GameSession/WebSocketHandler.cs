﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets; 
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace BrowserChess.GameSession
{
    public class WebSocketHandler
    {
        public SessionManager SM { get; set; } = new SessionManager();
        public GameSessionServer SessionServer { get; set; }

        public Player PlayerOwner { get; set; }

        public Socket ClientSocket { get; set; }
        public NetworkStream Stream { get; set; }

        public string Request { get; set; } = "";
        public byte[] ByteRequest { get; set; } = new byte[1024];
        public byte[] ByteResponse { get; set; } = new byte[1] { 0 };
        public Dictionary<string, string[]> ParsedRequest { get; set; } = new Dictionary<string, string[]>();

        public WebSocketHandler(Socket socket, GameSessionServer server)
        {
            SM = new SessionManager();
            SessionServer = server;
            ClientSocket = socket;
            Stream = new NetworkStream(ClientSocket);

            #region "ПРИЕМ ПЕРВОГО ЗАПРОСА"
            //Предпроверка нового клиента
            //Ожидаем запрос от сокета 10 сек. Если его нет, значит сокет бу.
            DateTime timerStart = DateTime.Now;
            DateTime timerCurrent = DateTime.Now;
            TimeSpan dTime = TimeSpan.Zero;
            while (!Stream.DataAvailable)
            {
                timerCurrent = DateTime.Now;
                dTime = timerCurrent - timerStart;
                if (dTime.TotalMilliseconds > 10000)
                    return;
                Thread.Sleep(500);
            }

            int Count = 0;
            while ((Count = Stream.Read(ByteRequest, 0, ByteRequest.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                Request += Encoding.UTF8.GetString(ByteRequest, 0, Count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                if (Request.IndexOf("\r\n\r\n") >= 0)
                {
                    break;
                }
            }
            Debug.WriteLine("\nRequest: \n" + Request);
            #endregion

            //Парсинг запроса
            ParsedRequest = ParseHttpRequest(Request);

            //Рукопожатие и начало обработки web socket запросов
            if (ParsedRequest.ContainsKey("Upgrade"))
            {
                if (ParsedRequest["Upgrade"][0] == "websocket")
                {
                    Handshake(ParsedRequest["Sec-WebSocket-Key"][0]);
                    ProcessWebSocketQuery();
                }
            }         
        }

        public Dictionary<string, string> ParsedWSRequest = new Dictionary<string, string>();
        private void ProcessWebSocketQuery()
        {
            while (true)
            {
                Request = "";
                ByteRequest = new byte[1024];
                #region "Прием запросов"
                //Ожидает 5 секунд запрос от клиента
                DateTime timerStart = DateTime.Now;
                DateTime timerCurrent = DateTime.Now;
                TimeSpan dTime = TimeSpan.Zero;
                while (!Stream.DataAvailable)
                {
                    timerCurrent = DateTime.Now;
                    dTime = timerCurrent - timerStart;
                    if (dTime.TotalMilliseconds > 5000)
                    {
                        SessionServer.FinalizeSession();
                        if (PlayerOwner is null) Debug.WriteLine(">> UNKNOWN_PLAYER close socket and stream<<");
                        else Debug.WriteLine(">>" + PlayerOwner.PlayerNumber + " close socket and stream<<");
                        return;
                    }
                }

                Stream.Read(ByteRequest, 0, ByteRequest.Length);
                Request = DecodeWebSocketMessage(ByteRequest);
                //по веб сокету приходят строки с '\r\n\r\n' в конце
                //отрезаем мусорную часть строки
                Request = Request.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                #endregion


                #region "Парсинг и обработка запросов"
                ParsedWSRequest = ParseWebSocketRequest(Request);

                //первый коннект-запрос
                if (ParsedWSRequest.ContainsKey("IsConnectedQuery"))
                {
                    int sessionId = int.Parse(ParsedWSRequest["SessionId"]);
                    int playerId = int.Parse(ParsedWSRequest["PlayerNumber"]);
                    //устанавливаем игрока и его цвет
                    FirstQueryToConnect(sessionId, playerId);
                    ParsedWSRequest.Remove("IsConnectedQuery");
                }
                if (PlayerOwner.PlayerState == PlayerStates.Disconnected) break;

                if (SessionServer.SessionInfo.Players.Count == 2)
                {
                    ParsedWSRequest["PlayerState"] = PlayerOwner.PlayerState.ToString();
                    //чья очередь ходить
                    if (PlayerOwner.PlayerState == PlayerStates.ActiveLeading)
                        SessionServer.SessionInfo.CommonDataHub = ParsedWSRequest["BoardState"];
                    if (PlayerOwner.PlayerState == PlayerStates.ActiveWaiting)
                        ParsedWSRequest["BoardState"] = SessionServer.SessionInfo.CommonDataHub;
                }
                else
                    ParsedWSRequest["PlayerState"] = PlayerStates.WaitBegining.ToString();
                #endregion


                #region "Отправка ответов"
                ByteResponse = EncodeWebSocketMessage(BuildWebSocketResponse(ParsedWSRequest));
                Stream.Write(ByteResponse, 0, ByteResponse.Length);
                Stream.Flush();
                #endregion
            }
        }

        #region "WebSocketSupportCode"
        private string DecodeWebSocketMessage(byte[] bytes)
        {
            try
            {
                string incomingData = "";
                byte secondByte = bytes[1];

                int dataLength = secondByte & 127;
                int indexFirstMask = 2;

                if (dataLength == 126) indexFirstMask = 4;
                else if (dataLength == 127) indexFirstMask = 10;

                IEnumerable<byte> keys = bytes.Skip(indexFirstMask).Take(4);
                int indexFirstDataByte = indexFirstMask + 4;

                byte[] decoded = new byte[bytes.Length - indexFirstDataByte];
                for (int i = indexFirstDataByte, j = 0; i < bytes.Length; i++, j++)
                {
                    decoded[j] = (byte)(bytes[i] ^ keys.ElementAt(j % 4));
                }

                return incomingData = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not decode due to :" + ex.Message);
            }
            return null;
        }

        private byte[] EncodeWebSocketMessage(string message)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.UTF8.GetBytes(message);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)129;
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        private void Handshake(string keyHash)
        {
            string newKeyHash = ComputeWebSocketHandshakeSecurityHash(keyHash);

            string Response = "HTTP/1.1 101 Switching Protocols\r\n" +
                              "Upgrade: websocket\r\n" +
                              "Connection: Upgrade\r\n" +
                              "Sec-WebSocket-Accept: " + newKeyHash + "\r\n\r\n";
            var byteResponse = Encoding.UTF8.GetBytes(Response);

            Request = "";
            ByteRequest = new byte[1024];
            Stream.Write(byteResponse, 0, byteResponse.Length);
            Stream.Flush();
        }

        private string ComputeWebSocketHandshakeSecurityHash(string secWebSocketKey)
        {
            string MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string secWebSocketAccept = "";

            string ret = secWebSocketKey + MagicKEY;

            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));

            secWebSocketAccept = Convert.ToBase64String(sha1Hash);

            return secWebSocketAccept;
        }
        #endregion

        public void FirstQueryToConnect(int sessionId, int playerNumber)
        {
            var curSession = SM.FindSessionById(sessionId);
            PlayerOwner = curSession.Players.Find(a => a.PlayerNumber == playerNumber);

            //если один игрок уже есть в сессии, то цвет и состояние второго подбирается на основе первого.
            if (SessionServer.SessionInfo.Players.Count == 2)
            {
                if (SessionServer.SessionInfo.Players.FirstOrDefault().Side == Side.White)
                    PlayerOwner.Side = Side.Black;
                else
                    PlayerOwner.Side = Side.White;
                if (SessionServer.SessionInfo.Players.FirstOrDefault().PlayerState == PlayerStates.ActiveLeading)
                    PlayerOwner.PlayerState = PlayerStates.ActiveWaiting;
                else
                    PlayerOwner.PlayerState = PlayerStates.ActiveLeading;
            }
            else if (SessionServer.SessionInfo.Players.Count == 1)
            {
                PlayerOwner.Side = Side.White;
                PlayerOwner.PlayerState = PlayerStates.ActiveLeading;
            }
            ParsedWSRequest["Side"] = PlayerOwner.Side.ToString();
        }

        private Dictionary<string, string[]> ParseHttpRequest(string request)
        {
            string[] tempReq = request.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);//массив пакетов строк
            var dict = new Dictionary<string, string[]>();//словарь пар - заголовок: строка после заголовка
            string[] temp1 = null;
            Regex regex = new Regex("(GET )|(POST )");
            foreach (var e in tempReq)
            {
                if (regex.IsMatch(e))
                    temp1 = e.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                else
                    temp1 = e.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                dict.Add(temp1[0], temp1.Skip(1).ToArray());
            }


            if (dict.ContainsKey("Cookie"))
            {
                regex = new Regex("(cookie1=)([0-9]+)");
                dict["Cookie"] = regex.Match(dict["Cookie"][0]).Value.Split('=');
                //dict["Cookie"] = dict["Cookie"][0].Split('=');
            }

            return dict;
        }

        private Dictionary<string, string> ParseWebSocketRequest(string request)
        {
            string[] tempReq = request.Split(new string[] { "<delimiter>" }, StringSplitOptions.RemoveEmptyEntries);//массив пакетов строк
            var dict = new Dictionary<string, string>();
            string[] temp1 = null;
            foreach (var e in tempReq)
            {
                temp1 = e.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (temp1.Length < 2)
                    dict.Add(temp1[0], "");
                else
                    dict.Add(temp1[0], temp1[1]);
            }

            return dict;
        }
        private string BuildWebSocketResponse(Dictionary<string, string> subResponses)
        {
            string result = "";
            foreach (var e in subResponses)
            {
                result += e.Key + ":" + e.Value + "<delimiter>";
            }

            return result;
        }

        //Формат данных общения с клиентом:
        //Side:...<delimiter>
        //PlayerState:...<delimiter>
        //BoardState:(id, x1, y1);(id, x2, y2);...<delimiter>
        //PlayerNumber:...<delimiter>
        //SessionId:...<delimiter>
        //IsConnectedQuery:...<delimiter>(на один запрос)

        //последняя строка это id конкретной фигуры и ее относительные координаты(относительно положения доски на холсте)
        //относительные координаты нужны для синхронизации координат игроков с разными разрешениями экрана
    }
}