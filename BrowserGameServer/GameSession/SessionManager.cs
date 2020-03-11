﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrowserGameServer.GameSession
{
    public class SessionManager
    {
        public static List<SessionInfo> Sessions = new List<SessionInfo>();

        public SessionInfo FindSessionById(int id)
        {
            return Sessions.Find(a => a.SessionId == id);
        }
    }
}