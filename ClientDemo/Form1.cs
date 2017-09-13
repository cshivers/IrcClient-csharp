using System;
using TechLifeForum;
using System.Windows.Forms;

namespace ClientDemo
{
    public partial class frmMain : Form
    {
        IrcClient client;
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Random r = new Random();

            for (int i = 0; i < 3; i++)
                txtNick.AppendText(r.Next(10).ToString());
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
            {
                checkSSL.Enabled = false;
                DoConnect();
            }
            else
            {
                DoDisconnect();
                checkSSL.Enabled = true;
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (client.Connected && !String.IsNullOrEmpty(txtSend.Text.Trim()))
            {
                if (txtChannel.Text.StartsWith("#"))
                    client.SendMessage(txtChannel.Text.Trim(), txtSend.Text.Trim());
                else
                    client.SendMessage("#" + txtChannel.Text.Trim(), txtSend.Text.Trim());

                AddToChatWindow("Me: " + txtSend.Text.Trim());
                txtSend.Clear();
                txtSend.Focus();
            }
        }
        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSend.PerformClick();
        }

        private void DoConnect()
        {
            if (String.IsNullOrEmpty(txtServer.Text.Trim()))
            {
                MessageBox.Show("Please specify a server");
                return;
            }
            if (String.IsNullOrEmpty(txtChannel.Text.Trim()))
            {
                MessageBox.Show("Please specify a channel");
                return;
            }
            if (String.IsNullOrEmpty(txtNick.Text.Trim()))
            {
                MessageBox.Show("Please specify a nick");
                return;
            }

            int port;
            if (Int32.TryParse(txtPort.Text, out port))
                if (checkSSL.Checked)
                {
                    client = new IrcClient(txtServer.Text.Trim(), port, true);
                }
                else {
                    client = new IrcClient(txtServer.Text.Trim(), port, false);
                }
            else
                client = new IrcClient(txtServer.Text.Trim());

            AddEvents();
            client.Nick = txtNick.Text.Trim();

            btnConnect.Enabled = false;
            txtChannel.Enabled = false;
            txtPort.Enabled = false;
            txtServer.Enabled = false;
            txtNick.Enabled = false;
            rtbOutput.Clear(); // in case they reconnect and have old stuff there


            client.Connect();
        }
        private void DoDisconnect()
        {

            btnConnect.Enabled = true;
            txtChannel.Enabled = true;
            txtPort.Enabled = true;
            txtServer.Enabled = true;
            txtNick.Enabled = true;

            lstUsers.Items.Clear();
            txtSend.Enabled = false;
            btnSend.Enabled = false;

            client.Disconnect();
            client = null;

            btnConnect.Text = "Connect";
        }
        private void AddEvents()
        {
            client.ChannelMessage += client_ChannelMessage;
            client.ExceptionThrown += client_ExceptionThrown;
            client.NoticeMessage += client_NoticeMessage;
            client.OnConnect += client_OnConnect;
            client.PrivateMessage += client_PrivateMessage;
            client.ServerMessage += client_ServerMessage;
            client.UpdateUsers += client_UpdateUsers;
            client.UserJoined += client_UserJoined;
            client.UserLeft += client_UserLeft;
            client.UserNickChange += client_UserNickChange;
        }
        private void AddToChatWindow(string message)
        {
            rtbOutput.AppendText(message + "\n");
            rtbOutput.ScrollToCaret();
        }

        #region Event Listeners

        void client_OnConnect(object sender, EventArgs e)
        {
            txtSend.Enabled = true;
            txtSend.Focus();
            btnSend.Enabled = true;

            btnConnect.Text = "Disconnect";
            btnConnect.Enabled = true;

            if (txtChannel.Text.StartsWith("#"))
                client.JoinChannel(txtChannel.Text.Trim());
            else
                client.JoinChannel("#" + txtChannel.Text.Trim());

        }

        void client_UserNickChange(object sender, UserNickChangedEventArgs e)
        {
            lstUsers.Items[lstUsers.Items.IndexOf(e.Old)] = e.New;
        }

        void client_UserLeft(object sender, UserLeftEventArgs e)
        {
            lstUsers.Items.Remove(e.User);
        }

        void client_UserJoined(object sender, UserJoinedEventArgs e)
        {
            lstUsers.Items.Add(e.User);
        }

        void client_UpdateUsers(object sender, UpdateUsersEventArgs e)
        {
            lstUsers.Items.Clear();
            lstUsers.Items.AddRange(e.UserList);
            
        }

        void client_ServerMessage(object sender, StringEventArgs e)
        {
            Console.WriteLine(e.Result);
        }

        void client_PrivateMessage(object sender, PrivateMessageEventArgs e)
        {
            AddToChatWindow("PM FROM " + e.From + ": " + e.Message);
        }

        void client_NoticeMessage(object sender,NoticeMessageEventArgs e)
        {
            AddToChatWindow("NOTICE FROM " + e.From + ": " + e.Message);
        }

        void client_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        void client_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            AddToChatWindow(e.From + ": " + e.Message);
        }

        #endregion
        #region SSL
        private void checkSSL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkSSL.Checked)
            {
                MessageBox.Show("SSL IRC Usually opperates over port 6697!", "WARNING!");
            }
        }
        #endregion
    }
}
