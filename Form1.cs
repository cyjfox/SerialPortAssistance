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
using System.Diagnostics;

namespace SerialPortAssistance
{
    public partial class Form1 : Form
    {
        private static FileStream fileStream;
        private static uint count = 0;
        private static int lastChannelNumber = -1;
        private static RingBuffer<byte> ringBuffer = new RingBuffer<byte>(40960);
        private static bool toExit = false;
        private static int channelNumber = -1;

        private static Bitmap bitmap = new Bitmap(1200, 800);
        private static Graphics backgroundGraphics = Graphics.FromImage(bitmap);
        private static Graphics foregroundGraphics;
        private static Pen[] pens = new Pen[4];
        private static double x0 = 150.0, y0 = 400.0;
        private static double lastX = x0;
        private static SolidBrush whiteBrush = new SolidBrush(Color.White);
        private static PointF[] lastPoints = new PointF[4];
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
            foregroundGraphics = Graphics.FromHwnd(pictureBox1.Handle);

            //for (int i = 0; i < 4; i++)
            {
                pens[0] = new Pen(Color.Red);
                pens[1] = new Pen(Color.Green);
                pens[2] = new Pen(Color.Blue);
                pens[3] = new Pen(Color.Yellow);
            }

            for (int i = 0; i < 4; i++)
            {
                lastPoints[i].X = (float)x0;
                lastPoints[i].Y = (float)y0;
            }

            //count = 0;
            System.Timers.Timer t = new System.Timers.Timer(100);//实例化Timer类，设置间隔时间为100毫秒；

            t.Elapsed += new System.Timers.ElapsedEventHandler(timeout);//到达时间的时候执行事件；

            t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

            
            

            //Thread thread = new Thread(doWork);
            //thread.Start();
            serialPort1.Open();

        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string str = serialPort1.ReadExisting();     //字符串方式读
            //serialPort1.read
            //textBox1.AppendText(str);//添加内容
            byte[] buf = System.Text.Encoding.Default.GetBytes(str);
            fileStream.Write(buf, 0, buf.Length);

            //如果格式正确
            /*
            if (buf[8] == '\n')
            {
                int value = Int32.Parse(str);
                int channelNumber = buf[7] & 0x07;
                
                if (lastChannelNumber == -1)
                {
                    lastChannelNumber = channelNumber;
                }
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
            */

            ringBuffer.Push(buf, 0, buf.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            fileStream.Close();
        }

        
        public static void doWork(object arguments)
        {
            byte[] buf = new byte[16];
            int lastRead = 0;
            PointF[] lastPoints = new PointF[4];
            for (int i = 0; i < 4; i++)
            {
                lastPoints[i].X = 0;
                lastPoints[i].Y = 0;
            }
            Point[] currentPoints = new Point[4];
            double maxValue = 2.5, minValue = -2.5;
            int x = 0, y = 0;
            while (!toExit)
            {
                //环形缓冲区的头指针指向尚未读取的数据的开始
                //环形缓冲区的尾指针指向尚未读取的数据的结束
                while ((ringBuffer.Tail - ringBuffer.Head) >= 9)
                {
                    //读取9个字节的数据
                    ringBuffer.Pop(buf, 0, 9);
                    if (buf[8] != '\n')
                    {
                        MessageBox.Show("非法格式的数据");
                        buf[8] = 0;
                        string str = System.Text.Encoding.ASCII.GetString(buf);
                        MessageBox.Show(str);
                        //Console.WriteLine("非法格式的数据!");
                    }
                    else
                    {
                        buf[8] = 0;
                        string str = System.Text.Encoding.ASCII.GetString(buf);
                        //str = "88ffc013";
                        uint value = UInt32.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        int channel = (int)(value & 0x0f);

                        if (channelNumber == -1)
                        {
                            channelNumber = channel;
                        }

                        if (channelNumber != channel)
                        {
                            MessageBox.Show("数据同步出错");
                            //errorCount++;
                        }
                        value = value >> 4;
                        double finalValue = (int)((int)value - (int)0x8000000) * 2.5 / 134217728.0;
                        
                        //SolidBrush transparentBrush = new SolidBrush(Color.Transparent);
                        //SolidBrush transparentBrush = new SolidBrush(Color.Red);
                        //backgroundGraphics.FillRectangle(transparentBrush, new Rectangle(x, y, 15, 800));
                         
                        //foregroundGraphics.DrawImage(bitmap, 0, 0);
                        channelNumber++;
                        
                        if (channelNumber >= 4)
                        {
                            channelNumber = 0;
                        }
                    }
                    
                    
                   
                }
            }
        }

        /*
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
        */

