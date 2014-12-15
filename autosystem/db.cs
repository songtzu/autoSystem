using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.IO;





/*
 * 用【】框的字符为可以调节的字符。
 * 
 * 1.0出现上穿信号在A交易日，此后连续【N】个交易日，【最高价】为B交易日，B交易日比A交易日上涨超过10%，买入。
 * 止损：比买入价下跌10%卖出。
 * 赢利平仓：与最高价相比，出现10%的下跌，卖出。
 * 
 * 
 *
 * 1.1出现上穿信号在A交易日，此后连续【N】个交易日，【第A+N个交易日】为B交易日，B交易日比A交易日上涨超过10%，买入。
 * 止损：比买入价下跌10%卖出。
 * 赢利平仓：与最高价相比，出现10%的下跌，卖出。
 * 此方案为最简化方案。
 * 
 *  
 * 1.2出现上穿信号在A交易日，此后连续【N】个交易日，【最高价】为B交易日，B交易日比A交易日上涨超过10%，买入。
 * 止损：比买入价下跌10%卖出。
 * 赢利平仓：出现连续四个交易日下跌，或者四个以内交易日相比最高价格，下跌幅度超过15%。
 * 
 * 
 * 1.1.2出现上穿信号在A交易日，此后连续【N】(20)个交易日，【第A+N(20)个交易日】为B交易日，B交易日比A交易日上涨超过15%，买入。
 * 止损：比买入价下跌10%卖出。
 * 赢利平仓：与最高价相比，出现10%的下跌，卖出。
 * 此方案为1.1的过滤版本，对于买入的限制增多。。
 * 
 * 
 * 
 * 1.1.3出现上穿信号在A交易日，此后连续【N】(20)个交易日，【第A+N(20)个交易日】为B交易日，B交易日比A交易日上涨超过20%，买入。
 * 止损：比买入价下跌10%卖出。
 * 赢利平仓：与最高价相比，出现6%的下跌，卖出。
 * 此方案为1.1.2改版。
 * 
 * 
 * 
 */

namespace Write2SQLServer
{





    class Program
    {

        static string strConnection = "Data Source=.;Initial Catalog=StockTrading;Integrated Security=True";
        static SqlConnection con=new SqlConnection(strConnection);
        static SqlConnection con2 = new SqlConnection(strConnection);
        static SqlCommand sqlcmd;
        
        static void Main(string[] args)
        {
            StreamReader sr;
            con.Open();
            con2.Open();
            sqlcmd = new SqlCommand("", con);
          
            foreach (string filename in Directory.GetFiles("..\\..\\files", "*.txt", SearchOption.AllDirectories))
            {
                Console.WriteLine(filename);
                string path = filename.Substring(filename.Length - 10, 6);
                writOrignalData2DB(filename);

            }
          

            //求均值。需要注意数据库是否有该均值的列

            sr = new StreamReader("..\\..\\shanghai.txt");
            string shcode;
            while ((shcode = sr.ReadLine()) != null)
            {
                calAverage(30, shcode);
                calAverage(60, shcode);
            }


            sr = new StreamReader("..\\..\\shenzhen.txt");
          //  string shcode;
            while ((shcode = sr.ReadLine()) != null)
            {
                calAverage(30, shcode);
                calAverage(60, shcode);
            }



            /*  
                     foreach (string filename in Directory.GetFiles("..\\..\\files", "*.cvs", SearchOption.AllDirectories))
                     {
                         Console.WriteLine(filename);
                         string path = filename.Substring(filename.Length - 10, 6); 
                         calAverage(30, path);
                         calAverage(60, path);
            
                     }
                           
         objConnection = new OleDbConnection(strConnection);  //建立连接
         objConnection.Open();  //打开连接
         foreach (string filename in Directory.GetFiles("..\\..\\files", "*.cvs", SearchOption.AllDirectories))
         {
             Console.WriteLine(filename);
             string path = filename.Substring(filename.Length - 10, 6);
             //     CountResult(path, 30, 60);
             CountResult112(path, 30, 60);
         }
         */

        }

        static void writOrignalData2DB(string filename)
        {

            string strSQL = "";
            //OleDbCommand sqlcmd = new OleDbCommand(strSQL, objConnection);  //sql语句
            sqlcmd.CommandText = strSQL;
            string path = filename.Substring(filename.Length - 10, 6);
            StreamReader sr = new StreamReader(filename);
            string text;
      
            while ((text = sr.ReadLine()) != null)
            {
 
                Console.WriteLine(text);
                string[] strline = text.Split(';');
                if (strline[1].Equals("-"))
                {
                    continue;
                }
                strSQL = "insert into t_data([t_date],[open],[high],[low],[close],[code]) values('";
                strSQL += strline[0];
                strSQL += "',";
                strSQL += Convert.ToDouble(strline[1]);
                strSQL += ",";
                strSQL += Convert.ToDouble(strline[2]);
                strSQL += ",";
                strSQL += Convert.ToDouble(strline[3]);
                strSQL += ",";
                strSQL += Convert.ToDouble(strline[4]);
             

                strSQL += ",'";
                strSQL += path;
                strSQL += "')";
                sqlcmd.CommandText = strSQL;
                int n = sqlcmd.ExecuteNonQuery();              //执行查询

            } 

        }




