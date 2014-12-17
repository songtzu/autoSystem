using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;

namespace autosystem
{
    /// <summary>
    /// 数据库管理类，考虑到效率和连接池的开闭问题，在此声明为单例模式。
    /// 可能有线程unsafe问题。待后面有空了再考虑。
    /// </summary>
    class DBManager
    {
        private DBManager() {
            objConnection = new OleDbConnection(strConnection);  //建立连接
            objConnection.Open();  //打开连接 
            sqlcmd = new OleDbCommand("", objConnection);  //sql语句

        }
        private static void initconfig(){
            if(!isInited){
                isInited = true;
                objConnection = new OleDbConnection(strConnection);  //建立连接
                objConnection.Open();  //打开连接 
                sqlcmd = new OleDbCommand("", objConnection);  //sql语句

            }
        }

        static private DBManager _instance = null;

        static string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
        static OleDbConnection objConnection;
        static OleDbCommand sqlcmd;
        static string sqlInsert;
        static bool isInited = false;
        public static DBManager getInstance() {
            if (_instance == null)
            {
                _instance = new DBManager();

            }
            return _instance;
        }


        public static void InsertDealRecord(string total, string code)
        {
            try
            {
                initconfig();
                string[] cc = Regex.Split(total, "\n", RegexOptions.IgnoreCase);//outf.Split("\r");
                foreach (string text in cc)
                {
                    string[] strline = text.Split(';');
                    if (strline[1].Equals("-"))
                    {
                        return;
                    }
                    sqlInsert = "insert into k_data([t_date],[open],[high],[low],[close],[code]) values('";
                    sqlInsert += strline[0];
                    sqlInsert += "',";
                    sqlInsert += Convert.ToDouble(strline[1]);
                    sqlInsert += ",";
                    sqlInsert += Convert.ToDouble(strline[2]);
                    sqlInsert += ",";
                    sqlInsert += Convert.ToDouble(strline[3]);
                    sqlInsert += ",";
                    sqlInsert += Convert.ToDouble(strline[4]);
                    sqlInsert += ",'";
                    sqlInsert += code;
                    sqlInsert += "')";
                    sqlcmd.CommandText = sqlInsert;
                    int n = sqlcmd.ExecuteNonQuery();              //执行查询
                }

            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }

        }


        /// <summary>
        /// 每天执行一个回合此运算，用来计算某个交易日对应的均线。可接在下载线程任务之后执行。
        /// </summary>
        public static void TaskAverageManager() {
            StreamReader sr = new StreamReader("..\\..\\shanghai.txt");
            string shcode;
            while ((shcode = sr.ReadLine()) != null)
            {
                DoAverage(30, shcode);
                DoAverage(60, shcode);
            }

            ///////////////////////////////////////////////////
            StreamReader sr2 = new StreamReader("..\\..\\shenzhen.txt");
            string shcode2;
            while ((shcode2 = sr2.ReadLine()) != null)
            {
                DoAverage(30, shcode2);
                DoAverage(60, shcode2);
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
        static void DoAverage(int kname, string scode)
        {

            int count = 0;
            //此处需要判断该字段为空。
            string strCom = "Select * from k_data  where code='" + scode +" and "+kname.ToString()+ "'is NULL order by t_date desc";
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
                    string strAvgSql = "select avg([close]) from (select top " + kname.ToString() + " [close] from k_data where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc)";

                    writeCommand.CommandText = strAvgSql;
                    decimal avg = Convert.ToDecimal(writeCommand.ExecuteScalar());



                    string insertAvg = "update k_data set k" + kname.ToString() + "=" + avg + " where code='" + scode + "' and t_date=#" + time + "#";
                    writeCommand.CommandText = insertAvg;
                    int line = writeCommand.ExecuteNonQuery();
                    Console.WriteLine("average date is " + avg.ToString() + "   " + count.ToString());

                }
                reader.Close();

            }
            catch  
            { 
            }
        }















     

        static void WritToLog(string txt)
        {
            if (!System.IO.Directory.Exists(DownloadGGData.m_configpath + "\\result\\"))
            {
                System.IO.Directory.CreateDirectory(DownloadGGData.m_configpath + "\\result\\");
            }
            string logfile =DownloadGGData.m_configpath+"\\result\\"+ DateTime.Now.ToShortDateString();
            if (!File.Exists(logfile)) {
                File.Create(logfile);
            }
            Console.WriteLine(txt);
            StreamWriter sw = File.AppendText(logfile);
            sw.WriteLine(txt);
            sw.Close();
        }




























    }
}
