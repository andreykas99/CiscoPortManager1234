using System;
using System.Windows.Forms;
using Renci.SshNet;

namespace CiscoPortManager
{
    public class MainForm : Form
    {
        private TextBox txtIP, txtUsername, txtPassword, txtCommand, txtOutput;
        private Button btnConnect, btnSend, btnVlan, btnPortStatus, btnFindMac;
        private TextBox txtInterface, txtVlan, txtMac;
        private SshClient sshClient;

        public MainForm()
        {
            this.Text = "Cisco Port Manager";
            this.Width = 800;
            this.Height = 700;

            Label lblIP = new Label { Text = "IP:", Left = 10, Top = 10 };
            txtIP = new TextBox { Left = 50, Top = 10, Width = 120 };

            Label lblUser = new Label { Text = "User:", Left = 180, Top = 10 };
            txtUsername = new TextBox { Left = 230, Top = 10, Width = 100 };

            Label lblPass = new Label { Text = "Pass:", Left = 340, Top = 10 };
            txtPassword = new TextBox { Left = 390, Top = 10, Width = 100, UseSystemPasswordChar = true };

            btnConnect = new Button { Text = "Connect", Left = 500, Top = 10 };
            btnConnect.Click += BtnConnect_Click;

            // Command field
            txtCommand = new TextBox { Left = 10, Top = 40, Width = 600 };
            btnSend = new Button { Text = "Send", Left = 620, Top = 40 };
            btnSend.Click += BtnSend_Click;

            // Interface and VLAN controls
            Label lblInterface = new Label { Text = "Interface:", Left = 10, Top = 80 };
            txtInterface = new TextBox { Left = 80, Top = 80, Width = 120 };
            Label lblVlan = new Label { Text = "VLAN:", Left = 210, Top = 80 };
            txtVlan = new TextBox { Left = 260, Top = 80, Width = 60 };
            btnVlan = new Button { Text = "Set VLAN", Left = 330, Top = 78 };
            btnVlan.Click += BtnVlan_Click;

            // Port status button
            btnPortStatus = new Button { Text = "Port Status", Left = 420, Top = 78 };
            btnPortStatus.Click += BtnPortStatus_Click;

            // MAC search controls
            Label lblMac = new Label { Text = "MAC:", Left = 10, Top = 110 };
            txtMac = new TextBox { Left = 50, Top = 110, Width = 150 };
            btnFindMac = new Button { Text = "Find MAC", Left = 210, Top = 108 };
            btnFindMac.Click += BtnFindMac_Click;

            txtOutput = new TextBox { Left = 10, Top = 150, Width = 760, Height = 480, Multiline = true, ScrollBars = ScrollBars.Both };

            this.Controls.AddRange(new Control[] {
                lblIP, txtIP, lblUser, txtUsername, lblPass, txtPassword, btnConnect,
                txtCommand, btnSend,
                lblInterface, txtInterface, lblVlan, txtVlan, btnVlan, btnPortStatus,
                lblMac, txtMac, btnFindMac,
                txtOutput
            });
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                sshClient = new SshClient(txtIP.Text, txtUsername.Text, txtPassword.Text);
                sshClient.Connect();
                txtOutput.Text = "Connected successfully.";
            }
            catch (Exception ex)
            {
                txtOutput.Text = "Connection failed: " + ex.Message;
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                try
                {
                    var result = sshClient.RunCommand(txtCommand.Text);
                    txtOutput.Text = result.Result;
                }
                catch (Exception ex)
                {
                    txtOutput.Text = "Command failed: " + ex.Message;
                }
            }
            else
            {
                txtOutput.Text = "Not connected.";
            }
        }

        private void BtnVlan_Click(object sender, EventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                string iface = txtInterface.Text;
                string vlan = txtVlan.Text;
                string cmd = $"configure terminal
interface {iface}
switchport access vlan {vlan}
end
write memory
";
                ExecuteMultilineCommand(cmd);
            }
            else
            {
                txtOutput.Text = "Not connected.";
            }
        }

        private void BtnPortStatus_Click(object sender, EventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                string iface = txtInterface.Text;
                string cmd = $"show interface {iface} status";
                var result = sshClient.RunCommand(cmd);
                txtOutput.Text = result.Result;
            }
            else
            {
                txtOutput.Text = "Not connected.";
            }
        }

        private void BtnFindMac_Click(object sender, EventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                string mac = txtMac.Text;
                string cmd = $"show mac address-table address {mac}";
                var result = sshClient.RunCommand(cmd);
                txtOutput.Text = result.Result;
            }
            else
            {
                txtOutput.Text = "Not connected.";
            }
        }

        private void ExecuteMultilineCommand(string commands)
        {
            var shell = sshClient.CreateShellStream("shell", 80, 24, 800, 600, 1024);
            shell.WriteLine(commands);
            System.Threading.Thread.Sleep(1000);
            string output = shell.Read();
            txtOutput.Text = output;
        }
    }
}
