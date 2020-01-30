using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Data.Entity;
using System.Security.Cryptography;

namespace BrowserGameServer.GameSession
{
    public enum ClientStatus
    {
        Visitor,
        User
    };

    //сущность клиента в б/д текущих клиентов(клиенты-сокеты полученные при подключении к серверу)
    public class ClientProfile
    {
        //ограничение на количество одновременно поддерживающихся запросов(socket+nstream)
        public LinkedList<Controller2> clientControllers = new LinkedList<Controller2>();

        public Socket currentClientSocket;
        public string ClientCookie;
        public string ClientLogin;
        public ClientStatus clientStatus;

        //public Context db;

        public ClientProfile(Socket currentClientSocket)
        {
            this.currentClientSocket = currentClientSocket;
            clientStatus = ClientStatus.Visitor;
            //db = new Context();
            //db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
        }

        ////проверка на наличие 2х одинаковых паролей во 2й и 3й строках
        ////и корректного логина
        //public bool AccountVerification1(string nameAndPasswords)
        //{
        //    string PasswordPattern = "((Password=)([a-z]|[0-9])+)(&)\\1";
        //    string LoginPattern = "(Name=)([a-z]|[0-9])+";
        //    Regex regex1 = new Regex(PasswordPattern);
        //    Regex regex2 = new Regex(LoginPattern);
        //    MatchCollection matchs1 = regex1.Matches(nameAndPasswords);
        //    MatchCollection matchs2 = regex2.Matches(nameAndPasswords);
        //    return matchs1.Count != 0 && matchs2.Count != 0;
        //}

        ////проверка на наличие введенного логина в б/д клиентов
        //public bool AccountVerification2(string nameAndPasswords)
        //{
        //    string LoginPattern = "(Name=)([a-z]|[0-9])+";
        //    Regex regex = new Regex(LoginPattern);
        //    MatchCollection matchs = regex.Matches(nameAndPasswords);
        //    string login = "";
        //    foreach (Match e in matchs) login += e.Value;
        //    login = login.Split('=')[1];

        //    //запрос к б/д
        //    db = new Context();
        //    var clientLogin = db.Clients.Where(a => a.login == login).ToList();

        //    return clientLogin.Count != 0;//есть такой логин
        //}

        //public void AddAccountToDB(string nameAndPasswords)
        //{
        //    string Login = "";
        //    string Password = "";

        //    string NamePattern = "(Name=)([a-z]|[0-9])+";
        //    Regex regex = new Regex(NamePattern);
        //    MatchCollection matchs = regex.Matches(nameAndPasswords);
        //    foreach (Match e in matchs)
        //    {
        //        Login += e.Value;
        //        break;
        //    }
        //    Login = Login.Split('=')[1];

        //    string PasswordPattern = "(&Password=)([a-z]|[0-9])+(&)";
        //    regex = new Regex(PasswordPattern);
        //    matchs = regex.Matches(nameAndPasswords);
        //    foreach (Match e in matchs)
        //    {
        //        Password += e.Value;
        //        break;
        //    }
        //    Password = Password.Split('=', '&')[2];

        //    //запрос к б/д
        //    db = new Context();
        //    var newClient = new Client()
        //    {
        //        login = Login,
        //        passwordHash = GetHash(Password)
        //    };
        //    db.Clients.Attach(newClient);
        //    db.Entry(newClient).State = EntityState.Added;
        //    db.SaveChanges();
        //}

        ////проверка на наличие введенного логина и пароля в б/д клиентов
        //public bool AccountValidation(string nameAndPasswords, out string Login)
        //{
        //    string login = "";
        //    string password = "";

        //    string NamePattern = "(Name=)([a-z]|[0-9])+";
        //    Regex regex = new Regex(NamePattern);
        //    MatchCollection matchs = regex.Matches(nameAndPasswords);
        //    foreach (Match e in matchs)
        //    {
        //        login += e.Value;
        //        break;
        //    }
        //    login = login.Split('=')[1];
        //    Login = login;

        //    string PasswordPattern = "(&Password=)([a-z]|[0-9])+";
        //    regex = new Regex(PasswordPattern);
        //    matchs = regex.Matches(nameAndPasswords);
        //    foreach (Match e in matchs)
        //    {
        //        password += e.Value;
        //        break;
        //    }
        //    password = password.Split('=')[1];

        //    string passwordHash = GetHash(password);

        //    //запрос к б/д
        //    db = new Context();
        //    var clientLogin = db.Clients.Where(a => a.login == login && a.passwordHash == passwordHash).ToList();

        //    return clientLogin.Count != 0;//есть такой логин
        //}

        //public string GetHash(string input)
        //{
        //    var md5 = MD5.Create();
        //    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

        //    return Convert.ToBase64String(hash);
        //}
    }
}
