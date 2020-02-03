﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public ActionResult GameSession()
        {
            if(!HttpContext.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
                return Redirect("/Account/Login"); ;
            //Логин текущего клиента
            var currentClientLogin = HttpContext.GetOwinContext().Authentication.User.Identity.Name; 
            var currentClientIPAddress = HttpContext.Request.UserHostAddress;
            if (currentClientIPAddress == "::1")
            {
                currentClientIPAddress = MvcApplication.GetLocalIPAddress();
            }

            GameSessionServer freeSession = null;//////////
            foreach (var e in MvcApplication.ActiveGameSessions)
            {
                if (e.FirstPlayer != null)
                    if (e.FirstPlayer.PlayerAddress == currentClientIPAddress)
                    {
                        e.FinalizeSession();
                        break;
                    }
                if(e.SecondPlayer != null)
                    if (e.SecondPlayer.PlayerAddress == currentClientIPAddress)
                    {
                        e.FinalizeSession();
                        break;
                    }
            }
            foreach (var e in MvcApplication.ActiveGameSessions)
            {
                if (e.PlayersCount < 2)
                {
                    freeSession = e;
                    break;
                }
            }

            if (freeSession is null)
            {
                GameSessionServer newSession = new GameSessionServer();
                ViewBag.PORT = newSession.PORT;
                ViewBag.Ip = newSession.ip.ToString();

                MvcApplication.ActiveGameSessions.Add(newSession);

                var player1 = new Player();
                player1.Side = Side.White;
                player1.PlayerAddress = currentClientIPAddress;
                player1.PlayerLogin = currentClientLogin;
                newSession.Players.Add(1, player1);

                Task.Run(() =>
                {
                    newSession.StartServer();
                });
            }
            else
            {
                var player2 = new Player();
                player2.Side = Side.Black;
                player2.PlayerAddress = currentClientIPAddress;
                player2.PlayerLogin = currentClientLogin;

                freeSession.Players.Add(2, player2);

                ViewBag.PORT = freeSession.PORT;
                ViewBag.Ip = freeSession.ip;
            }

            return View();
        }

        //!!!если не закрыта сессия, то у игроков которые были в ней после перезагрузки страницы с игрой 
        //не сработает корректно этот контроллер!!!!!!!
        public string NextMove()
        {
            var currentClientIPAddress = HttpContext.Request.UserHostAddress;
            if (currentClientIPAddress == "::1")
            {
                currentClientIPAddress = MvcApplication.GetLocalIPAddress();
            }

            var tempSession = MvcApplication.ActiveGameSessions.FirstOrDefault(a =>
            a.Players.FirstOrDefault(b => b.Value.PlayerAddress == currentClientIPAddress).Value != null);
            bool result = false;
            if (tempSession != null)
            {
                result = tempSession.SwapActiveState();
            }

            if (result)return "ok";////
            else return "notok";////
        }

        public string Surrender()
        {
            var currentClientIPAddress = HttpContext.Request.UserHostAddress;
            if (currentClientIPAddress == "::1")
            {
                currentClientIPAddress = MvcApplication.GetLocalIPAddress();
            }

            var tempSession = MvcApplication.ActiveGameSessions.FirstOrDefault(a =>
            a.Players.FirstOrDefault(b => b.Value.PlayerAddress == currentClientIPAddress).Value != null);
            
            if (tempSession != null)
            {
                if (tempSession.FirstPlayer.PlayerAddress == currentClientIPAddress)
                {
                    tempSession.FirstPlayer.PlayerState = PlayerStates.Loser;
                    tempSession.SecondPlayer.PlayerState = PlayerStates.Winner;
                }
                else
                {
                    tempSession.FirstPlayer.PlayerState = PlayerStates.Winner;
                    tempSession.SecondPlayer.PlayerState = PlayerStates.Loser;
                }
            }

            return "";
        }

    }
}
//Концепт разрыва соединения/окончания сессии построен на факте того, что под каждого клиента создается единственный TCP сокет
//для рукопожатия по http и обмена фреймами по websocket