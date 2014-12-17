using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace autosystem
{
    class WeightedResult
    {
        /// <summary>
        /// ����ָ���ļ���Ȩ�ؽ��������һ��resultд��ͬ��.result�ļ��С�
        /// </summary>
        /// <param name="filename"></param>
        public static void GivenSuggestion(string filename){


             //000001,   1991/3/25 0:00:00   ,1.6    ,1991/5/2 0:00:00   ,1.43
           // string m_out;
        
            //string m_path =filename.Substring(0,filename.LastIndexOf("\\"));
            string m_in = filename;//m_path + "all.txt";
           
            DateTime tdate = DateTime.Parse("2012/11/11");
            tdate = tdate.AddDays(-10);


            float rate = 0;
            //StreamReader sr = new StreamReader("..\\..\\204\\all.txt", Encoding.GetEncoding("GB2312"));///StreamReader sr = new StreamReader("TestFile.txt",Encoding.GetEncoding("GB2312"))
            StreamReader sr = new StreamReader(filename);///StreamReader sr = new StreamReader("TestFile.txt",Encoding.GetEncoding("GB2312"))
            ///GBK
            //String line;
            //while ((line = sr.ReadLine()) != null) 
            //{
            //    string[] data = line.Split(',');
            //    float b = float.Parse(data[2]);
            //    float s = float.Parse(data[4]);
            //    rate += (s - b) / s;
            //}
            //WritToLog("sum is :" + rate.ToString());

            //600000,4.23,2002-6-24 0:00:00,3.64,2002-10-8 0:00:00,3.74
            String line;
            float closerate = 0;
            int count = 0;
            int days = 0;
            int countwindays = 0;//ӯ��ʱ��ƽ����������
            int wintimes = 0;//ӯ������
            DateTime lmt = DateTime.Parse("2010/01/01 0:00:00");
            DateTime lmtup = DateTime.Parse("2008/03/03 0:00:00");
            while ((line = sr.ReadLine()) != null)
            {


                string[] data = line.Split(',');

                float top = float.Parse(data[1]);
                float buy = float.Parse(data[3]);
                float sellclose = float.Parse(data[5]);
                DateTime buydate = DateTime.Parse(data[2]);
                DateTime selldate = DateTime.Parse(data[4]);
                //if(DateTime.Compare(buydate,lmt)>0&&DateTime.Compare(buydate,lmtup)<0)
                //{
                //    continue;
                //}
                if (DateTime.Compare(buydate, lmt) < 0)
                {
                    continue;
                }
                rate += (top * (float)0.9 - buy) / buy;
                closerate += (sellclose - buy) / buy;

                if (((top * (float)0.9 - buy) / buy) > 0.5)
                {
                    //    WritToLog(line);
                }

                if ((sellclose - buy) < 0)
                {
                    WritToLog(line, filename);
                }


                TimeSpan ts = selldate.Subtract(buydate);
                if ((top * (float)0.9 - buy) > 0)
                {
                    countwindays += ts.Days;
                    wintimes++;
                }
                days += ts.Days;
                count++;
            }
            WritToLog("����߼���   is :" + rate.ToString(), filename);
            float avgtop = (float)(rate / count);
            WritToLog("����߼���ƽ��ֵΪ   " + avgtop.ToString(), filename);

            WritToLog("����β�ۼ��� is :" + closerate.ToString(), filename);
            //avgtop = 0;
            float avgend = (float)(closerate / count);
            WritToLog("��β�ۼ���ƽ��ֵΪ   " + avgend.ToString(), filename);

            WritToLog("���� ����   " + count.ToString(), filename);

            WritToLog("��������   " + days.ToString(), filename);

            int avgdays = days / count;
            WritToLog("ƽ����������   " + avgdays.ToString(), filename);

            int opercount = 365 / avgdays;

            float yearsincome = ((float)opercount * avgtop);
            WritToLog("  ��߼���� �껯���   " + yearsincome.ToString(), filename);

            float endyearsincome = ((float)opercount * avgend);
            WritToLog("  ��β����� �껯���   " + endyearsincome.ToString(), filename);

            int avgwinholdsdays = (int)countwindays / wintimes;
            WritToLog("ӯ������" + wintimes.ToString() + "ƽ��������" + avgwinholdsdays.ToString(), filename);


        }




         


        static void WritToLog(string txt,string filename)
        {
            string file = filename.Substring(0, filename.LastIndexOf("\\")) + DateTime.Now.ToShortDateString() + ".txt";
            if (!File.Exists(file))
            {
                File.Create(file);
            }
            Console.WriteLine(txt);
            // StreamWriter sw = File.AppendText("..\\..\\204\\fenxi.txt");
            StreamWriter sw = File.AppendText(file);
            sw.WriteLine(txt);
            sw.Close();
        }











    }
}
