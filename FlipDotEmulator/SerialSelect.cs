using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlipDotEmulator
{
    public partial class SerialSelect : Form
    {
        public SerialSelect()
        {
            InitializeComponent();
        }

        public string serialName=string.Empty;

        private void SerialSelect_Load(object sender, EventArgs e)
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                serialPorts.Items.Add(s);
            }

            if (serialPorts.Items.Count > 0)
            {
                serialPorts.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (-1 != serialPorts.SelectedIndex)
            {
                serialName=serialPorts.GetItemText(serialPorts.SelectedItem);
            }
        }
    }
}