        public void timeout(object source, System.Timers.ElapsedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] buf = new byte[16];
            int lastRead = 0;
            
            
            PointF[] currentPoints = new PointF[4];
            for (int i = 0; i < 4; i++)
            {
                currentPoints[i].X = (float)x0;
                currentPoints[i].Y = (float)y0;
            }
            double maxValue = 2.5, minValue = -2.5;
            int x = 0, y = 0;
            //while (!toExit)
            {
                int dataSize = ringBuffer.DataSize;
                int toRead = dataSize - dataSize % 9;
                byte[] data = new byte[toRead];
                ringBuffer.Pop(data, 0, toRead);
                int count = toRead / 9;
                RingBuffer<byte> tempBuffer = new RingBuffer<byte>(toRead);
                tempBuffer.Push(data, 0, toRead);
                //环形缓冲区的头指针指向尚未读取的数据的开始
                //环形缓冲区的尾指针指向尚未读取的数据的结束
                while (tempBuffer.DataSize > 0)
                {
                    //读取9个字节的数据
                    tempBuffer.Pop(buf, 0, 9);
                    if (buf[8] != '\n')
                    {
                        MessageBox.Show("非法格式的数据");
                        buf[9] = 0;
                        string str = System.Text.Encoding.ASCII.GetString(buf);
                        MessageBox.Show(str);
                        //Console.WriteLine("非法格式的数据!");
                    }
                    else
                    {
                        buf[8] = 0;
                        string str = System.Text.Encoding.ASCII.GetString(buf);
                        //str = "88ffc013";
                        uint value = UInt32.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        int channel = (int)(value & 0x0f);

                        if (channelNumber == -1)
                        {
                            channelNumber = channel;
                        }

                        if (channelNumber != channel)
                        {
                            MessageBox.Show("数据同步出错");
                            //errorCount++;
                        }
                        value = value >> 4;
                        double finalValue = (int)((int)value - (int)0x8000000) * 2.5 / 134217728.0;
                        //30倍放大
                        currentPoints[channelNumber].Y = (float)(y0 - 400.0 * finalValue / 2.5);
                        
                        if (lastX - x0 <= 0.0001)
                        {
                            lastPoints[channelNumber].X = currentPoints[channelNumber].X = (float)x0;
                            lastPoints[channelNumber].Y = currentPoints[channelNumber].Y;
                        }
                        else
                        {
                            currentPoints[channelNumber].X = (float)(lastX);
                        }
                        
                        Console.WriteLine("current-X : " + currentPoints[channelNumber].X + " , current-Y : " + currentPoints[channelNumber].Y + " , last-X : " + lastPoints[channelNumber].X + " , last-Y : " + lastPoints[channelNumber].Y);

                        backgroundGraphics.DrawLine(pens[channelNumber], currentPoints[channelNumber], lastPoints[channelNumber]);
                        lastPoints[channelNumber].X = currentPoints[channelNumber].X;
                        lastPoints[channelNumber].Y = currentPoints[channelNumber].Y;
                        //Console.WriteLine("finalValue : " + finalValue);
                        
                        //currentPoints[channelNumber].Y = finalValue / 2.5 * 
                        //SolidBrush transparentBrush = new SolidBrush(Color.Transparent);
                        //SolidBrush transparentBrush = new SolidBrush(Color.Red);
                        //backgroundGraphics.FillRectangle(transparentBrush, new Rectangle(x, y, 15, 800));

                        //foregroundGraphics.DrawImage(bitmap, 0, 0);
                        channelNumber++;

                        if (channelNumber >= 4)
                        {
                            channelNumber = 0;
                            lastX += 4.0;
                            if (lastX >= 1000.0 + 4.0 + x0)
                            {
                                lastX = x0;
                            }
                        }
                    }
                }

                //再擦掉前面5个数据
                if (1000.0 - lastX + 4.0 >= 20.0)
                {
                    backgroundGraphics.FillRectangle(whiteBrush, new RectangleF((float)(lastX - 4.0), (float)(y0 - 400), 20.0F, 800.0F));
                }
                else
                {
                    //先擦掉最后剩下的所有
                    backgroundGraphics.FillRectangle(whiteBrush, new RectangleF((float)(lastX - 4.0), (float)(y0 - 400), (float)(1000.0 - lastX + 4.0), 800.0F));
                    //再从头擦掉一部分补足
                    backgroundGraphics.FillRectangle(whiteBrush, new RectangleF(0.0F, (float)(y0 - 400), (float)(20.0 - 1000.0 + lastX - 4.0), 800.0F));
                }

                //更新到前台
                foregroundGraphics.DrawImage(bitmap, 0, 0);
            }

            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed;
            Console.WriteLine("DateTime costed for Shuffle function is: {0}ms", timespan.TotalMilliseconds);
        }

        private void draw()
        {
            Graphics graphics = Graphics.FromHwnd(pictureBox1.Handle);
            graphics.DrawEllipse(Pens.Red, new Rectangle(20, 780, 200, 200));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Graphics graphics = Graphics.FromHwnd(pictureBox1.Handle);
            graphics.DrawLine(new Pen(Color.Black), 150, 400, 200, 400);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            draw();
        }
    }

    
}
