using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace BrowserGameServer.GameSessionSignalR
{
    public class GameHub : Hub
    {
        static List<Player> Players = new List<Player>();
        static string CommonDataHub = "";

        // Отправка сообщений
        public void Send(GameInfo gameInfo)
        {
            var id = Context.ConnectionId;
            var curPlayer = Players.FirstOrDefault(a => a.ConnectionId == id);

            if (curPlayer.Side == Side.White) gameInfo.Side = Side.White;
            else gameInfo.Side = Side.Black;

            if (Players.Count == 2)
            {
                gameInfo.PlayerState = curPlayer.PlayerState;
                //чья очередь ходить
                if (curPlayer.PlayerState == PlayerStates.ActiveLeading)
                    CommonDataHub = gameInfo.TableState;
                if (curPlayer.PlayerState == PlayerStates.ActiveWaiting)
                    gameInfo.TableState = CommonDataHub;
            }
            else
                gameInfo.PlayerState = PlayerStates.WaitBegining;


            Clients.Caller.sendGameState(gameInfo);
        }

        public void SwapActiveStates()
        {
            var temp = Players[0].PlayerState;
            Players[0].PlayerState = Players[1].PlayerState;
            Players[1].PlayerState = temp;
        }

        public void Surrender()
        {
            var id = Context.ConnectionId;
            var surPlayer = Players.Find(a => a.ConnectionId == id);

            Clients.Caller.lose();
            Clients.AllExcept(id).win();
            Clients.All.disconnect();
        }

        // Подключение нового пользователя
        public void Connect()
        {
            var id = Context.ConnectionId;

            if (!Players.Any(x => x.ConnectionId == id))
            {
                var newPlayer = new Player { ConnectionId = id};

                if (Players.Count == 1)
                {
                    if (Players.FirstOrDefault().Side == Side.White)
                        newPlayer.Side = Side.Black;
                    else
                        newPlayer.Side = Side.White;
                    if(Players.FirstOrDefault().PlayerState == PlayerStates.ActiveLeading)
                        newPlayer.PlayerState = PlayerStates.ActiveWaiting;
                    else
                        newPlayer.PlayerState = PlayerStates.ActiveLeading;
                }
                else if(Players.Count == 0)
                {
                    newPlayer.Side = Side.White;
                    newPlayer.PlayerState = PlayerStates.ActiveLeading;
                }
                Players.Add(newPlayer);

                // Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(id);
            }
        }

        public void Disconnect()
        {
            OnDisconnected(true);
        }

        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = Players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                Players.Remove(item);
                var id = Context.ConnectionId;
                Clients.AllExcept(id).onUserDisconnected();
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}