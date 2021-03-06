﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace ЭЦП_на_эллиптических_кривых
{
    public partial class Form1 : Form
    {
        //Эллиптическая кривая
        //const int p = 449;
        //const int a = 1, b = 3; //опорная точка
        const int p = 41;//7;//41;
        const int a = 1, b = 3; //опорная точка
        Curve group;

        public Form1()
        {
            InitializeComponent();
            group = new Curve();
            //здесь можно определить свой генератор группы
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            string text = richTextBox1.Text;
            string sig = group.GenerateSignature(text);

            richTextBox2.Text = text;
            richTextBox4.Text = sig;

            richTextBox3.Text = "";
            richTextBox5.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text = richTextBox2.Text;
            string sig = richTextBox4.Text;

            richTextBox3.Text = text;
            richTextBox5.Text = group.VerifySignature(text, sig).ToString();
        }
    }
}
