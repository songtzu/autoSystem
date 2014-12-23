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
    /// ���ݿ�����࣬���ǵ�Ч�ʺ����ӳصĿ������⣬�ڴ�����Ϊ����ģʽ��
    /// �������߳�unsafe���⡣�������п����ٿ��ǡ�
    /// </summary>
    class DBManager
    {
        private DBManager() {
            objConnection = new OleDbConnection(strConnection);  //��������
            objConnection.Open();  //������ 
            sqlcmd = new OleDbCommand("", objConnection);  //sql���

        }
        private static void initconfig()
        {
            //if (isCompactDB++ >= 0)
            //{
            //    isCompactDB = 0;
            //    FileInfo fileInfo = new FileInfo("..\\..\\db_data.mdb");
            //    //  return fileInfo.Length;
            //   // long len = fileInfo.Length / 1024 / 1024;
            //    if ((fileInfo.Length / 1024 / 1024)>100)//������ݿ��ļ��Ƿ񳬹�1G�����������ִ��compact��
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
                objConnection = new OleDbConnection(strConnection);  //��������
                objConnection.Open();  //������ 
                sqlcmd = new OleDbCommand("", objConnection);  //sql���

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
                    int n = sqlcmd.ExecuteNonQuery();              //ִ�в�ѯ
                    
                }

            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }

        }


        /// <summary>
        /// ÿ��ִ��һ���غϴ����㣬��������ĳ�������ն�Ӧ�ľ��ߡ��ɽ��������߳�����֮��ִ�С�
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
                int n = sqlcmd.ExecuteNonQuery();              //ִ�в�ѯ 

            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        /// <summary>
        /// ������ݿ�������ݣ��������ڣ����򷵻ؿ�
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
                OleDbDataReader reader = sqlcmd.ExecuteReader(); //ִ��command���õ���Ӧ��DataReaderִ��SQL������һ��������
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
        /// <param name="kname">��ֵ����</param>
        /// <param name="scode">������</param>
        static void DoAverage(int kname, string scode)
        {
            initconfig();
            int count = 0;
            //�˴���Ҫ�жϸ��ֶ�Ϊ�ա�
            string strCom = "Select * from k_data  where code='" + scode + "' and k" + kname.ToString() + " is NULL order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbCommand writeCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            try
            {
                reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
                //����ѵõ���ֵ����tempnote����
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
            if (!File.Exists(mdbPath)) //������ݿ��Ƿ��Ѵ���
            {
                throw new Exception("Ŀ�����ݿⲻ����,�޷�ѹ��");
            }
            //������ʱ���ݿ������
            string temp = DateTime.Now.ToString() + ".bak";//�Զ�������
            temp = mdbPath.Substring(0, mdbPath.LastIndexOf("\\") + 1) + temp;
            //������ʱ���ݿ�������ַ���
            string destconnection = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + temp;
            //����Ŀ�����ݿ�������ַ���
            string srcconnection = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbPath;
            //����һ��JetEngineClass�����ʵ��
            JRO.JetEngineClass jt = new JRO.JetEngineClass();
            //ʹ��JetEngineClass�����CompactDatabase����ѹ���޸����ݿ�
            jt.CompactDatabase(srcconnection, destconnection);
            //������ʱ���ݿ⵽Ŀ�����ݿ�(����)
            File.Copy(temp, mdbPath, true);
            //���ɾ����ʱ���ݿ�
            File.Delete(temp);
        }









    }
}
