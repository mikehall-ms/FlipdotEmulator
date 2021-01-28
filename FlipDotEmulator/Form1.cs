using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlipDotEmulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool[,] bFlipArray = new bool[14, 28];
        int iDotwidth = 0;
        int iDotHeight = 0;
        bool bInUpdate = false;

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(
               0, 0, 600, 300);
            e.Graphics.FillRectangle(Brushes.Black, rectangle);
            for (int x = 0; x < 28; x++)
            {
                for (int y = 0; y < 14; y++)
                {
                    Rectangle rcDot = new Rectangle(new Point(x * iDotwidth, y * iDotHeight), new Size(iDotwidth, iDotHeight));
                    e.Graphics.DrawEllipse(System.Drawing.Pens.Gray, rcDot);
                    if (bFlipArray[y, x] == true)
                    {
                        e.Graphics.FillEllipse(Brushes.BlanchedAlmond, rcDot);
                    }
                }
            }

        }

        private SerialPort _serialPort;
        private bool _continue = true;

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: check whether we have an existing instance of the app
            // TODO: if yes, switch to the existing app.

            string[] sPortNames = SerialPort.GetPortNames();
            if (sPortNames.Count() == 0)
            {
                MessageBox.Show("You don't have any active Serial Ports");
                return;
            }

            SerialSelect sDialog = new SerialSelect();
            if (DialogResult.OK == sDialog.ShowDialog())
            {
                _serialPort = new SerialPort();
                _serialPort.PortName = sDialog.serialName;
                _serialPort.BaudRate = 57600;
                _serialPort.Open();

                Thread readThread = new Thread(Read);
                readThread.Start();

                iDotwidth = 600 / 28;
                iDotHeight = 300 / 14;

                int iFormHeight=(iDotHeight*14);
                int iFormWidth = (iDotwidth * 28);

                this.ClientSize = new Size(iFormWidth, iFormHeight);

                this.DoubleBuffered = true;
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 28; x++)
                    {
                        bFlipArray[y, x] = false;
                    }
                }

                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Tick += T_Tick;
                t.Interval = 100;
                t.Start();
            }
        }

        public void Read()
        {
            byte[] frame = new byte[40];
            int iCounter = 0;
            bool bInFrame = false;
            byte bStartFrame = 0x80;
            byte bEndFrame = 0x8f;

            while (_continue)
            {
                try
                {
                    int iByte = _serialPort.ReadByte();

                    // if we're not in a frame and we get a frame byte.
                    if (!bInFrame && bStartFrame == iByte)
                    {
                        bInFrame = true;
                        iCounter = 0;
                    }

                    // if we're in the frame, add the byte to the buffer
                    if (bInFrame)
                    {
                        frame[iCounter++] = (byte)iByte;
                    }

                    // if we're in the frame, and we get an end byte
                    // process the buffer.
                    if (bInFrame && iCounter == 32 && bEndFrame == iByte)
                    {
                        bInUpdate = true;
                        ProcessFrame(frame);
                        bInUpdate = false;
                        bInFrame = false;
                        iCounter = 0;
                    }

                    // if (for some reason) we go over the buffer size
                    // reject the buffer
                    if (bInFrame && iCounter > 32)
                    {
                        bInFrame = false;
                        iCounter = 0;
                    }

                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Exception: {e.Message}, iCounter: {iCounter}.");
                }
            }
        }

        public int[] Mask = { 64, 32, 16, 8, 4, 2, 1 };

        public void ProcessFrame(byte[] bFrame)
        {
            // bytes 3 to 31 are display bytes.

            int iOffset = 0;

            if (bFrame[2] == 1) // first display
            {
                iOffset = 7;
            }

            for (int x = 0; x < 28; x++)
            {
                int bByte = (int)bFrame[x + 3]; // vertical stripe of pixels.
                for (int iMask = 0; iMask < 7; iMask++)
                {
                    int iMasked = bByte & Mask[iMask];
                    bool bSet = false;
                    if (0 != iMasked)
                    {
                        bSet=true;
                    }
                    // protocol is vertical stripe, right to left
                    // to get this into 'physical layout' we need to offset
                    bFlipArray[iMask+iOffset, 27-x] = bSet;
                }
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            if (!bInUpdate)
                this.Invalidate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;
            if (null != _serialPort)
                _serialPort.Close();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        bool bTopMost = false;

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                string sTitle= "Flipdot Emulator";
                bTopMost = !bTopMost;
                this.TopMost = bTopMost;
                if (!bTopMost)
                {
                    this.Text = sTitle;
                }
                else
                {
                    this.Text = sTitle + " [Pinned]";
                }
            }
        }
    }
}

