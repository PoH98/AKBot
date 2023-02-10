using AKBot.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zeraniumu;

namespace AKBot
{
    public partial class Form1 : Form
    {
        private ILog log;
        private Script bot;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bot = new CustomScript(new BotScript(log));
            bot.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            log = new Log(logTextBox);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(bot != null)
                bot.Stop();
        }

        private void logTextBox_TextChanged(object sender, EventArgs e)
        {
            logTextBox.ScrollToCaret();
        }
    }
}
