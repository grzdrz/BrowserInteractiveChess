using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BrowserGameServer.GameSession;

namespace BrowserGameServer.Controllers
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
            //if(!HttpContext.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
            //    return Redirect("/Account/Login"); 
            #region"onDelete"
            ////Логин текущего клиента
            //var currentClientLogin = HttpContext.GetOwinContext().Authentication.User.Identity.Name; 
            //var currentClientIPAddress = HttpContext.Request.UserHostAddress;
            //if (currentClientIPAddress == "::1")
            //{
            //    currentClientIPAddress = MvcApplication.GetLocalIPAddress();
            //}

            //GameSessionServer freeSession = null;
            //foreach (var e in MvcApplication.ActiveGameSessions)
            //{
            //    if (e.FirstPlayer != null)
            //        if (e.FirstPlayer.PlayerAddress == currentClientIPAddress)
            //        {
            //            e.FinalizeSession();
            //            break;
            //        }
            //    if(e.SecondPlayer != null)
            //        if (e.SecondPlayer.PlayerAddress == currentClientIPAddress)
            //        {
            //            e.FinalizeSession();
            //            break;
            //        }
            //}

            ////находим свободную сессию, где уже сидит в режиме ожидания один игрок
            //foreach (var ee in MvcApplication.ActiveGameSessions)
            //{
            //    if (ee.PlayersCount < 2)
            //    {
            //        freeSession = ee;
            //        break;
            //    }
            //}

            //if (freeSession is null)
            //{
            //    GameSessionServer newSession = new GameSessionServer();
            //    ViewBag.PORT = newSession.PORT;
            //    ViewBag.Ip = newSession.ip.ToString();

            //    MvcApplication.ActiveGameSessions.Add(newSession);

            //    var player1 = new Player();
            //    player1.Side = Side.White;
            //    player1.PlayerAddress = currentClientIPAddress;
            //    player1.PlayerLogin = currentClientLogin;
            //    newSession.Players.Add(1, player1);

            //    Task.Run(() =>
            //    {
            //        newSession.StartServer();
            //    });
            //}
            //else
            //{
            //    var player2 = new Player();
            //    player2.Side = Side.Black;
            //    player2.PlayerAddress = currentClientIPAddress;
            //    player2.PlayerLogin = currentClientLogin;

            //    freeSession.Players.Add(2, player2);

            //    ViewBag.PORT = freeSession.PORT;
            //    ViewBag.Ip = freeSession.ip;
            //}
            #endregion

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

                newSession.SessionId = sessionId = BrowserGameServer.GameSessionSignalR.SessionManager.Sessions.Count - 1;////////на guid

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

            var notActiveSession = BrowserGameServer.GameSessionSignalR.SessionManager.Sessions.FirstOrDefault(a => !(a.IsGameActive));
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
                BrowserGameServer.GameSessionSignalR.SessionManager.Sessions.Add(newSession);

                var newPlayer = new GameSessionSignalR.Player();
                newPlayer.PlayerSession = newSession;
                newPlayer.PlayerNumber = playerNumber = 1;
                newSession.Players.Add(newPlayer);

                newSession.SessionId = sessionId = BrowserGameServer.GameSessionSignalR.SessionManager.Sessions.Count - 1;////////на guid
            }

            ViewBag.sessionId = sessionId;
            ViewBag.playerNumber = playerNumber;
            return View();
        }
    }
}
//Концепт разрыва соединения/окончания сессии построен на факте того, что под каждого клиента создается единственный TCP сокет
//для рукопожатия по http и обмена фреймами по websocket