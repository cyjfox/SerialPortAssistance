using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace SerialPortAssistance
{
    public partial class Form1 : Form
    {
        static FileStream fileStream;
        static uint count = 0;
        static int lastChannelNumber = 0;
        //static uint 
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            fileStream = File.Create(@"D:\workspace\SerialPortAssistance\data.txt");
            //count = 0;
            System.Timers.Timer t = new System.Timers.Timer(100000);//实例化Timer类，设置间隔时间为10000毫秒；

            t.Elapsed += new System.Timers.ElapsedEventHandler(timeout);//到达时间的时候执行事件；

            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；

            //t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            serialPort1.Open();

        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string str = serialPort1.ReadExisting();     //字符串方式读
            //serialPort1.read
            textBox1.AppendText(str);//添加内容
            byte[] buf = System.Text.Encoding.Default.GetBytes(str);
            fileStream.Write(buf, 0, buf.Length);

            //如果格式正确
            if (buf[8] == '\n')
            {
                int channelNumber = buf[7] & 0x07;
                if (channelNumber == lastChannelNumber)
                {
                    //int value = Int32.Parse();
                }

                lastChannelNumber++;
                if (lastChannelNumber >= 4)
                {
                    lastChannelNumber = 0;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            fileStream.Close();
        }

        
        public static void doWork(object arguments)
        {

        }
 

        public void timeout(object source, System.Timers.ElapsedEventArgs e)
        {
            button2_Click(null, null);
            FileStream fs = File.Open(@"D:\workspace\SerialPortAssistance\data.txt", FileMode.Open);
            //while (fs.)
            byte[] buf = new byte[16];
            int i = 0;
            int num;
            int channelNumber = -1;
            int errorCount = 0;
            do
            {
                num = fs.Read(buf, 0, 9);
                i++;
                if (buf[8] != '\n')
                {
                    MessageBox.Show("非法格式的数据");
                }
                int channel = buf[7] & 0x07;
                if (channelNumber == -1)
                {
                    channelNumber = channel;
                }
                if (channelNumber != channel)
                {
                    //MessageBox.Show("数据同步出错");
                    errorCount++;
                }

                channelNumber++;
                if (channelNumber >=4)
                {
                    channelNumber = 0;
                }
            } while (num >= 9);

            MessageBox.Show("total number of samples is : " + i);
            MessageBox.Show("error count is : " + errorCount);
            MessageBox.Show("OK!");

        }

        private void draw()
        {
            Graphics graphics = Graphics.FromHwnd(pictureBox1.Handle);
            graphics.DrawEllipse(Pens.Red, new Rectangle(20, 780, 200, 200));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            draw();
        }
    }

    
}
