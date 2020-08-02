using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace BrowserChess.GameSessionSignalR
{
    public class GameHub : Hub
    {
        public SessionManager SM = new SessionManager();

        public void Send(GameInfo gameInfo, int sessionId)
        {
            var curSession = SM.FindSessionById(sessionId);
            var id = Context.ConnectionId;

            if (curSession == null) return;
            Player curPlayer = curSession.Players.FirstOrDefault(a => a.ConnectionId == id);

            if (curPlayer.Side == Side.White) gameInfo.Side = Side.White;
            else gameInfo.Side = Side.Black;
            

            if (curSession.Players.Count == 2)
            {
                gameInfo.PlayerState = curPlayer.PlayerState;
                //чья очередь ходить
                if (curPlayer.PlayerState == PlayerStates.ActiveLeading)
                    curSession.CommonDataHub = gameInfo.BoardState;
                if (curPlayer.PlayerState == PlayerStates.ActiveWaiting)
                    gameInfo.BoardState = curSession.CommonDataHub;
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

            curSession.Players.Find(a => a.PlayerNumber == playerNumber).PlayerState = PlayerStates.Loser;
            curSession.Players.Find(a => !(a.PlayerNumber == playerNumber)).PlayerState = PlayerStates.Winner;
            Clients.Client(curSession.Players.Find(a => a.PlayerNumber == playerNumber).ConnectionId).lose();
            Clients.Client(curSession.Players.Find(a => !(a.PlayerNumber == playerNumber)).ConnectionId).win();
        }

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

            Clients.Caller.onConnected(id);
        }

        public void Disconnect(int sessionId)
        {
            OnDisconnected(true);
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var session = SessionManager.Sessions.FirstOrDefault(x => 
            x.FirstPlayer.ConnectionId == Context.ConnectionId /*|| x.SecondPlayer.ConnectionId == Context.ConnectionId*/);

            if (session != null)
            {
                Clients.Clients(session.Players.Select(a => a.ConnectionId).ToList()).onDisconnect();
                SessionManager.Sessions.Remove(session);//если отключился один игрок, то вся сессия удаляется////////
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}