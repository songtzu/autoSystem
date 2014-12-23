using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

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
        private static void initconfig()
        {
            //if (isCompactDB++ >= 0)
            //{
            //    isCompactDB = 0;
            //    FileInfo fileInfo = new FileInfo("..\\..\\db_data.mdb");
            //    //  return fileInfo.Length;
            //   // long len = fileInfo.Length / 1024 / 1024;
            //    if ((fileInfo.Length / 1024 / 1024)>100)//检查数据库文件是否超过1G，如果超过，执行compact。
            //    {

            //        isInited = false;
            //        if (objConnection != null)
            //        {
            //            objConnection.Close();
            //        }
            //        if (conn_other != null)
            //        {
            //            conn_other.Close();
            //        }
            //        CompactDB();
            //        //compact db
            //        //
            //    }
            //}
       
            if (!isInited)
            {
                isInited = true;
                objConnection = new OleDbConnection(strConnection);  //建立连接
                objConnection.Open();  //打开连接 
                sqlcmd = new OleDbCommand("", objConnection);  //sql语句

                conn_other = new OleDbConnection(strConnection);
                conn_other.Open();
                cmd_other = new OleDbCommand("", conn_other);
            }
        }

        static private DBManager _instance = null;

        static string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
        static OleDbConnection objConnection;
        static OleDbConnection conn_other;
        static OleDbCommand sqlcmd;
        static OleDbCommand cmd_other;
        static string sqlInsert;
        static bool isInited = false;
       // static int isCompactDB = 0;
        
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
                Console.WriteLine("average" + shcode);
                DoAverage(30, shcode);
                DoAverage(60, shcode);
            }

            ///////////////////////////////////////////////////
            StreamReader sr2 = new StreamReader("..\\..\\shenzhen.txt");
            string szcode;
            while ((szcode = sr2.ReadLine()) != null)
            {
                Console.WriteLine("average sz" + szcode);
                DoAverage(30, szcode);
                DoAverage(60, szcode);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockid">stock id</param>
        /// <param name="group">sz or sh</param>
        public static void FinishedDownloadCertainStockData(string stockid,string group)
        {
            try
            {
                //insert into t_code([code],[market],[loaddata]) values('600000',sh,1')
                initconfig();

                sqlInsert = "insert into t_code([code],[market],[loaddata]) values('";
                sqlInsert += stockid;
                sqlInsert += "','";
                sqlInsert += group;
                sqlInsert += "',";
                sqlInsert += 1; 
                sqlInsert += ")";
                sqlcmd.CommandText = sqlInsert;
                int n = sqlcmd.ExecuteNonQuery();              //执行查询 

            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        /// <summary>
        /// 如果数据库存在数据，返回日期，否则返回空
        /// </summary>
        /// <param name="sStockNum"></param>
        /// <returns></returns>
        public static string isStockDataLoaded(string sStockNum)
        {
            //select top 1 k_data.* from k_data,t_code where t_code.code="600000" and t_code.code=k_data.code order by k_data.t_date desc
            try
            {
                initconfig();

                sqlInsert = string.Format("select top 1 k_data.* from k_data,t_code where t_code.code=\"{0}\" and t_code.code=k_data.code order by k_data.t_date desc", sStockNum);
                sqlcmd.CommandText = sqlInsert;
                OleDbDataReader reader = sqlcmd.ExecuteReader(); //执行command并得到相应的DataReader执行SQL，返回一个“流”
                if (reader.Read())
                {
                    string ret=reader["t_date"].ToString();
                    reader.Close();
                    return ret;
                }
                else {
                    reader.Close();
                    return "";
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                
                return "";
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
            initconfig();
            int count = 0;
            //此处需要判断该字段为空。
            string strCom = "Select * from k_data  where code='" + scode + "' and k" + kname.ToString() + " is NULL order by t_date desc";
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
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
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











        /// <summary> 
        /// MBD compact method (c) 2004 Alexander Youmashev 
        /// !!IMPORTANT!! 
        /// !make sure there's no open connections 
        ///    to your db before calling this method! 
        /// !!IMPORTANT!! 
        /// </summary> 
        /// <param name="connectionString">connection string to your db</param> 
        /// <param name="mdwfilename">FULL name 
        ///     of an MDB file you want to compress.</param> 
        //public static void CompactAccessDB( )
        //{ 
        //    object[] oParams; 
        //    //create an inctance of a Jet Replication Object 
        //    object objJRO =  Activator.CreateInstance(Type.GetTypeFromProgID("JRO.JetEngine")); 
        //    //filling Parameters array 
        //    //cnahge "Jet OLEDB:Engine Type=5" to an appropriate value 
        //    // or leave it as is if you db is JET4X format (access 2000,2002) 
        //    //(yes, jetengine5 is for JET4X, no misprint here)

        //    oParams = new object[] { connectionString, "Provider=Microsoft.Jet.OLEDB.4.0;Data" + " Source=..\\..\\db_data.mdb;Jet OLEDB:Engine Type=5" };
             
        //    //invoke a CompactDatabase method of a JRO object

        //    //pass Parameters array


        //    objJRO.GetType().InvokeMember("CompactDatabase",
        //        System.Reflection.BindingFlags.InvokeMethod,  null, objJRO,  oParams); 
        //    //database is compacted now 
        //    //to a new file C:\\tempdb.mdw 
        //    //let's copy it over an old one and delete it 
        //    System.IO.File.Delete(mdwfilename); 
        //    System.IO.File.Move("..\\..\\tempdb.mdb", mdwfilename); 
        //    //clean up (just in case) 
        //    System.Runtime.InteropServices.Marshal.ReleaseComObject(objJRO);

        //    objJRO = null;

        //}




        static public void CompactDB()
        {
            string mdbPath = "..\\..\\db_data.mdb";
            if (!File.Exists(mdbPath)) //检查数据库是否已存在
            {
                throw new Exception("目标数据库不存在,无法压缩");
            }
            //声明临时数据库的名称
            string temp = DateTime.Now.ToString() + ".bak";//自定义名称
            temp = mdbPath.Substring(0, mdbPath.LastIndexOf("\\") + 1) + temp;
            //定义临时数据库的连接字符串
            string destconnection = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + temp;
            //定义目标数据库的连接字符串
            string srcconnection = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbPath;
            //创建一个JetEngineClass对象的实例
            JRO.JetEngineClass jt = new JRO.JetEngineClass();
            //使用JetEngineClass对象的CompactDatabase方法压缩修复数据库
            jt.CompactDatabase(srcconnection, destconnection);
            //拷贝临时数据库到目标数据库(覆盖)
            File.Copy(temp, mdbPath, true);
            //最后删除临时数据库
            File.Delete(temp);
        }









    }
}
