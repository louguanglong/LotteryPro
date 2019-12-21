using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;

namespace LotteryPro
{
    /// <summary>
    /// 选号器类
    /// </summary>
    class Selector
    {
        private Random objRandom;//生成随机数对象

        Fath.BarcodeX barCode = new Fath.BarcodeX();

        /// <summary>
        /// 选择的号码（存储部分）兑奖
        /// </summary>
        public List<string[]> SelectedNums { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public Selector()
        {
            this.objRandom = new Random();
            this.SelectedNums = new List<string[]>();
        }

        /// <summary>
        /// 生成7个随机号码
        /// </summary>
        /// <returns></returns>
        public string[] CreateNum()
        {
            string[] numList = new string[7];
            for (int i = 0; i < 7; i++)
            {
                numList[i] = objRandom.Next(10).ToString();
            }
            return numList;
        }
        /// <summary>
        /// 生成指定组的随机号码
        /// </summary>
        /// <param name="count"></param>
        public void CreatGroupNums(int count)
        {
            SelectedNums.Clear();//首先清零
            for (int i = 0; i < count; i++)
            {
                SelectedNums.Add(CreateNum());//增加一组
            }
        }
        /// <summary>
        /// 获取打印的号码列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetPrintedNums()
        {
            List<string> numList = new List<string>();
            //拼接成带空格的字符串（为了打印好看）
            for (int i = 0; i < this.SelectedNums.Count; i++)
            {
                string printedNum = string.Empty;
                if (i < 9)
                {
                    printedNum = "第0" + (i + 1) + "组：";
                }
                else
                {
                    printedNum = "第" + (i + 1) + "组：";
                }
                for (int a = 0; a < this.SelectedNums[i].Length; a++)
                {
                    if (a == 5)//最后一个是特殊号，空格加大
                    {
                        printedNum += this.SelectedNums[i][a] + "    ";
                    }
                    else
                    {
                        printedNum += this.SelectedNums[i][a] + "  ";
                    }
                }
                numList.Add(printedNum);
            }
            return numList;
        }
        /// <summary>
        /// 保存所选号码
        /// </summary>
        public void Save(string serialNum)
        {
            DirectoryInfo dir = new DirectoryInfo("numList");
            if (!dir.Exists)
            {
                dir.Create();
            }
            string path = @"numList\" + serialNum + ".num";
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this.SelectedNums);
            fs.Close();
        }
        /// <summary>
        /// 打印彩票
        /// </summary>
        public void PrintLottery(System.Drawing.Printing.PrintPageEventArgs e, string serialNum, List<string> numList)
        {
            //生成条形码
            barCode.Text = serialNum;//条码数据
            barCode.Symbology = Fath.bcType.Code128;//设置条码格式
            barCode.ShowText = true;//同时显示文本           
            e.Graphics.DrawImage(barCode.Image(240, 50), new Point(20, 5));//画条形码

            //生成票据
            float left = 2; //打印区域的左边界
            float top = 70;//打印区域的上边界
            Font titlefont = new Font("仿宋", 10);//标题字体
            Font font = new Font("仿宋", 8);//内容字体            
            e.Graphics.DrawString("天津百万奖彩票中心", titlefont, Brushes.Blue, left + 20, top, new StringFormat());//打印标题
            //画一条分界线
            Pen pen = new Pen(Color.Green, 1);
            e.Graphics.DrawLine(pen, new Point((int)left - 2, (int)top + 20), new Point((int)left + (int)180, (int)top + 20));

            //循环打印选号
            for (int i = 0; i < numList.Count; i++)
            {
                e.Graphics.DrawString(numList[i], font, Brushes.Blue, left,
                    top + titlefont.GetHeight(e.Graphics) + font.GetHeight(e.Graphics) * i + 12, new StringFormat());
            }

            //画一条分界线           
            float topPoint = titlefont.GetHeight(e.Graphics) + font.GetHeight(e.Graphics) * (numList.Count) + 22;

            e.Graphics.DrawLine(pen, new Point((int)left - 2, (int)top + (int)topPoint),
                new Point((int)left + (int)180, (int)top + (int)topPoint));

            //打印时间
            string time = "购买时间：" + DateTime.Now.ToString("yyy-MM-dd  HH:mm:ss");
            e.Graphics.DrawString(time, font, Brushes.Blue, left, top + titlefont.GetHeight(e.Graphics)
                + font.GetHeight(e.Graphics) * (numList.Count + 1) + 12, new StringFormat());

            //二维码图片left和top坐标
            int qrcodetop = (int)(top + titlefont.GetHeight(e.Graphics) + font.GetHeight(e.Graphics) * (numList.Count + 3) + 12);
            int qrcodeleft = (int)left + 32;

            //生成二维码图片
            // Image bmp = QRcodeCreator.GetQRCodeBmp("http://www.xiketang.com/duijiang/query?id=" + serialNum);//直接添加流水号，将来从网站直接查询
            Image bmp = QRcodeCreator.GetQRCodeBmp("http://www.xiketang.com");
            //打印
            e.Graphics.DrawImage(bmp, new Point(qrcodeleft, qrcodetop));//不同的URL图片大小不同，可以根据需要调整left坐标
            e.Graphics.DrawString("扫描二维码可直接查询兑奖结果", font, Brushes.Blue, left, qrcodetop + bmp.Height + 10, new StringFormat());
        }     
    }
}
