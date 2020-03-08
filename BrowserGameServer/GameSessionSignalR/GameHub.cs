using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace BrowserGameServer.GameSessionSignalR
{
    public class GameHub : Hub
    {
        //static List<Player> Players = new List<Player>();
        //static string CommonDataHub = "";

        public SessionManager SM = new SessionManager();

        // Отправка сообщений
        public void Send(GameInfo gameInfo, int sessionId)
        {
            var curSession = SM.FindSessionById(sessionId);
            var id = Context.ConnectionId;
            var curPlayer = curSession.Players.FirstOrDefault(a => a.ConnectionId == id);

            if (curPlayer.Side == Side.White) gameInfo.Side = Side.White;
            else gameInfo.Side = Side.Black;

            if (curSession.Players.Count == 2)
            {
                gameInfo.PlayerState = curPlayer.PlayerState;
                //чья очередь ходить
                if (curPlayer.PlayerState == PlayerStates.ActiveLeading)
                    curSession.CommonDataHub = gameInfo.TableState;
                if (curPlayer.PlayerState == PlayerStates.ActiveWaiting)
                    gameInfo.TableState = curSession.CommonDataHub;
            }
            else
                gameInfo.PlayerState = PlayerStates.WaitBegining;


            Clients.Caller.sendGameState(gameInfo);
        }

        public void SwapActiveStates(int sessionId)
        {
            var curSession = SM.FindSessionById(sessionId);

            var temp = curSession.Players[0].PlayerState;
            curSession.Players[0].PlayerState = curSession.Players[1].PlayerState;
            curSession.Players[1].PlayerState = temp;
        }

        public void Surrender(int sessionId, int playerNumber)
        {
            var curSession = SM.FindSessionById(sessionId);

            var id = Context.ConnectionId;
            var surPlayer = curSession.Players.Find(a => a.ConnectionId == id);

            Clients.Client(curSession.Players.Find(a => a.PlayerNumber == playerNumber).ConnectionId).lose();
            Clients.Client(curSession.Players.Find(a => !(a.PlayerNumber == playerNumber)).ConnectionId).win();
        }

        // Подключение нового пользователя
        public void Connect(int sessionId, int playerNumber)
        {
            var curSession = SM.FindSessionById(sessionId);
            var id = Context.ConnectionId;
            var newPlayer = curSession.Players.Find(a => a.PlayerNumber == playerNumber);
            newPlayer.ConnectionId = id;

            //если один игрок уже есть в сессии, то цвет и состояние второго подбирается на основе первого.
            if (curSession.Players.Count == 2)
            {
                if (curSession.Players.FirstOrDefault().Side == Side.White)
                    newPlayer.Side = Side.Black;
                else
                    newPlayer.Side = Side.White;
                if (curSession.Players.FirstOrDefault().PlayerState == PlayerStates.ActiveLeading)
                    newPlayer.PlayerState = PlayerStates.ActiveWaiting;
                else
                    newPlayer.PlayerState = PlayerStates.ActiveLeading;
            }
            else if (curSession.Players.Count == 1)
            {
                newPlayer.Side = Side.White;
                newPlayer.PlayerState = PlayerStates.ActiveLeading;
            }

            // Посылаем сообщение текущему пользователю
            Clients.Caller.onConnected(id);
        }

        public void Disconnect(int sessionId)
        {
            OnDisconnected(true);
        }

        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var session = SessionManager.Sessions.FirstOrDefault(x => 
            x.FirstPlayer.ConnectionId == Context.ConnectionId || 
            x.SecondPlayer.ConnectionId == Context.ConnectionId);

            if (session != null)
            {
                Clients.Clients(session.Players.Select(a => a.ConnectionId).ToList()).onDisconnect();
                SessionManager.Sessions.Remove(session);//если отключился один игрок, то вся сессия удаляется////////
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}