        /* select avg([close]) from (select top 3
         * [close] from k_data where code='000001' and t_date<=#14-03-03# order by t_date desc);
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kname">均值名称</param>
        /// <param name="scode">代码编号</param>
        static void calAverage(int kname, string scode)
        {
 
            string strCom = "Select [t_date],[close] from t_data where code='" + scode + "' order by t_date desc";
            sqlcmd.CommandText = strCom;
            SqlCommand cmdWrite = new SqlCommand("", con2);
    
            try
            {
                SqlDataReader reader = sqlcmd.ExecuteReader(); //执行command并得到相应的DataReader执行SQL，返回一个“流”
                //下面把得到的值赋给tempnote对象
                while (reader.Read())
                {
                    string time = reader["t_date"].ToString();
                    string strAvgSql = "select avg([close]) from (select top " + kname.ToString() + " [close] from t_data where code='" + scode + "' and t_date<='" + time + "' order by t_date desc) as [close]";

                    cmdWrite.CommandText = strAvgSql;
                    decimal avg = Convert.ToDecimal(cmdWrite.ExecuteScalar());
                    string insertAvg = "update t_data set k" + kname.ToString() + "=" + avg + " where code='" + scode + "' and t_date='" + time + "'"; 
                    cmdWrite.CommandText = insertAvg;
                     int line =cmdWrite.ExecuteNonQuery();
                    //= writeCommand.ExecuteNonQuery();
                     Console.WriteLine("average date is " + avg.ToString() + "   影响 " + line.ToString());
                }
                reader.Close();
            }
            catch (Exception e)
            {
                //throw(new Exception("数据库出错:" + e.Message)) ;
            }
        }





        static void CountResult(string scode, int klow, int klarge)
        {
            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //建立连接
            //objConnection.Open();  //打开连接
            //int count = 0;
            //string strCom = "Select * from k_data  where code='" + scode + "' order by t_date ";
            //OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            //OleDbCommand writeCommand = new OleDbCommand(strCom, objConnection);
            //OleDbDataReader reader;
            //try
            //{
            //    reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
            //    //下面把得到的值赋给tempnote对象
            //    while (reader.Read())
            //    {
            //        count++;
            //        string time = reader["t_date"].ToString();
            //        Console.WriteLine(time + "    " + scode);
            //        GetBenifit(time, scode);
            //        //    GetBenifit();
            //    }
            //    reader.Close();

            //}
            //catch (Exception e)
            //{
            //    //throw(new Exception("数据库出错:" + e.Message)) ;
            //}
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="scode"></param>
        static void GetBenifit(string time, string scode)
        {
        /*    decimal k30N_1 = 0, k60N_1 = 0, k30N = 0, k60N = 0;
            decimal closeN = 0;//信号量当天价
            decimal closeN_10 = 0;//出现信号量之后的连续十天的最后一天收尾
            decimal closeN_N10_high = 0;//出现信号量到此后第十个交易日中出现的最大值。
            decimal buy = 0;
            decimal sell = 0;
            string buydate = time;//买入日期。
            decimal top = 0;
             
            //N-1天。
            string strCom = "Select top 2 * from k_data  where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
            if (reader.Read())
            {
                k30N_1 = (decimal)reader["k30"];
                k60N_1 = (decimal)reader["k60"];

                if (reader.Read())
                {
                    closeN = (decimal)reader["close"];
                    k30N = (decimal)reader["k30"];
                    k60N = (decimal)reader["k60"];
                }
            }
            reader.Close();

            if ((k30N_1 < k60N_1) && (k30N >= k60N))//信号量出现。
            {

                //读取之后十天的交易状况
                strCom = "Select top 10 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ";
                //  myCommand = new OleDbCommand(strCom, objConnection);
                myCommand.CommandText = strCom;
                reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                while (reader.Read())
                {

                    closeN_10 = (decimal)reader["close"];
                    buydate = reader["t_date"].ToString();
                }
                top = closeN_10;//最高价初始化为买入价。
                reader.Close();
                if (closeN_10 >= (decimal)1.1 * closeN)//买入信号
                {
                    buy = closeN_10;
                    //计算卖出。
                    strCom = "Select   * from k_data  where code='" + scode + "' and t_date>=#" + buydate + "# order by t_date   ";
                    // myCommand = new OleDbCommand(strCom, objConnection);
                    myCommand.CommandText = strCom;
                    reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                    while (reader.Read())
                    {
                        decimal p = (decimal)reader["close"];
                        if (p > top)
                        {
                            top = p;
                        }
                        if (p < top * (decimal)0.9)
                        {
                            sell = p;
                            //盈利出局。
                            //写日志文件，记录code,buy,sell
                            string selltime = reader["t_date"].ToString();
                            string txt = scode + "," + time + ",";
                            txt += buy.ToString() + "," + selltime + "," + sell.ToString();
                            WritToLog(txt);
                            break;
                        }
                    }
                }

            }
            else //失败。
            {
            }
            reader.Close();
            myCommand.Dispose();
            //objConnection.Close();
         * */
        }


