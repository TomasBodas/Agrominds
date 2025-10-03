using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SessionManager
    {
        public User User { get; set; }
        private static SessionManager session;
        private static object _lock = new object();
        public static void login(User pUser)
        {
            lock (_lock)
            {
                if (session == null)
                {
                    session = new SessionManager();
                    session.User = pUser;
                }
            }
        }
        public static void logout()
        {
            lock (_lock)
            {
                if (session != null)
                {
                    session = null;
                }
            }
        }
        public static SessionManager GetInstance
        {
            get
            {
                return session;
            }
        }
        public static bool IsLogged()
        {
            return session != null;
        }
    }
}
