using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace fes101test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static Socket conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Thread NetSendThread, NetRecvThread;
        private delegate void LogAppendDelegate(Color color, string text);
        int index = 0;
        List<string> TxList = new List<string>();
        byte[]Sendadr;
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开")
            {
                conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                conn.Connect(IPAddress.Parse(textBoxIP.Text.Trim()), Convert.ToInt16(textBoxPort.Text.Trim()));

                NetSendThread = new Thread(new ThreadStart(SendProc));
                NetSendThread.Start();

                NetRecvThread = new Thread(new ThreadStart(ReceiveProc));
                NetRecvThread.Start();
                button1.Text = "关闭";
            }
            else
            {
                conn.Close();
                if (NetSendThread != null)
                    NetSendThread.Abort();
                if (NetRecvThread != null)
                    NetRecvThread.Abort();
                button1.Text = "打开";
            }
        }
        private void LogAppend(Color color, string text)
        {
            if (richTextBox1.ReadOnly == true)
                return;
            richTextBox1.SelectionColor = color;
            richTextBox1.AppendText(text);
            richTextBox1.ScrollToCaret();
        }
        private void SendMessage(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBox1.Invoke(la, Color.Black, text);
        }
        private void ReceiveMessage(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBox2.Invoke(la, Color.Green, text);
        }

        private void  StrToByteArray(string str)
        {
            str = str.Replace(" ","");
            int len = (str.Length) / 2;
            Sendadr = new byte[len];
            for (int i = 0; i < len; i++)
            {
                Sendadr[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string FileName = System.Environment.CurrentDirectory + "\\Cfg.txt";
            FileStream afile;
            StreamReader sr;
            string strline="";
            afile = new FileStream(FileName, FileMode.Open);//
            sr = new StreamReader(afile);

            while (strline != "Start")
            {
                strline = sr.ReadLine();
            }
            while (true)
            {
                strline = sr.ReadLine();
                if (strline.IndexOf("End") >= 0)
                    break;
                TxList.Add(strline);
               
            }
            sr.Close();
            afile.Close();
        }
        private void SendProc()   //网络发送数据线程
        {
            while (true)    //处理事物
            {
                if (index >= TxList.Count)
                {
                    index = 0;
                    return;
                }
                try
                {
                    StrToByteArray(TxList[index]);
                    conn.Send(Sendadr, 0, Sendadr.Length, 0);
                    StringBuilder str = new StringBuilder("发送:");
                    for (int i = 0; i < Sendadr.Length; i++)
                    {
                        Invoke(new Action(() =>
                        {
                            str.Append(" ");
                            str.Append(Sendadr[i].ToString("X2"));
                        }));
                    }
                    SendMessage(str + string.Format(" ({0})", System.DateTime.Now) + "\n");
                    index++;
                }
                catch
                {
                }
                Thread.Sleep(1000);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void ReceiveProc()
        {
            int NetRLen=0;
            byte[] NetRBuffer = new byte[1024];
            while (true)
            {
                try
                {
                     NetRLen = conn.Receive(NetRBuffer);
                    if (NetRLen > 0)
                    {
                        StringBuilder str = new StringBuilder("接收:");
                        for (int i = 0; i < NetRLen; i++)
                        {
                            Invoke(new Action(() =>
                            {
                                str.Append(" ");
                                str.Append(NetRBuffer[i].ToString("X2"));
                            }));
                        }
                        ReceiveMessage(str + string.Format(" ({0})", System.DateTime.Now) + "\n");
                    }
                    else
                    {

                    }
                }
                catch
                {
                }
                Thread.Sleep(1);

            }
        }
    }
}