        static void WritToLog(string txt)
        {
            Console.WriteLine(txt);
            StreamWriter sw = File.AppendText("D:\\resultlog1_2_part2.txt");
            sw.WriteLine(txt);
            sw.Close();
        }












        /// <summary>
        /// 采用算法1.1.2
        /// 采用更加严谨的算法来捕获较大的涨幅
        /// 问题所在：20%的涨幅是不小的行情了，很多人在买入上涨20%之后都会出货。如何更加快速的在此种情况止损平仓是个问题。
        /// </summary>
        /// <param name="scode"></param>
        /// <param name="klow"></param>
        /// <param name="klarge"></param>
        static void CountResult112(string scode, int klow, int klarge)
        {
            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //建立连接
            //objConnection.Open();  //打开连接
            //    int count = 0;
         /*   string strCom = "Select * from k_data  where code='" + scode + "' order by t_date ";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbCommand writeCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            try
            {
                reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                //下面把得到的值赋给tempnote对象
                while (reader.Read())
                {
                    //     count++;
                    string time = reader["t_date"].ToString();
                    //    Console.WriteLine(time + "    " + scode);
                    WriteBenifit112(time, scode);
                    //    GetBenifit();
                }
                reader.Close();

            }
            catch (Exception e)
            {
                //throw(new Exception("数据库出错:" + e.Message)) ;
            }
          * */
        }


        /// <summary>
        /// 采用算法1.1.2
        /// 采用更加严谨的算法来捕获较大的涨幅
        /// 问题所在：20%的涨幅是不小的行情了，很多人在买入上涨20%之后都会出货。如何更加快速的在此种情况止损平仓是个问题。
        /// </summary>
        /// <param name="time"></param>
        /// <param name="scode"></param>
        static void WriteBenifit112(string time, string scode)
        {
            /*
            decimal k30N_1 = 0, k60N_1 = 0, k30N = 0, k60N = 0;
            decimal closeN = 0;//信号量当天价
            decimal closeN_10 = 0;//出现信号量之后的连续十天的最后一天收尾
            decimal closeN_N10_high = 0;//出现信号量到此后第十个交易日中出现的最大值。
            decimal buy = 0;
            decimal sell = 0;
            string buydate = time;//买入日期。
            decimal top = 0;

            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //建立连接
            //objConnection.Open();  //打开连接
            //N-1天。
            string strCom = "Select top 2 * from k_data  where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
            if (reader.Read())
            {
                k30N_1 = (decimal)reader["k30"];
                k60N_1 = (decimal)reader["k60"];

                if (reader.Read())
                {
                    closeN = (decimal)reader["close"];
                    k30N = (decimal)reader["k30"];
                    k60N = (decimal)reader["k60"];
                }
            }
            reader.Close();

            if ((k30N_1 < k60N_1) && (k30N >= k60N))//信号量出现。
            {

                //读取之后十天的交易状况
                //strCom = "Select top 20 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ";
                strCom = "SELECT top 1 * FROM  (Select top 20 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ) order by t_date desc";
                //  myCommand = new OleDbCommand(strCom, objConnection);
                myCommand.CommandText = strCom;
                reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                while (reader.Read())
                {
                    closeN_10 = (decimal)reader["close"];
                    buydate = reader["t_date"].ToString();

                }

                top = closeN_10;//最高价初始化为买入价。
                reader.Close();
                if (closeN_10 >= (decimal)1.15 * closeN)//买入信号
                {
                    buy = closeN_10;
                    //计算卖出。
                    strCom = "Select   * from k_data  where code='" + scode + "' and t_date>=#" + buydate + "# order by t_date   ";
                    // myCommand = new OleDbCommand(strCom, objConnection);
                    myCommand.CommandText = strCom;
                    reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                    while (reader.Read())
                    {
                        decimal p = (decimal)reader["close"];
                        if (p > top)
                        {
                            top = p;
                        }
                        if (p < top * (decimal)0.9)
                        {
                            sell = p;
                            //盈利出局。
                            //写日志文件，记录code,buy,sell
                            string selltime = reader["t_date"].ToString();
                            string txt = scode + "," + time + ",";
                            txt += buy.ToString() + "," + selltime + "," + sell.ToString();
                            WritToLog(txt);
                            break;
                        }
                    }
                }

            }
            else //失败。
            {
            }
            reader.Close();
            myCommand.Dispose();
            //   objConnection.Close();
             * 
             * */
        }































