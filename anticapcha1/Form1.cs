using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace anticapcha1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NewCapcha();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            NewCapcha();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            NewCapcha();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            NewCapcha();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text.Length == 6)
            {
                Anticapcha.EnteredCaptcha(maskedTextBox1.Text);
                NewCapcha();
            }
        }

        private bool NewCapcha()
        {
            if (Anticapcha.NewCapcha())
            {
                pictureBox1.Image = Anticapcha.CapchaImage;
                pictureBox2.Image = Anticapcha.CapchaImageContur;
                pictureBox3.Image = Anticapcha.CapchaOriginImage;
                if (!string.IsNullOrEmpty(Anticapcha.CapchaText))
                    maskedTextBox1.Text = Anticapcha.CapchaText;
                else
                    maskedTextBox1.Clear();
                return true;
            }
            return false;
        }
    }
}
