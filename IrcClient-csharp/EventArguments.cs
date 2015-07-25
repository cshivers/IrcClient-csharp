using System;

namespace TechLifeForum
{
    public class UpdateUsersEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string[] UserList { get; internal set; }

        public UpdateUsersEventArgs(string channel, string[] userList)
        {
            this.Channel = channel;
            this.UserList = userList;
        }
    }

    public class UserJoinedEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string User { get; internal set; }

        public UserJoinedEventArgs(string channel, string user)
        {
            this.Channel = channel;
            this.User = user;
        }
    }

    public class UserLeftEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string User { get; internal set; }

        public UserLeftEventArgs(string channel, string user)
        {
            this.Channel = channel;
            this.User = user;
        }
    }

    public class ChannelMessageEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public ChannelMessageEventArgs(string channel, string from, string message)
        {
            this.Channel = channel;
            this.From = from;
            this.Message = message;
        }
    }

    public class NoticeMessageEventArgs : EventArgs
    {
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public NoticeMessageEventArgs(string from, string message)
        {
            this.From = from;
            this.Message = message;
        }
    }

    public class PrivateMessageEventArgs : EventArgs
    {
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public PrivateMessageEventArgs(string from, string message)
        {
            this.From = from;
            this.Message = message;
        }
    }

    public class UserNickChangedEventArgs : EventArgs
    {
        public string Old { get; internal set; }
        public string New { get; internal set; }

        public UserNickChangedEventArgs(string oldNick, string newNick)
        {
            this.Old = oldNick;
            this.New = newNick;
        }
    }

    public class StringEventArgs : EventArgs
    {
        public string Result { get; internal set; }

        public StringEventArgs(string s)
        {
            Result = s;
        }

        public override string ToString()
        {
            return Result;
        }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; internal set; }

        public ExceptionEventArgs(Exception x)
        {
            Exception = x;
        }

        public override string ToString()
        {
            return Exception.ToString();
        }
    }

    public class ModeSetEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string From { get; internal set; }
        public string To { get; internal set; }
        public string Mode { get; internal set; }

        public ModeSetEventArgs(string channel, string from, string to, string mode)
        {
            this.Channel = channel;
            this.From = from;
            this.To = to;
            this.Mode = mode;
        }
    }
}