        /// <summary>
        /// 采用算法1.1.3
        /// 采用更加严谨的算法来捕获较大的涨幅
        /// 问题所在：20%的涨幅是不小的行情了，很多人在买入上涨20%之后都会出货。如何更加快速的在此种情况止损平仓是个问题。
        /// </summary>
        /// <param name="scode"></param>
        /// <param name="klow"></param>
        /// <param name="klarge"></param>
        static void CountResult113(string scode, int klow, int klarge)
        {
            /*
            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //建立连接
            //objConnection.Open();  //打开连接
            int count = 0;
            string strCom = "Select * from k_data  where code='" + scode + "' order by t_date ";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbCommand writeCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            try
            {
                reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                //下面把得到的值赋给tempnote对象
                while (reader.Read())
                {
                    count++;
                    string time = reader["t_date"].ToString();
                    Console.WriteLine(time + "    " + scode);
                    WriteBenifit113(time, scode);
                    //    GetBenifit();
                }
                reader.Close();

            }
            catch (Exception e)
            {
                //throw(new Exception("数据库出错:" + e.Message)) ;
            }
             * */
        }


        /// <summary>
        /// 采用算法1.1.3
        /// 采用更加严谨的算法来捕获较大的涨幅
        /// 问题所在：20%的涨幅是不小的行情了，很多人在买入上涨20%之后都会出货。如何更加快速的在此种情况止损平仓是个问题。
        /// </summary>
        /// <param name="time"></param>
        /// <param name="scode"></param>
        static void WriteBenifit113(string time, string scode)
        {
            /*
            decimal k30N_1 = 0, k60N_1 = 0, k30N = 0, k60N = 0;
            decimal closeN = 0;//信号量当天价
            decimal closeN_10 = 0;//出现信号量之后的连续十天的最后一天收尾
            decimal closeN_N10_high = 0;//出现信号量到此后第十个交易日中出现的最大值。
            decimal buy = 0;
            decimal sell = 0;
            string buydate = time;//买入日期。
            decimal top = 0;

            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //建立连接
            //objConnection.Open();  //打开连接
            //N-1天。
            string strCom = "Select top 2 * from k_data  where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
            if (reader.Read())
            {
                k30N_1 = (decimal)reader["k30"];
                k60N_1 = (decimal)reader["k60"];

                if (reader.Read())
                {
                    closeN = (decimal)reader["close"];
                    k30N = (decimal)reader["k30"];
                    k60N = (decimal)reader["k60"];
                }
            }
            reader.Close();

            if ((k30N_1 < k60N_1) && (k30N >= k60N))//信号量出现。
            {

                //读取之后十天的交易状况
                strCom = "Select top 20 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ";
                //  myCommand = new OleDbCommand(strCom, objConnection);
                myCommand.CommandText = strCom;
                reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                while (reader.Read())
                {

                    closeN_10 = (decimal)reader["close"];
                    buydate = reader["t_date"].ToString();
                }
                top = closeN_10;//最高价初始化为买入价。
                reader.Close();
                if (closeN_10 >= (decimal)1.15 * closeN)//买入信号
                {
                    buy = closeN_10;
                    //计算卖出。
                    strCom = "Select   * from k_data  where code='" + scode + "' and t_date>=#" + buydate + "# order by t_date   ";
                    // myCommand = new OleDbCommand(strCom, objConnection);
                    myCommand.CommandText = strCom;
                    reader = myCommand.ExecuteReader(); //执行command并得到相应的DataReader
                    while (reader.Read())
                    {
                        decimal p = (decimal)reader["close"];
                        if (p > top)
                        {
                            top = p;
                        }
                        if (p < top * (decimal)0.9)
                        {
                            sell = p;
                            //盈利出局。
                            //写日志文件，记录code,buy,sell
                            string selltime = reader["t_date"].ToString();
                            string txt = scode + "," + time + ",";
                            txt += buy.ToString() + "," + selltime + "," + sell.ToString();
                            WritToLog(txt);
                            break;
                        }
                    }
                }

            }
            else //失败。
            {
            }
            reader.Close();
            myCommand.Dispose();
            //   objConnection.Close();
             *  * */
        } 
            

    }





}
