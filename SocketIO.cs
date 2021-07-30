using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace GarticPicture
{
    class SocketIO
    {
        WebSocket ws;
        Random ran = new Random();

        bool isConnected = false;
        Form1 form1;

        int turnNum = 0;

        public SocketIO(Form1 form1)
        {
            this.form1 = form1; // Получаем форму

            Thread th = new Thread(() =>  // Проверка подключения
            {
                while (true)
                {
                    if (ws != null) isConnected = ws.IsAlive;
                    else if (ws == null) isConnected = false;
                }
            });
            th.IsBackground = true;
            th.Start();

            Thread th2 = new Thread(() => // Проверка соеденения с сокетом
            {
                while (true)
                {
                    if (isConnected)
                    {
                        ws.Send("3");
                    }
                    Thread.Sleep(25000);
                }
            });
            th2.IsBackground = true;
            th2.Start();
        }


        public string Connect(string socket , string nick, string roomCode)
        {
            if (isConnected) return Disconnect();

            ws = new WebSocket($"wss://sv-{socket}.garticphone.com/socket.io/?EIO=4&transport=websocket");

            ws.OnMessage -= MessageSocket;
            ws.OnMessage += MessageSocket;

            try 
            { 
                ws.Connect();
            }
            catch
            {
                form1.ConnectInformationButton(false);
                return "Подключение не удалось";
            }

            ws.Send("40");
            ws.Send($"42[1,\"{"bfcc9fd4-e2a2-44db-a6e4-0d05eed1fb0e" + ran.Next(0, 1000)}\",\"{nick}\",{ran.Next(1, 30)},\"ru\",false,\"{roomCode}\"]");

            form1.ConnectInformationButton(true);
            return "Подключено";
        }

        public string Disconnect()
        {
            if (!isConnected) return "Нет подключения";

            ws.Close();
            form1.ConnectInformationButton(false);
            return "Отключено";
        }

        public string Send(string text)
        {
            if (!isConnected) return "Нет подключения";

            ws.Send(text);

            return "Отправлено";
        }

        public string DrawImage(Image img)
        {
            if (!isConnected) return "Нет подключения";

            Thread th = new Thread(() => 
            {
                Bitmap btm = new Bitmap(img, 758, 424);

                for (int Y = 0; Y < 424; Y += 2)
                {
                    for (int X = 0; X < 758; X += 2)
                    {

                        var pixel = btm.GetPixel(X, Y);

                        string hex = pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2");

                        if (!(pixel.R == 255 && pixel.G == 255 && pixel.B == 255))
                        {
                            ws.Send($"42[2,7,{{\"t\":{turnNum},\"v\":[6,[\"#{hex}\",1,1],[{X},{Y}] , [{X + 2},{Y + 2}]]}}]");
                        }

                    }
                }

                ws.Send("42[2,15,true]");
                form1.SendToTextBox("Изображение успешно отправлено");
            });
            th.IsBackground = true;
            th.Start();

            return "Отправление изображения...";

        }

        private void MessageSocket(object sender, MessageEventArgs e)
        {

            if (e.IsText)
            {
                char[] charData = e.Data.ToCharArray();

                if (e.Data != null)
                {
                    form1.SendToTextBox(e.Data);
                }

                if (charData.Length >= 21 && charData[5] == '1' && charData[6] == '1')
                {
                    string[] a = e.Data.Split(',');
                    string[] b = a[2].Split(':');
                    turnNum = Int32.Parse(b[1].ToString());
                }

            }

        }

    }
}
