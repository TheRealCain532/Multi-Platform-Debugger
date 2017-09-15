using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RTF;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Management;
using MultiLib;
using MultiPlatformDebugger.Properties;
using System.Threading;
using MultiPlatformDebugger;
using System.Net;
using System.Net.Sockets;

namespace MultiPlatformDebugger
{
    public partial class Form1 : Form
    {
        SaveFileDialog _SD = new SaveFileDialog();
        public static MultiConsoleAPI PS3 = new MultiConsoleAPI();
        public Form1()
        {
            InitializeComponent();
            Debugging();
        }
        public static Thread ConWrite = new Thread(new ThreadStart(ConstantWrite));
        private void connect_Click(object sender, EventArgs e)
        {
            switch (_Console)
            {
                case 0: Connection(SelectAPI.ControlConsole);break;
                case 1: Connection(SelectAPI.TargetManager);break;
                case 2: Connection(SelectAPI.XboxNeighborhood);break;
                case 3: Connection(SelectAPI.PCAPI);break;
            }
            connection1.ForeColor = IsConnected ? Color.LimeGreen : Color.Red;
            connection1.Text = IsConnected ? "Connected" : "Not Connected";
            debuggerStatus.Enabled = IsConnected;
            if (IsConnected)
                Debugging();
        }
        private void ps3btn_RightClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    {
                        rclickmenu.Show(this.ps3btn, new Point(e.X, e.Y));
                    }
                    break;
            }
        }
        private void ThreadDebugger_Pressed(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:

                    _tDebugger.Show(this.menuStrip1, new Point(e.X, e.Y));
                    break;
            }
        }
        private void DebuggerType(object sender, EventArgs e)
        {
            Thread_Refresher = !Thread_Refresher;
            _tDebugger.Text = string.Format("Threaded Refresher - {0}", Thread_Refresher ? "Enabled" : "Disabled");
            _tDebugger.ForeColor = Thread_Refresher ? Color.Green : Color.Red;
        }
            private void APIChanging(object sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)sender;
            PS3.DisconnectTarget();
            if (btn.Checked)
            {
                switch (btn.Text)
                {
                    case "PlayStation 3":
                        rclickmenu.Enabled = true;
                        this.Text = "CEX/PSX Debugger";
                        this.Icon = Resources.PSXIcon;
                        startHex.Text = "0x00010000";
                        break;
                    case "Xbox 360":
                        rclickmenu.Enabled = false;
                        this.Text = "Xbox Debugger";
                        this.Icon = Resources.XboxIcon;
                        startHex.Text = "0xC0010000";
                        _Console = 2;
                        break;
                    case "PC":
                        rclickmenu.Enabled = false;
                        this.Text = "PC Debugger";
                        startHex.Text = "0x00010000";
                        _Console = 3;
                        break;
                }
            }
        }
        private void x360btn_RightClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    {
                        rclickmenux.Show(this.x360btn, new Point(e.X, e.Y));
                    }
                    break;
            }
        }
        private void CEX_DEX(object sender, EventArgs e)
        {
            this.Icon = Resources.PSXIcon;
            switch (sender.ToString())
            {
                case "CEX / PSX":
                    cEX.ForeColor = Color.Blue;
                    dEX.ForeColor = Color.Red;
                    this.Text = "CEX/PSX Debugger";
                    _Console = 0;
                    break;
                case "DEX":
                    this.Text = "PlayStation 3 DEX Debugger";
                    cEX.ForeColor = Color.Red;
                    dEX.ForeColor = Color.Blue;
                    _Console = 1;
                    break;
            }
        }
        #region Variables
        public static bool 
            ValHex = true,
            NewSearch = true,
            debugger,
            IsConnected,
            Thread_Refresher;
        public static uint[] 
            MemArray,
            PID;
        public static uint 
            num3, 
            dOffset, 
            address;
        public static byte[] 
            oldBytes,
            write;
        public static ulong 
            SchResCnt = 0,
            GlobAlign = 0;
        public static string 
            dFileName = "a.txt",
            str2,
            str3;
        public static int
            cWrite = 0,
            _Console = 0,
            apiDLL = 0,
            NextSAlign = 0,
            CancelSearch = 0,
            compMode = 0;
        public const int
            MaxRes = 10000,
            MaxCodes = 1000,
            compEq = 0,
            compNEq = 1,
            compLT = 2,
            compLTE = 3,
            compGT = 4,
            compGTE = 5,
            compVBet = 6,
            compINC = 7,
            compDEC = 8,
            compChg = 9,
            compUChg = 10,
            compANEq = 20;

        public struct CodeRes
        {
            public bool state;
            public ulong addr;
            public byte[] val;
            public int align;
        };
        public struct ListRes
        {
            public string Addr;
            public string HexVal;
            public string DecVal;
            public string AlignStr;
        }
        #endregion
        #region Functions
        Boolean Connection(SelectAPI API = SelectAPI.TargetManager)
        {
            PS3.ChangeAPI(API);
            bool state = false;
            switch (API)
            {
                case SelectAPI.ControlConsole:
                    state = PS3.ConnectTarget();
                    if (state)
                    {
                        PS3.CCAPI.GetProcessList(out PID);
                        PS3.CCAPI.AttachProcess(PID[0]);
                        state = PS3.ConnectionStatus();
                    }
                    else
                        state = false;
                    break;
                case SelectAPI.TargetManager:
                    state = PS3.ConnectTarget();
                    if (state)
                        state = PS3.AttachProcess();
                    else
                        state = false;
                    break;
                case SelectAPI.XboxNeighborhood:
                    state = PS3.ConnectTarget();
                    break;
                case SelectAPI.PCAPI:
                    state = PS3.ConnectTarget();
                    break;
                default:
                    PS3.ChangeAPI(SelectAPI.TargetManager);
                    state = PS3.ConnectTarget();
                    if (state)
                        state = PS3.AttachProcess();
                    else
                        state = false;
                    break;
            }
            IsConnected = state;
            return state;
        }
        #region Debugger Functions
        public static string SpliceText(string text, int lineLength)
        {
            return Regex.Replace(text, "(.{" + lineLength + "})", "$1" + Environment.NewLine);
        }
        public void Debugging()
        {
            offsetsText.Text = null;
            byte[] bytes;
                string str = startHex.Text.Substring(startHex.Text.Length - 1, 1);
                if (!"0".Equals(str))
                    startHex.Text = startHex.Text.Remove(startHex.Text.Length - 1, 1) + "0";
                uint offset = Convert.ToUInt32(startHex.Text, 0x10);
                RTFBuilderbase builderbase = new RTFBuilder();
                builderbase.Font(RTFFont.CourierNew);
                builderbase.FontSize(22f);
                bytes = PS3.Extension.ReadBytes(offset, 800);

                for (int i = 0; i < bytes.Length; i++)
                {
                    builderbase.Font(RTFFont.CourierNew);
                    builderbase.FontSize(22f);
                    if (oldBytes != null)
                    {
                        if (bytes.Length == oldBytes.Length)
                        {
                            if (bytes[i] == oldBytes[i])
                                builderbase.Append(bytes[i].ToString("X2") + " ");
                            else
                                builderbase.ForeColor(KnownColor.LimeGreen).Append(bytes[i].ToString("X2") + " ");
                        }
                        else
                            builderbase.Append(bytes[i].ToString("X2") + " ");
                    }
                    else
                        builderbase.Append(bytes[i].ToString("X2") + " ");
                }
                oldBytes = bytes;
                string str2 = builderbase.ToString();
                hexCode.Rtf = str2;
                string str3 = SpliceText(Encoding.Default.GetString(bytes).Replace("\0", ".").Replace("\a", ".").Replace("\v", ".").Replace("\r", ".").Replace(" ", ".").Replace("\t", ".").Replace("\n", ".").Replace("\b", ".").Replace("\f", "."), 0x10);
                asciiText.Text = str3;
                uint num3 = offset;
                int num4 = (hexCode.Text.Length / 0x30) + 1;
                for (int j = 0; j < num4; j++)
                {
                    offsetsText.Text = offsetsText.Text + "0x" + Convert.ToString((long)num3, 0x10).ToUpper() + Environment.NewLine;
                    num3 += 0x10;
                }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            Handler();
        }
        private void Handler()
        {
            string str = startHex.Text.Substring(startHex.Text.Length - 1, 1);
            if (!"0".Equals(str))
                startHex.Text = startHex.Text.Remove(startHex.Text.Length - 1, 1) + "0";
            dOffset = Convert.ToUInt32(startHex.Text, 0x10);

            hexCode.Rtf = str2;

            asciiText.Text = str3;

            int num4 = (hexCode.Text.Length / 0x30) + 1;
            for (int j = 0; j < num4; j++)
            {
                offsetsText.Text = offsetsText.Text + "0x" + Convert.ToString((long)num3, 0x10).ToUpper() + Environment.NewLine;
                num3 += 0x10;
            }
        }
        private static void ConstantWrite()
        {
            bool sleep = false;
            while (true)
            {
                if (cWrite == 1)
                {
                    try
                    {
                        byte[] bytes;
                        RTFBuilderbase builderbase = new RTFBuilder();
                        builderbase.Font(RTFFont.CourierNew);
                        builderbase.FontSize(22f);
                        bytes = PS3.Extension.ReadBytes(dOffset, 800);

                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builderbase.Font(RTFFont.CourierNew);
                            builderbase.FontSize(22f);
                            if (oldBytes != null)
                            {
                                if (bytes.Length == oldBytes.Length)
                                {
                                    if (bytes[i] == oldBytes[i])
                                        builderbase.Append(bytes[i].ToString("X2") + " ");
                                    else
                                        builderbase.ForeColor(KnownColor.LimeGreen).Append(bytes[i].ToString("X2") + " ");
                                }
                                else
                                    builderbase.Append(bytes[i].ToString("X2") + " ");
                            }
                            else
                                builderbase.Append(bytes[i].ToString("X2") + " ");
                        }
                        oldBytes = bytes;

                        str2 = builderbase.ToString();
                        str3 = SpliceText(Encoding.Default.GetString(bytes).Replace("\0", ".").Replace("\a", ".").Replace("\v", ".").Replace("\r", ".").Replace(" ", ".").Replace("\t", ".").Replace("\n", ".").Replace("\b", ".").Replace("\f", "."), 0x10);

                    }
                    catch { }
                    if (!sleep)
                        Thread.Sleep(10);
                }
            }
        }
        public static byte[] STB(string hex)
        {
            if ((hex.Length % 2) != 0)
                hex = "0" + hex;
            int length = hex.Length;
            byte[] buffer = new byte[((length / 2) - 1) + 1];
            for (int i = 0; i < length; i += 2)
                buffer[i / 2] = Convert.ToByte(hex.Substring(i, 2), 0x10);
            return buffer;
        }
        #endregion
        #endregion
        #region Debugger
        private void hexCode_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
                debugger = false;
            Thread.Sleep(20);
            int index = hexCode.SelectionStart / 0x30;
            int num2 = (hexCode.SelectionStart - (index * 0x30)) / 3;
            string str = offsetsText.Lines.ElementAt<string>(index);
            string str2 = num2.ToString("X");
            str = str.Remove(str.Length - 1, 1) + str2;
            offsetTxt.Text = str;
            if (timer1.Enabled)
                debugger = true;
        }
        private void startHex_TextChanged(object sender, EventArgs e)
        {
            if (debugger)
            {
                if (startHex.Text == null)
                    startHex.Text = "0x00000000";
                try
                {
                    Debugging();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Something went wrong!!\n\nException Data has been copied to your clipboard. Please notify Cain532"));
                    Clipboard.SetText(ex.ToString());
                }
            }
            else
            {

            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Debugging();
        }
        private void debuggerStatus_Click(object sender, EventArgs e)
        {
            debugger = !debugger;
            if (Thread_Refresher)
            {
                cWrite = debugger ? 1 : 0;
                timer2.Enabled = debugger;
            }
            else
                timer1.Enabled = debugger;
            debuggerStatus.Text = string.Format("Auto Refresher - {0}", debugger ? "On" : "Off");
            debuggerStatus.ForeColor = debugger ? Color.LimeGreen : Color.Red;
        }
        private void WriteMem()
        {
            writebox.Text.Replace(" ", "");
            write = STB(writebox.Text);
            address = Convert.ToUInt32(offsetTxt.Text, 16);
            PS3.SetMemory(address, write);
            Debugging();
        }
        private void writebyte_Click(object sender, EventArgs e)
        {
            WriteMem();
        }
        private void writebox_TextChanged(object sender, EventArgs e)
        {
            if (debugger)
                WriteMem();
            else
            { }
        }
        private void byterefresh_Click(object sender, EventArgs e)
        {
            Debugging();
        }
        private void jumpplus_Click(object sender, EventArgs e)
        {
            uint jump = Convert.ToUInt32(startHex.Text, 16) + Convert.ToUInt32(jumpbox.Text, 16);
            startHex.Text = "0x" + Convert.ToString((long)jump, 16).ToUpper();
            Debugging();
        }
        private void jumpminus_Click(object sender, EventArgs e)
        {
            uint jump = Convert.ToUInt32(startHex.Text, 16) - Convert.ToUInt32(jumpbox.Text, 16);
            startHex.Text = "0x" + Convert.ToString((long)jump, 16).ToUpper();
            Debugging();
        }
        #endregion
    }
}