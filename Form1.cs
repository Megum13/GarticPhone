using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GarticPicture
{
    public partial class Form1 : Form
    {
        public string url = null;

        string[] files;

        SocketIO socketIO;
        string directory = "Img";
        int directoryValue = 0;
        int directoryValueMax = 0;
        int directoryValueMin = 0;

        public Form1()
        {
            InitializeComponent();
            DirectoryValueChanged(0);

            socketIO = new SocketIO(this); // Общение с сокетом [Connect] [Disconnect] [Send] [DrawImage]
        }

        private void button3_Click(object sender, EventArgs e) // Connect button
        {
            var result = socketIO.Connect(textBox3.Text, textBox1.Text, textBox2.Text);
            SendToTextBox(result);
        }

        private void button8_Click(object sender, EventArgs e) // Disconect button
        {
            var result = socketIO.Disconnect();
            SendToTextBox(result);
        }

        private void button1_Click(object sender, EventArgs e) // Draw button
        {
            var result = socketIO.DrawImage(Image.FromFile(files[directoryValue]));
            SendToTextBox(result);
        }

        private void button2_Click(object sender, EventArgs e) // Done button
        {
            var result = socketIO.Send("42[2,15,true]");
            SendToTextBox(result);
        }

        private void button4_Click(object sender, EventArgs e) // Send text button
        {
            var result = socketIO.Send(textBox5.Text);
            SendToTextBox(result);
        }

        public void SendToTextBox(string text)
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

        private void DirectoryValueChanged(int change)
        {
            directoryValue += change; // Изменяем значение (если не 0)

            files = Directory.GetFiles(directory); // Берем пикчи
            directoryValueMax = files.Length - 1; // Выставляем макс значение

            if (directoryValue >= directoryValueMax) directoryValue = directoryValueMax; // Проверка ограничений
            else if (directoryValue <= directoryValueMin) directoryValue = directoryValueMin;

            label5.Text = directoryValue + ""; // Вывод информации
            pictureBox1.Image = Image.FromFile(files[directoryValue]); // Вывод картинки
        }

        private void button5_Click(object sender, EventArgs e) // Directory change right
        {
            DirectoryValueChanged(1);
        }

        private void button6_Click(object sender, EventArgs e) // Directory change left
        {
            DirectoryValueChanged(-1);
        }

        private void button7_Click(object sender, EventArgs e) // Connect information visible/unvisible
        {
            panel1.Visible = panel1.Visible ? panel1.Visible = false : panel1.Visible = true;
        }

        public void ConnectInformationButton(bool isTrue)
        {
            if (isTrue)
            {
                button7.Text = "Подключено";
                button7.ForeColor = Color.Green;
            }
            else 
            {
                button7.Text = "Не подключено";
                button7.ForeColor = Color.Red;
            }
        }

    }

}

