using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BrowserChess.GameSession;

namespace BrowserChess.Controllers
{
    public class GameController : Controller
    {
        public ActionResult MainMenu()
        {
            return View();
        }

        //=======================================================================================

        public ActionResult GameSessionWebSocket()
        {
            int sessionId;
            int playerNumber;
            int PORT;
            string ip;

            var notActiveSession = SessionManager.Sessions.FirstOrDefault(a => !(a.IsGameActive));
            if (notActiveSession != null)
            {
                var newPlayer = new Player();
                newPlayer.PlayerSession = notActiveSession;
                newPlayer.PlayerNumber = playerNumber = 2;
                notActiveSession.Players.Add(newPlayer);

                sessionId = notActiveSession.SessionId;
                PORT = notActiveSession.SessionServer.PORT;
                ip = notActiveSession.SessionServer.ip.ToString();
            }
            else
            {
                var newSession = new SessionInfo();
                SessionManager.Sessions.Add(newSession);

                newSession.SessionServer = new GameSessionServer(newSession);
                PORT = newSession.SessionServer.PORT;
                ip = newSession.SessionServer.ip.ToString();

                var newPlayer = new Player();
                newPlayer.PlayerSession = newSession;
                newPlayer.PlayerNumber = playerNumber = 1;
                newSession.Players.Add(newPlayer);

                newSession.SessionId = sessionId = BrowserChess.GameSessionSignalR.SessionManager.Sessions.Count - 1;////////на guid

                Task.Run(() => newSession.SessionServer.StartServer());
            }

            ViewBag.sessionId = sessionId;
            ViewBag.playerNumber = playerNumber;
            ViewBag.PORT = PORT;
            ViewBag.Ip = ip;

            return View();
        }

        SessionManager SM = new SessionManager();
        [HttpPost]
        public string Continue(int sessionId)
        {
            var curSession = SM.FindSessionById(sessionId);

            var temp = curSession.Players[0].PlayerState;
            curSession.Players[0].PlayerState = curSession.Players[1].PlayerState;
            curSession.Players[1].PlayerState = temp;

            return "";
        }
        
        [HttpPost]
        public string Surrender(int sessionId, int playerNumber)
        {
            var curSession = SM.FindSessionById(sessionId);

            curSession.Players.Find(a => a.PlayerNumber == playerNumber).PlayerState = PlayerStates.Loser;
            curSession.Players.Find(a => !(a.PlayerNumber == playerNumber)).PlayerState = PlayerStates.Winner;

            return "";
        }

        //=======================================================================================

        public ActionResult GameSessionSignalR()
        {
            int sessionId;
            int playerNumber;

            var notActiveSession = BrowserChess.GameSessionSignalR.SessionManager.Sessions.FirstOrDefault(a => !(a.IsGameActive));
            if (notActiveSession != null)
            {
                var newPlayer = new GameSessionSignalR.Player();
                newPlayer.PlayerSession = notActiveSession;
                newPlayer.PlayerNumber = playerNumber = 2;
                notActiveSession.Players.Add(newPlayer);

                sessionId = notActiveSession.SessionId;
            }
            else
            {
                var newSession = new GameSessionSignalR.SessionInfo();
                BrowserChess.GameSessionSignalR.SessionManager.Sessions.Add(newSession);

                var newPlayer = new GameSessionSignalR.Player();
                newPlayer.PlayerSession = newSession;
                newPlayer.PlayerNumber = playerNumber = 1;
                newSession.Players.Add(newPlayer);

                newSession.SessionId = sessionId = BrowserChess.GameSessionSignalR.SessionManager.Sessions.Count - 1;////////на guid
            }

            ViewBag.sessionId = sessionId;
            ViewBag.playerNumber = playerNumber;
            return View();
        }
    }
}
//Концепт разрыва соединения/окончания сессии построен на факте того, что под каждого клиента создается единственный TCP сокет
//для рукопожатия по http и обмена фреймами по websocket