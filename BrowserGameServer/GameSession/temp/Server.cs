using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrowserGameServer.GameSession
{
    class Server
    {
        public int PORT = 8005;
        public IPAddress ip = IPAddress.Parse("192.168.0.10");
        public IPEndPoint endPoint;

        //public static Dictionary<string, ClientProfile> activeClients = new Dictionary<string, ClientProfile>();

        int numberOfClientRequestToConnect = 0;//TEST

        //запускаем сервер
        public Server()
        {
            //подготавливаем б/д
            //Database.SetInitializer<Context>(new ContextInitializer());

            endPoint = new IPEndPoint(ip, PORT);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);
            Console.WriteLine("SERVER STARTED");
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("\n==============>NEW_CLIENT_ACCEPT_" + (++numberOfClientRequestToConnect) + "_<==============\n");
                Task task = Task.Run(() =>
                {
                    Controller2 controller = new Controller2(clientSocket, numberOfClientRequestToConnect);
                });
            }

        }
    }
}

//После отключения клиента(клиентское приложение пользователя закрылось или чтото типо того)
//клиент переподключается к серверу и проходит проверку IP адреса через сокет на наличие таких же IP адресов в б/д текущих клиентов
//если такой клиент уже заходил на сервер, то его сущность с сокетом останется в б/д без изменений, а нового сокета создано не будет.
//А т.к.

//Данный концепт основан на том что связь и обмен данными между клиентом и сервером происходит через экземпляры Socket и NetworkStream,
//а т.к. на стороне сервера при закрытии клиентского приложения они не уничтожаются, 
//то при перезапуске клиентского приложения и повторного создания этих экз-ов в нём обмен данными можно возобновить без пересоздания
//экз-ов на стороне сервера.

//steam.DataAvailable меняется на false при первом считывании из потока, т.е. если прислать несколько пакетов через поток, 
//то цикл while(Stream.DataAvailable) для непрерывного считывания этих пакетов за раз не поможет,
//т.к. после первой итерации DataAvailable сменит значение на false и цикл прервется.
//Для счета данных до конца нужно либо знать что из себя представляет последний пакет, чтобы определить когда прервать бесконечный цикл считываний,
//либо знать сколько всего пакетов должно быть прислано и на каждый писать streamReader.ReadLine().

//Браузер присылает запрос на подключение(создание нового клиентского сокета на сервере) при каждом новом вводе адреса в адресной строк 
//или при переходе по ссылке, и соответственно на каждый запрос нужно создавать свой Stream-поток,
//НО если браузер запросит html-страницу, на которой есть например изображение, то ПО ЭТОМУ ЖЕ Stream'у будет послан запрос на картинку
//(адрес в запросе совпадает с ссылкой в теге картинки)
