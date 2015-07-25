IrcClient-csharp
================


Current events:

    UserJoined - fires when a user joins
    UserLeft - fires when a user quits
    ChannelMessage - fires when a channel message is received
    PrivateMessage - fires when a private message is received
    NoticeMessage - fires when a notice is received
    ServerMessage - fires when no other event is fired
    UpdateUserList - fires when the IRC numeric 353 is received (user list)
    NickTaken - fires if the client's nick is in use already
    Disconnected - fires when the client is disconnected (currently disabled)
    OnConnected- fires when the client is connected (when you are able to join a channel)
    NickChanged- fires if someone changes their nick
    ExceptionThrown- fires when an exception is thrown on the underlying thread
    ChannelModeSet - fires when mode is set on the channel
    
Example usage:

    // new instance
    client = new IrcClient("server", 6667);
    
    // connect to the server
    client.Connect();
    
    // join channel
    client.JoinChannel("#channel");
    
    //send message
    client.SendMessage("#channel","Message");
    
    //send notice
    client.SendNotice("user","Message");
    
    //listen for channel messages
    irc.ChannelMessage += (o,e) =>
    {
        rtbOutput.AppendText(e.User + ":\t" + e.Message + "\n");
        rtbOutput.ScrollToCaret();
    };
