using System;
using System.Collections.Generic;
 
using System.Text; 
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
namespace autosystem
{
    /*
     * 
     * http://www.google.com.hk/finance/historical?q=600316&hl=zh-CN&ei=eBAkU_DjGI-ukgWhKQ&startdate=Jan+1%2C+1990&enddate=2014-03-14&start=200&num=200
     * 
     * 
     * 
     * 上海SHA
     * http://www.google.com.hk/finance/historical?q=SHA%3A600006&gl=cn&ei=AhMkU-jNPMfYkgXZMw&startdate=Jan+1%2C+1990&enddate=2014-03-14&start=200&num=200
     * 
     * 
     * 深圳  代码
     * SHE
     * 
     * 
     * http://www.google.com.hk/finance/historical?q=SHE%3A000001&hl=zh-CN&gl=cn&ei=LxIkU_jILs_VkAXenQE&startdate=Jan+1%2C+1990&enddate=2014-03-14&start=200&num=200
     */
    public class DownloadGGData
    {

        public static string m_configpath = System.IO.Directory.GetCurrentDirectory();
        static string m_downloadfilename = "download";
        static bool m_isRelease = false;
        /// <summary>
        /// 构造函数
        /// </summary>
        public DownloadGGData(bool isRelease)
        {
            m_isRelease = isRelease;
            InitDownloader();
        }

        /// <summary>
        /// 初始化下载器类的配置信息
        /// </summary>
        /// <returns></returns>
        int InitDownloader()
        {
            int retVal = 0;
            if (!System.IO.Directory.Exists(m_configpath + "\\" + m_downloadfilename))
            {
                System.IO.Directory.CreateDirectory(m_configpath + "\\" + m_downloadfilename);
            }
            return retVal;
        }







        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="s">开始</param>
        /// <param name="e">结束</param>
        /// <returns></returns> 
        public string GetValueBetween(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }



        string GetContent(string str)
        {
            string resultwithnotag = GetValueBetween(str, "<td  class=\"lm\">", "</table>");
            if (resultwithnotag.Equals(""))
            {
            }
            string repalce = resultwithnotag.Replace("<td  class=\"rgt\">", ";");
            string thired = repalce.Replace("<td  class=\"rgt rm\">", ";");
            string final = thired.Replace("\r\n", "");
            string outf = final.Replace("<tr><td  class=\"lm\">", "\r\n");
            return outf;
        }



        void WritToLog(string txt, string filename)
        {
            if (!File.Exists(filename))
            {
                StreamWriter sw = File.CreateText(filename);
                sw.WriteLine(txt);
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.AppendText(filename);
                sw.WriteLine(txt);
                sw.Close();
            }
        }




