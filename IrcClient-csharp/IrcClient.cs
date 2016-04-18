using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace TechLifeForum
{
    /// <summary>
    /// IRC Client class written at http://tech.reboot.pro
    /// </summary>
    public class IrcClient : IDisposable
    {
        #region Variables
        // default server
        private string _server = "";

        // default port
        private int _port = 6667;

        private string _ServerPass = "";

        // default nick
        private string _nick = "Test";

        // default alternate nick
        private string _altNick = "";

        //default consoleOutput mode
        private bool _consoleOutput = true;

        // private TcpClient used to talk to the server
        private TcpClient irc;

        // private network stream used to read/write from/to
        private NetworkStream stream;

        // global variable used to read input from the client
        private string inputLine;

        // stream reader to read from the network stream
        private StreamReader reader;

        // stream writer to write to the stream
        private StreamWriter writer;

        // AsyncOperation used to handle cross-thread wonderness
        private AsyncOperation op;

        #endregion

        #region Constructors
        /// <summary>
        /// IrcClient used to connect to an IRC Server (default port: 6667)
        /// </summary>
        /// <param name="Server">IRC Server</param>
        public IrcClient(string Server) : this(Server, 6667) { }

        /// <summary>
        /// IrcClient used to connect to an IRC Server
        /// </summary>
        /// <param name="Server">IRC Server</param>
        /// <param name="Port">IRC Port (6667 if you are unsure)</param>
        public IrcClient(string Server, int Port)
        {
            op = AsyncOperationManager.CreateOperation(null);
            _server = Server;
            _port = Port;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the Server address used
        /// </summary>
        public string Server
        {
            get { return _server; }
        }
        /// <summary>
        /// Returns the Port used
        /// </summary>
        public int Port
        {
            get { return _port; }
        }
        /// <summary>
        /// Returns the password used to auth to the server
        /// </summary>
        public string ServerPass
        {
            get { return _ServerPass; }
            set { _ServerPass = value; }
        }
        /// <summary>
        /// Returns the current nick being used.
        /// </summary>
        public string Nick
        {
            get { return _nick; }
            set { _nick = value; }
        }
        /// <summary>
        /// Returns the alternate nick being used
        /// </summary>
        public string AltNick
        {
            get { return _altNick; }
            set { _altNick = value; }
        }
        /// <summary>
        /// Output RAW IRC data to console
        /// </summary>
        public bool ConsoleOutput
        {
            get { return _consoleOutput; }
            set { _consoleOutput = value; }
        }
        /// <summary>
        /// Returns true if the client is connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (irc != null)
                    if (irc.Connected)
                        return true;
                return false;
            }
        }
        #endregion

        #region Events

        public event EventHandler<UpdateUsersEventArgs> UpdateUsers = delegate { };
        public event EventHandler<UserJoinedEventArgs> UserJoined = delegate { };
        public event EventHandler<UserLeftEventArgs> UserLeft = delegate { };
        public event EventHandler<UserNickChangedEventArgs> UserNickChange = delegate { };

        public event EventHandler<ChannelMessageEventArgs> ChannelMessage = delegate { };
        public event EventHandler<NoticeMessageEventArgs> NoticeMessage = delegate { };
        public event EventHandler<PrivateMessageEventArgs> PrivateMessage = delegate { };
        public event EventHandler<StringEventArgs> ServerMessage = delegate { };

        public event EventHandler<StringEventArgs> NickTaken = delegate { };

        public event EventHandler OnConnect = delegate { };

        public event EventHandler<ExceptionEventArgs> ExceptionThrown = delegate { };

        public event EventHandler<ModeSetEventArgs> ChannelModeSet = delegate { };

        private void Fire_UpdateUsers(UpdateUsersEventArgs o)
        {
            op.Post(x => UpdateUsers(this, (UpdateUsersEventArgs)x), o);
        }
        private void Fire_UserJoined(UserJoinedEventArgs o)
        {
            op.Post(x => UserJoined(this, (UserJoinedEventArgs)x), o);

        }
        private void Fire_UserLeft(UserLeftEventArgs o)
        {
            op.Post(x => UserLeft(this, (UserLeftEventArgs)x), o);
        }
        private void Fire_NickChanged(UserNickChangedEventArgs o)
        {
            op.Post(x => UserNickChange(this, (UserNickChangedEventArgs)x), o);
        }
        private void Fire_ChannelMessage(ChannelMessageEventArgs o)
        {
            op.Post(x => ChannelMessage(this, (ChannelMessageEventArgs)x), o);
        }
        private void Fire_NoticeMessage(NoticeMessageEventArgs o)
        {
            op.Post(x => NoticeMessage(this, (NoticeMessageEventArgs)x), o);
        }
        private void Fire_PrivateMessage(PrivateMessageEventArgs o)
        {
            op.Post(x => PrivateMessage(this, (PrivateMessageEventArgs)x), o);
        }
        private void Fire_ServerMesssage(string s)
        {
            op.Post(x => ServerMessage(this, (StringEventArgs)x), new StringEventArgs(s));
        }
        private void Fire_NickTaken(string s)
        {
            op.Post(x => NickTaken(this, (StringEventArgs)x), new StringEventArgs(s));
        }
        private void Fire_Connected()
        {
            op.Post((x) => OnConnect(this, null), null);
        }
        private void Fire_ExceptionThrown(Exception ex)
        {
            op.Post(x => ExceptionThrown(this, (ExceptionEventArgs)x), new ExceptionEventArgs(ex));
        }
        private void Fire_ChannelModeSet(ModeSetEventArgs o)
        {
            op.Post(x => ChannelModeSet(this, (ModeSetEventArgs)x), o);
        }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Connect to the IRC server
        /// </summary>
        public void Connect()
        {
            Thread t = new Thread(DoConnect) { IsBackground = true };
            t.Start();
        }
        private void DoConnect()
        {
            try
            {
                irc = new TcpClient(_server, _port);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                if (!string.IsNullOrEmpty(_ServerPass))
                    Send("PASS " + _ServerPass);

                Send("NICK " + _nick);
                Send("USER " + _nick + " 0 * :" + _nick);

                Listen();
            }
            catch (Exception ex)
            {
                Fire_ExceptionThrown(ex);
            }
        }
        /// <summary>
        /// Disconnect from the IRC server
        /// </summary>
        public void Disconnect()
        {
            if (irc != null)
            {
                if (irc.Connected)
                {
                    Send("QUIT Client Disconnected: http://tech.reboot.pro");
                }
                irc = null;
            }
        }
        /// <summary>
        /// Sends the JOIN command to the server
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void JoinChannel(string channel)
        {
            if (irc != null && irc.Connected)
            {
                Send("JOIN " + channel);
            }
        }
        /// <summary>
        /// Sends the PART command for a given channel
        /// </summary>
        /// <param name="channel">Channel to leave</param>
        public void PartChannel(string channel)
        {
            Send("PART " + channel);
        }
        /// <summary>
        /// Send a notice to a user
        /// </summary>
        /// <param name="toNick">User to send the notice to</param>
        /// <param name="message">The message to send</param>
        public void SendNotice(string toNick, string message)
        {
            Send("NOTICE " + toNick + " :" + message);
        }

        /// <summary>
        /// Send a message to the channel
        /// </summary>
        /// <param name="channel">Channel to send message</param>
        /// <param name="message">Message to send</param>
        public void SendMessage(string channel, string message)
        {
            Send("PRIVMSG " + channel + " :" + message);
        }
        /// <summary>
        /// Send RAW IRC commands
        /// </summary>
        /// <param name="message"></param>
        public void SendRaw(string message)
        {
            Send(message);
        }

        public void Dispose()
        {
            stream.Dispose();
            writer.Dispose();
            reader.Dispose();
        }
        #endregion

        #region PrivateMethods
        /// <summary>
        /// Listens for messages from the server
        /// </summary>
        private void Listen()
        {

            while ((inputLine = reader.ReadLine()) != null)
            {
                try
                {
                    ParseData(inputLine);
                    if(_consoleOutput) Console.Write(inputLine);
                }
                catch (Exception ex)
                {
                    Fire_ExceptionThrown(ex);
                }
            }//end while
        }
        /// <summary>
        /// Parses data sent from the server
        /// </summary>
        /// <param name="data">message from the server</param>
        private void ParseData(string data)
        {
            // split the data into parts
            string[] ircData = data.Split(' ');

            var ircCommand = ircData[1];

            // if the message starts with PING we must PONG back
            if (data.Length > 4)
            {
                if (data.Substring(0, 4) == "PING")
                {
                    Send("PONG " + ircData[1]);
                    return;
                }

            }

            // re-act according to the IRC Commands
            switch (ircCommand)
            {
                case "001": // server welcome message, after this we can join
                    Send("MODE " + _nick + " +B");
                    Fire_Connected();    //TODO: this might not work
                    break;
                case "353": // member list
                    {
                        var channel = ircData[4];
                        var userList = JoinArray(ircData, 5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        Fire_UpdateUsers(new UpdateUsersEventArgs(channel, userList));
                    }
                    break;
                case "433":
                    var takenNick = ircData[3];

                    // notify user
                    Fire_NickTaken(takenNick);

                    // try alt nick if it's the first time 
                    if (takenNick == _altNick)
                    {
                        var rand = new Random();
                        var randomNick = "Guest" + rand.Next(0, 9) + rand.Next(0, 9) + rand.Next(0, 9);
                        Send("NICK " + randomNick);
                        Send("USER " + randomNick + " 0 * :" + randomNick);
                        _nick = randomNick;
                    }
                    else
                    {
                        Send("NICK " + _altNick);
                        Send("USER " + _altNick + " 0 * :" + _altNick);
                        _nick = _altNick;
                    }
                    break;
                case "JOIN": // someone joined
                    {
                        var channel = ircData[2];
                        var user = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                        Fire_UserJoined(new UserJoinedEventArgs(channel, user));
                    }
                    break;
                case "MODE": // MODE was set
                    {
                        var channel = ircData[2];
                        if (channel != this.Nick)
                        {
                            string from;
                            if (ircData[0].Contains("!"))
                                from = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                            else
                                from = ircData[0].Substring(1);

                            var to = ircData[4];
                            var mode = ircData[3];
                            Fire_ChannelModeSet(new ModeSetEventArgs(channel, from, to, mode));
                        }

                        // TODO: event for userMode's
                    }
                    break;
                case "NICK": // someone changed their nick
                    var oldNick = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                    var newNick = JoinArray(ircData, 3);

                    Fire_NickChanged(new UserNickChangedEventArgs(oldNick, newNick));
                    break;
                case "NOTICE": // someone sent a notice
                    {
                        var from = ircData[0];
                        var message = JoinArray(ircData, 3);
                        if (from.Contains("!"))
                        {
                            from = from.Substring(1, ircData[0].IndexOf('!') - 1);
                            Fire_NoticeMessage(new NoticeMessageEventArgs(from, message));
                        }
                        else
                            Fire_NoticeMessage(new NoticeMessageEventArgs(_server, message));
                    }
                    break;
                case "PRIVMSG": // message was sent to the channel or as private
                    {
                        var from = ircData[0].Substring(1, ircData[0].IndexOf('!') - 1);
                        var to = ircData[2];
                        var message = JoinArray(ircData, 3);

                        // if it's a private message
                        if (String.Equals(to, _nick, StringComparison.CurrentCultureIgnoreCase))
                            Fire_PrivateMessage(new PrivateMessageEventArgs(from, message));
                        else
                            Fire_ChannelMessage(new ChannelMessageEventArgs(to, from, message));
                    }
                    break;
                case "PART":
                case "QUIT":// someone left
                    {
                        var channel = ircData[2];
                        var user = ircData[0].Substring(1, data.IndexOf("!") - 1);

                        Fire_UserLeft(new UserLeftEventArgs(channel, user));
                        Send("NAMES " + ircData[2]);
                    }
                    break;
                default:
                    // still using this while debugging

                    if (ircData.Length > 3)
                        Fire_ServerMesssage(JoinArray(ircData, 3));

                    break;
            }

        }
        /// <summary>
        /// Strips the message of unnecessary characters
        /// </summary>
        /// <param name="message">Message to strip</param>
        /// <returns>Stripped message</returns>
        private static string StripMessage(string message)
        {
            // remove IRC Color Codes
            foreach (Match m in new Regex((char)3 + @"(?:\d{1,2}(?:,\d{1,2})?)?").Matches(message))
                message = message.Replace(m.Value, "");

            // if there is nothing to strip
            if (message == "")
                return "";
            else if (message.Substring(0, 1) == ":" && message.Length > 2)
                return message.Substring(1, message.Length - 1);
            else
                return message;
        }
        /// <summary>
        /// Joins the array into a string after a specific index
        /// </summary>
        /// <param name="strArray">Array of strings</param>
        /// <param name="startIndex">Starting index</param>
        /// <returns>String</returns>
        private static string JoinArray(string[] strArray, int startIndex)
        {
            return StripMessage(String.Join(" ", strArray, startIndex, strArray.Length - startIndex));
        }
        /// <summary>
        /// Send message to server
        /// </summary>
        /// <param name="message">Message to send</param>
        private void Send(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
        }
        #endregion
    }

}