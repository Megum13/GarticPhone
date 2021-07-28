using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace WevSocketScetchful
{
    public partial class Form1 : Form
    {
        public string url = null;
        WebSocket ws;
        int turnNum = 0;
        string[] files;

        Random ran = new Random();

        public Form1()
        {
            InitializeComponent();

            files = Directory.GetFiles("Img");
            numericUpDown1.Maximum = files.Length - 1;

            pictureBox1.Image = Image.FromFile(files[(int)numericUpDown1.Value]);
        }

        private void button3_Click(object sender, EventArgs e) // connect
        {
            ws = new WebSocket(url = $"wss://sv-{textBox3.Text}.garticphone.com/socket.io/?EIO=4&transport=websocket");

            ws.OnMessage -= MessageSocket;
            ws.OnMessage += MessageSocket;

            ws.Connect();

            Thread th = new Thread(() => SocketRe());
            th.IsBackground = true;
            th.Start();


            string[] s = textBox2.Text.Split('=');

            ws.Send("40");
            SendToTextBox("40");
            ws.Send($"42[1,\"{"bfcc9fd4-e2a2-44db-a6e4-0d05eed1fb0e" + ran.Next(0, 1000)}\",\"{textBox1.Text}\",{ran.Next(1, 30)},\"ru\",false,\"{textBox2.Text}\"]");
            SendToTextBox($"42[1,\"{"bfcc9fd4-e2a2-44db-a6e4-0d05eed1fb0e" + ran.Next(0, 1000)}\",\"{textBox1.Text}\",{ran.Next(1, 30)},\"ru\",false,\"{textBox2.Text}\"]");


        }

        public void DrawImage(int x, int y, int width, int height)
        {

            Image img = Image.FromFile(files[(int)numericUpDown1.Value]);
            Bitmap btm = new Bitmap(img, width, height);

            for (int Y = 0; Y < height; Y += 2)
            {
                for (int X = 0; X < width; X += 2)
                {

                    var pixel = btm.GetPixel(X, Y);


                    string hex = pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2");

                    if (!(pixel.R == 255 && pixel.G == 255 && pixel.B == 255))
                    {
                        ws.Send($"42[2,7,{{\"t\":{turnNum},\"v\":[6,[\"#{hex}\",1,1],[{X + x},{Y + y}] , [{X + 2 + x},{Y + 2 + y}]]}}]");
                    }

                }
            }

            SendToTextBox("Отправка на холст пикчи завершена");
            SendToTextBox("42[2,15,true] (You)");
            ws.Send("42[2,15,true]");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() => DrawImage(0, 0, 758, 424));
            th.IsBackground = true;
            th.Start();
        }

        void MessageSocket(object sender, MessageEventArgs e)
        {

            if (e.IsText)
            {
                // 42[1,{"error":3}]
                char[] charData = e.Data.ToCharArray();

                if (e.Data != null)
                {
                    SendToTextBox(e.Data);
                }

                if (charData.Length >= 21 && charData[5] == '1' && charData[6] == '1')
                {
                    string[] a = e.Data.Split(',');
                    string[] b = a[2].Split(':');
                    turnNum = Int32.Parse(b[1].ToString());
                }

            }

        }

        void SendToTextBox(string text)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.Text += text + Environment.NewLine;
                if (richTextBox1.Lines.Count() > 33)
                {
                    richTextBox1.Text = "";
                }
            }));
        }

        void SocketRe()
        {
            while (true)
            {
                if (ws.IsAlive)
                {
                    try
                    {
                        ws.Send("3");
                        SendToTextBox("3");
                        Thread.Sleep(25000);
                    }
                    catch
                    {
                    }

                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            ws.Send("42[2,15,true]");
        }

        private void button4_Click(object sender, EventArgs e)
        {

            ws.Send($"42[2,6,{{\"t\":{turnNum},\"v\":\"{textBox5.Text}\"}}]");
            ws.Send("42[2,15,true]");

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            files = Directory.GetFiles("Img");
            numericUpDown1.Maximum = files.Length - 1;

            pictureBox1.Image = Image.FromFile(files[(int)numericUpDown1.Value]);
        }
        
    }

}