        /// <summary>
        /// 下载器线程主函数
        /// </summary>
        /// <param name="isRelease">true：部署环境，false：测试环境</param>
        public void DownloadThread()
        {
             DBManager.TaskAverageManager();
            //初始化一个历史时间戳，当前时间和时间戳不等时，执行下载任务。否则休眠一小时之后询问。
            string currentTime = Convert.ToDateTime(DateTime.Parse("2010-10-10")).ToShortDateString();
            while (true)
            {

                //周六周日跳过，周一到周五，下午四点开始运行。
                int dd = (int)DateTime.Now.DayOfWeek;
                if (2 == (int)DateTime.Now.DayOfWeek || 6 == (int)DateTime.Now.DayOfWeek)
                {
                    currentTime = DateTime.Now.ToShortDateString();
                    Console.WriteLine("周末程序休眠");
                    Thread.Sleep(1000 * 60);
                    continue;
                }
                else {
                    //if (DateTime.Now.Hour < 16)
                    //{
                     
                    //    /////测试使用
                    //    Console.WriteLine("未到时间点程序休眠");
                    //    currentTime = DateTime.Now.ToShortDateString();
                    //    Thread.Sleep(1000 * 10);
                    //    continue;
                    //}
                }

                if (currentTime != DateTime.Now.ToShortDateString())
                {

                    string strDate = DateTime.Now.ToShortDateString();//DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();

                    //从数据库中读取今天是否已经有这个code对应的交易数据了。如果有，无需下载。
                    StreamReader sr = new StreamReader("..\\..\\shanghai.txt");
                    bool jumped = false;
                    string shcode;
                    while ((shcode = sr.ReadLine()) != null)
                    {
                        //测试数据库，用来跳过某些
                        if (!shcode.Equals("600887") && !jumped)
                        {
                            continue;
                        }
                        else
                        {
                            //  continue;
                            jumped = true;
                        }

                        Console.WriteLine(shcode);

                        string lastData = DBManager.isStockDataLoaded(shcode);

                        if (lastData == "")
                        {

                            Console.WriteLine(shcode + "begin download");
                            int npage = 0;
                            string parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHA%3A{0}&gl=cn&ei=AhMkU-jNPMfYkgXZMw&startdate=Jan+1%2C+1990&enddate={1}&start={2}&num=200", shcode, strDate, npage);
                            int rtval = 0;
                            while ((rtval = DownHtmlUrl(parserurl, "", shcode)) != 0)
                            {

                                npage += rtval;
                                parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHA%3A{0}&gl=cn&ei=AhMkU-jNPMfYkgXZMw&startdate=Jan+1%2C+1990&enddate{1}&start={2}&num=200", shcode, strDate, npage);
                            }
                            //该代码对应的最早股票数据已经下载完成。将股票代码表中的该股票的代码数据是否完备设置为true.任何时候，在访问Google之前，都需要判断一下该股票数据是否完备，如果完备，则更新最新的数据即可。
                            DBManager.FinishedDownloadCertainStockData(shcode, "sh");
                        }
                        else
                        {
                            DateTime time = DateTime.Parse(lastData);
                            time = time.AddDays(1);
                            lastData = time.ToShortDateString();
                            Console.WriteLine(shcode + "finished download");
                            int npage = 0;
                            string parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHA%3A{0}&gl=cn&ei=AhMkU-jNPMfYkgXZMw&startdate={1}&enddate={2}&start={3}&num=200", shcode, lastData, strDate, npage);
                            int rtval = 0;
                            while ((rtval = DownHtmlUrl(parserurl, "", shcode)) != 0)
                            {
                                npage += rtval;
                                parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHA%3A{0}&gl=cn&ei=AhMkU-jNPMfYkgXZMw&startdate={1}&enddate{2}&start={3}&num=200", shcode, lastData, strDate, npage);
                            }
                            DBManager.FinishedDownloadCertainStockData(shcode, "sh");
                        }

                    }

                    sr = new StreamReader("..\\..\\shenzhen.txt");
                    string szcode;
                    while ((szcode = sr.ReadLine()) != null)
                    {

                        //if (!szcode.Equals("000517") && !jumped)
                        //{
                        //    continue;
                        //}
                        //else
                        //{
                        //    jumped = true;
                        //}

                        string lastData = DBManager.isStockDataLoaded(szcode);

                        if (lastData == "")
                        {
                            Console.WriteLine(szcode + "begin download");
                            int npage = 0;
                            string parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHE%3A{0}&hl=zh-CN&gl=cn&ei=LxIkU_jILs_VkAXenQE&startdate=Jan+1%2C+1990&enddate={1}&start={2}&num=200", szcode, strDate, npage);
                            int rtval = 0;
                            while ((rtval = DownHtmlUrl(parserurl, "", szcode)) != 0)
                            {

                                npage += rtval;
                                parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHE%3A{0}&hl=zh-CN&gl=cn&ei=LxIkU_jILs_VkAXenQE&startdate=Jan+1%2C+1990&enddate={1}&start={2}&num=200", szcode, strDate, npage);
                            }
                            //该代码对应的最早股票数据已经下载完成。将股票代码表中的该股票的代码数据是否完备设置为true.
                            DBManager.FinishedDownloadCertainStockData(szcode, "sz");
                        }
                        else
                        {
                            DateTime time = DateTime.Parse(lastData);
                            time = time.AddDays(1);
                            lastData = time.ToShortDateString();
                            Console.WriteLine(szcode + "finished download");
                            int npage = 0;
                            string parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHE%3A{0}&hl=zh-CN&gl=cn&ei=LxIkU_jILs_VkAXenQE&startdate={1}&enddate={2}&start={3}&num=200", szcode, lastData, strDate, npage);
                            int rtval = 0;
                            while ((rtval = DownHtmlUrl(parserurl, "", szcode)) != 0/*!= -1*/)
                            {

                                npage += rtval;
                                parserurl = String.Format("http://www.google.com.hk/finance/historical?q=SHE%3A{0}&hl=zh-CN&gl=cn&ei=LxIkU_jILs_VkAXenQE&startdate={1}&enddate={2}&start={3}&num=200", szcode, lastData, strDate, npage);
                            }
                            DBManager.FinishedDownloadCertainStockData(szcode, "sz");
                        }

                    }


                    //计算均值。
                    DBManager.TaskAverageManager();

                }
                //休眠一小时之后重启执行。
                currentTime = DateTime.Now.ToShortDateString();
                Thread.Sleep(1000 * 60);


            }



        }








        #region 下载指定的网页，并保存到指定位置
        /// <summary>
        /// 下载指定的网页，并保存到指定位置
        /// </summary>
        /// <param name="strUrl">网页URL地址</param>
        /// <param name="savePath">保存路径，以htm保存,暂时没用</param> 
        int DownHtmlUrl(string strUrl, string savePath, string scode)
        {
            int bnext = 0;

            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string encoding = response.CharacterSet;//获取该网页的编码方式 
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string status = ((HttpWebResponse)response).StatusCode.ToString();
                Console.WriteLine(status);
                string strHtmlContent = reader.ReadToEnd();
                response.Close();
                 
                string resultwithnotag = GetValueBetween(strHtmlContent, "<td  class=\"lm\">", "</table>");
                if (resultwithnotag.Equals(""))
                {
                    return 0;
                }
                string repalce = resultwithnotag.Replace("<td  class=\"rgt\">", ";");
                string thired = repalce.Replace("<td  class=\"rgt rm\">", ";");
                string final = thired.Replace("\n", "");
                string outf = final.Replace("<tr><td  class=\"lm\">", "\r\n");

                //string filepath = String.Format("{0}\\{1}\\{2}.txt", m_configpath, m_downloadfilename, scode);
                //TODO
                ///此处应该直接解析完写入数据库。
                //WritToLog(outf, filepath);
                DBManager.InsertDealRecord(outf, scode);
                string[] cc = Regex.Split(outf, "\n", RegexOptions.IgnoreCase);//outf.Split("\r");
                if (cc.Length == 0)
                {
                    bnext = -1;
                }
                else
                {
                    bnext = cc.Length;
                }

            }
            catch (Exception exp)
            {

                Console.WriteLine(exp.Message);
                return 0;
            }
            return bnext;
        } 
        #endregion









    }//for class.


}//for namespace





 
 