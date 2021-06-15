using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortAssistance
{
    public class RingBuffer
    {
        private int head;
        private int tail;
        private int size;
        private byte[] buffer;
        private int remain;
        
        public RingBuffer(int size)
        {
            this.size = size;
            this.head = 0;
            this.tail = 0;
            this.buffer = new byte[size];
            this.remain = size;
        }

        public void Push(byte[] data, int offset, int size)
        {
            //如果剩余的空间足够装下新数据
            if (this.remain >= size)
            {
                for (int i = 0; i < size; i++)
                {
                    this.buffer[tail + i] = data[offset + i];
                }

                this.tail += size;
                this.remain -= size;
            }
            else
            {
                //如果需要从缓冲区开始重新装数据
                //先将剩余的空间装满
                for (int i = 0; i < this.remain; i++)
                {
                    this.buffer[tail + i] = data[offset + i];
                }

                //再将装不下的数据装到缓冲区开头
                for (int i = 0; i < size - this.remain; i++)
                {
                    this.buffer[i] = data[offset + this.remain + i];
                }

                this.tail = size - this.remain;
                this.remain = this.size - this.tail;
            }
        }

        public void Pop(byte[] data, int offset, int size)
        {
            for (int i = 0; i < size; i++)
            {
                data[offset + i] = this.buffer[(this.head + i) % this.size];
            }

            this.head = (this.head + size) % this.size;
        }

    }
}
