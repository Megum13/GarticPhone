﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GarticPicture
{
    public partial class Form1 : Form
    {
        public string url = null;

        string[] files;
        public bool dos = false;

        SocketIO socketIO;

        public Form1()
        {
            InitializeComponent();

            ToolTip t = new ToolTip();
            t.SetToolTip(checkBox1, "Режим дудоса, при повторном нажатии на Connect - Disconnect производиться не будет");

            socketIO = new SocketIO(this); // Общение с сокетом [Connect] [Disconnect] [Send] [DrawImage]
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            DirectoryValueChanged(0); // Первая подгрузка пикч
        }

        private void button3_Click(object sender, EventArgs e) // Connect button
        {
            var result = socketIO.Connect(textBox1.Text, textBox2.Text);
            SendToTextBox(result);
        }

        private void button8_Click(object sender, EventArgs e) // Disconect button
        {
            var result = socketIO.Disconnect();
            SendToTextBox(result);
        }

        private void button1_Click(object sender, EventArgs e) // Draw button
        {

            if (directoryValueMax <= 0)
            {
                SendToTextBox("Нет картинок");
                return;
            }

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

        public void SendToTextBox(string text, Color color = default(Color))
        {
            if (object.Equals(color, default(Color))) color = Color.Black;

            Invoke(new Action(() =>
            {
                text = color == Color.Black ? text : text = color == Color.Green ?
                text = "↑ " + text : text = "↓ " + text;

                richTextBox1.AppendText(text + '\r' + '\n', color);

                if (!checkBox2.Checked) return; // Auto scroll
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }));

        }

        private readonly string directory = "Img";
        private int directoryValue = 0;
        private int directoryValueMax = 0;
        private int directoryValueMin = 0;
        private void DirectoryValueChanged(int change)
        {
            directoryValue += change; // Изменяем значение или не изменяем если 0

            if (!Directory.Exists(directory)) // Если нет, создаём
            {
                Directory.CreateDirectory(directory);
            }

            files = Directory.GetFiles(directory); // Берем пикчи

            directoryValueMax = files.Length-1; // Выставляем макс значение

            // Костыльное решение. ИСПРАВИТЬ!
            if (directoryValueMax == -1)
            {
                SendToTextBox("Нет картинок");
                return;
            }
            else if (directoryValueMax == 0)
            {
                directoryValue = 0;
            }

            if (directoryValue >= directoryValueMax) directoryValue = directoryValueMax; // Проверка ограничений
            else if (directoryValue <= directoryValueMin) directoryValue = directoryValueMin;

            label5.Text = directoryValue + ""; // Вывод идекса картинки
            pictureBox1.Image = Image.FromFile(files[directoryValue]); // Вывод картинки

            /* Вставка изображения. Не забыть выключить ридонли и добавить сохранение в буфер.
            Image img = pictureBox1.Image;
            Clipboard.SetImage(img);
            richTextBox1.Paste();
            richTextBox1.Focus();
            */
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
                button7.Text = "Connected";
                button7.ForeColor = Color.Green;
            }
            else
            {
                button7.Text = "Not connected";
                button7.ForeColor = Color.Red;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dos = checkBox1.Checked;
        } // Dos button

        private void button9_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void textBox2_Click(object sender, EventArgs e) // Выделение текста для удобства
        {
            textBox2.SelectionStart = 0;
            textBox2.SelectionLength = textBox2.Text.Length;
        }

        public void InformationChanger(int number , string text) // 0 - 5
        {
            Label[] labels = { label6, label7, label8, label9, label10, label11};

            if (number < 0 || number > labels.Length) // На всякий
            {
                Console.WriteLine("Вышло за пределы");
                return;
            }

            Invoke(new Action(() => 
            { 
                labels[number].Text = text;
            }));

        }

        private void button10_Click(object sender, EventArgs e) // открыть папку
        {
            System.Diagnostics.Process.Start(directory);
        }
    }
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}

