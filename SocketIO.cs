using System;
using System.Drawing;
using System.Net;
using System.Threading;
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
                        form1.SendToTextBox("3", Color.Green);
                    }
                    Thread.Sleep(25000);
                }
            });
            th2.IsBackground = true;
            th2.Start();
        }


        public string Connect(string nick, string roomLink)
        {
            if (isConnected && !form1.dos) return Disconnect();

            if (nick == null || nick == "") return "Ник не может быть пустым (хочешь пустой ставь пробел)";

            var roomCode = "";
            if (roomLink.Contains("?c=")) roomCode = roomLink.Split('=')[1];
            else
            {
                return "Ошибка связанная с ссылкой";
            }

            var response = "";

            using (var webClient = new WebClient()) // Получаем sv-
            {
                try
                {
                    response = webClient.DownloadString($"https://garticphone.com/api/server?code={"" + roomCode + ""}"); // https://sv-43.garticphone.com
                }
                catch (Exception ex)
                {
                    form1.SendToTextBox("ОШИБКА");
                    return ex.Message;
                }
            }

            var cutResponseLink = response.Substring(5, response.Length-5);
            ws = new WebSocket($"wss{cutResponseLink}/socket.io/?EIO=4&transport=websocket");

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

            var sendConnect = $"42[1,\"{"bfcc9fd4-e2a2-44db-a6e4-0d05eed1fb0e" + ran.Next(0, 1000)}\",\"{nick}\",{ran.Next(1, 30)},\"ru\",false,\"{roomCode}\"]";
            ws.Send("40");
            form1.SendToTextBox("40", Color.Green);
            ws.Send(sendConnect);
            form1.SendToTextBox(sendConnect, Color.Green);

            form1.ConnectInformationButton(true);

            form1.InformationChanger(0, $"Nick: {nick}");

            return "Подключено";
        }

        public string Disconnect()
        {
            if (!isConnected) return "Нет подключения";

            ws.Send("42[2,28]");
            form1.SendToTextBox("42[2,28]", Color.Green);
            ws.Send("41");
            form1.SendToTextBox("41", Color.Green);
            ws.Close();

            form1.ConnectInformationButton(false);
            turnNum = 0;

            form1.InformationChanger(0, "Nick: Unknown");
            form1.InformationChanger(1, "Room: Unknown");
            form1.InformationChanger(2, "Game status: Unknown");
            form1.InformationChanger(3, "Round: Unknown");

            return "Отключено";
        }

        public string Send(string text)
        {
            if (!isConnected) return "Нет подключения";

            ws.Send($"42[2,6,{{\"t\":{turnNum},\"v\":\"{text}\"}}]");
            form1.SendToTextBox($"42[2,6,{{\"t\":{turnNum},\"v\":\"{text}\"}}]", Color.Green);
            ws.Send("42[2,15,true]");
            form1.SendToTextBox("42[2,15,true]", Color.Green);

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
                            if (form1.animationMode)
                                ws.Send($"42[2,7,{{\"t\":{turnNum},\"v\":[1,[\"#{hex}\",3,1],[{X},{Y}] , [{X + 2},{Y + 2}]]}}]"); // Режим кисти
                            else
                                ws.Send($"42[2,7,{{\"t\":{turnNum},\"v\":[6,[\"#{hex}\",1,1],[{X},{Y}] , [{X + 2},{Y + 2}]]}}]"); // Режим прямоугольников
                        }

                    }
                }

                ws.Send("42[2,15,true]");
                form1.SendToTextBox("42[2,15,true]", Color.Green);
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

                if (e.Data != null)
                {
                    form1.SendToTextBox(e.Data, Color.Red);
                }

                if (e.Data.Contains("42[2,11,{")) // Начало нового раунда
                {
                    string[] a = e.Data.Split(',');
                    string[] b = a[2].Split(':');
                    turnNum = Int32.Parse(b[1].ToString());
                    form1.InformationChanger(3, $"Round: {turnNum}");
                }

                else if (e.Data.Contains("42[2,5,2]")) // Начало игры
                {
                    form1.InformationChanger(2, "Game status: Started");
                }

                else if (e.Data.Contains("42[2,24]")) // Конец игры
                {
                    form1.InformationChanger(2, "Game status: Over");
                }

            }

        }


        /// 42[2,5,2] начало раунда
        /// 42[2,11,{"turnNum":1,"screen":5,"previous":{"id":-1,"user":{"id":1,"nick":"01","avatar":"19","owner":true,"viewer":false,"points":0,"change":0},"type":2,"data":"да","active":true}}] Ход со строки на рисование
        /// 42[2,24] раунд закончен
        /// 42[2,11,{"turnNum":0,"screen":5,"previous":null}] ход рисования
        /// 42[2,11,{"turnNum":1,"screen":4,"previous":{"id":-1,"user":{"id":1,"nick":"01","avatar":"19","owner":true,"viewer":false,"points":0,"change":0},"type":1,"data":[[1,["#000000",6,1],[391,55]]],"active":true}}] пришло для комментирования

    }

}
