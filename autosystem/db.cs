using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.IO;





/*
 * �á�������ַ�Ϊ���Ե��ڵ��ַ���
 * 
 * 1.0�����ϴ��ź���A�����գ��˺�������N���������գ�����߼ۡ�ΪB�����գ�B�����ձ�A���������ǳ���10%�����롣
 * ֹ�𣺱�������µ�10%������
 * Ӯ��ƽ�֣�����߼���ȣ�����10%���µ���������
 * 
 * 
 *
 * 1.1�����ϴ��ź���A�����գ��˺�������N���������գ�����A+N�������ա�ΪB�����գ�B�����ձ�A���������ǳ���10%�����롣
 * ֹ�𣺱�������µ�10%������
 * Ӯ��ƽ�֣�����߼���ȣ�����10%���µ���������
 * �˷���Ϊ��򻯷�����
 * 
 *  
 * 1.2�����ϴ��ź���A�����գ��˺�������N���������գ�����߼ۡ�ΪB�����գ�B�����ձ�A���������ǳ���10%�����롣
 * ֹ�𣺱�������µ�10%������
 * Ӯ��ƽ�֣����������ĸ��������µ��������ĸ����ڽ����������߼۸��µ����ȳ���15%��
 * 
 * 
 * 1.1.2�����ϴ��ź���A�����գ��˺�������N��(20)�������գ�����A+N(20)�������ա�ΪB�����գ�B�����ձ�A���������ǳ���15%�����롣
 * ֹ�𣺱�������µ�10%������
 * Ӯ��ƽ�֣�����߼���ȣ�����10%���µ���������
 * �˷���Ϊ1.1�Ĺ��˰汾������������������ࡣ��
 * 
 * 
 * 
 * 1.1.3�����ϴ��ź���A�����գ��˺�������N��(20)�������գ�����A+N(20)�������ա�ΪB�����գ�B�����ձ�A���������ǳ���20%�����롣
 * ֹ�𣺱�������µ�10%������
 * Ӯ��ƽ�֣�����߼���ȣ�����6%���µ���������
 * �˷���Ϊ1.1.2�İ档
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
          

            //���ֵ����Ҫע�����ݿ��Ƿ��иþ�ֵ����

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
                           
         objConnection = new OleDbConnection(strConnection);  //��������
         objConnection.Open();  //������
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
            //OleDbCommand sqlcmd = new OleDbCommand(strSQL, objConnection);  //sql���
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
                int n = sqlcmd.ExecuteNonQuery();              //ִ�в�ѯ

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
        static void calAverage(int kname, string scode)
        {
 
            string strCom = "Select [t_date],[close] from t_data where code='" + scode + "' order by t_date desc";
            sqlcmd.CommandText = strCom;
            SqlCommand cmdWrite = new SqlCommand("", con2);
    
            try
            {
                SqlDataReader reader = sqlcmd.ExecuteReader(); //ִ��command���õ���Ӧ��DataReaderִ��SQL������һ��������
                //����ѵõ���ֵ����tempnote����
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
                     Console.WriteLine("average date is " + avg.ToString() + "   Ӱ�� " + line.ToString());
                }
                reader.Close();
            }
            catch (Exception e)
            {
                //throw(new Exception("���ݿ����:" + e.Message)) ;
            }
        }





        static void CountResult(string scode, int klow, int klarge)
        {
            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //��������
            //objConnection.Open();  //������
            //int count = 0;
            //string strCom = "Select * from k_data  where code='" + scode + "' order by t_date ";
            //OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            //OleDbCommand writeCommand = new OleDbCommand(strCom, objConnection);
            //OleDbDataReader reader;
            //try
            //{
            //    reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
            //    //����ѵõ���ֵ����tempnote����
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
            //    //throw(new Exception("���ݿ����:" + e.Message)) ;
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
            decimal closeN = 0;//�ź��������
            decimal closeN_10 = 0;//�����ź���֮�������ʮ������һ����β
            decimal closeN_N10_high = 0;//�����ź������˺��ʮ���������г��ֵ����ֵ��
            decimal buy = 0;
            decimal sell = 0;
            string buydate = time;//�������ڡ�
            decimal top = 0;
             
            //N-1�졣
            string strCom = "Select top 2 * from k_data  where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
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

            if ((k30N_1 < k60N_1) && (k30N >= k60N))//�ź������֡�
            {

                //��ȡ֮��ʮ��Ľ���״��
                strCom = "Select top 10 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ";
                //  myCommand = new OleDbCommand(strCom, objConnection);
                myCommand.CommandText = strCom;
                reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
                while (reader.Read())
                {

                    closeN_10 = (decimal)reader["close"];
                    buydate = reader["t_date"].ToString();
                }
                top = closeN_10;//��߼۳�ʼ��Ϊ����ۡ�
                reader.Close();
                if (closeN_10 >= (decimal)1.1 * closeN)//�����ź�
                {
                    buy = closeN_10;
                    //����������
                    strCom = "Select   * from k_data  where code='" + scode + "' and t_date>=#" + buydate + "# order by t_date   ";
                    // myCommand = new OleDbCommand(strCom, objConnection);
                    myCommand.CommandText = strCom;
                    reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
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
                            //ӯ�����֡�
                            //д��־�ļ�����¼code,buy,sell
                            string selltime = reader["t_date"].ToString();
                            string txt = scode + "," + time + ",";
                            txt += buy.ToString() + "," + selltime + "," + sell.ToString();
                            WritToLog(txt);
                            break;
                        }
                    }
                }

            }
            else //ʧ�ܡ�
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
        /// �����㷨1.1.2
        /// ���ø����Ͻ����㷨������ϴ���Ƿ�
        /// �������ڣ�20%���Ƿ��ǲ�С�������ˣ��ܶ�������������20%֮�󶼻��������θ��ӿ��ٵ��ڴ������ֹ��ƽ���Ǹ����⡣
        /// </summary>
        /// <param name="scode"></param>
        /// <param name="klow"></param>
        /// <param name="klarge"></param>
        static void CountResult112(string scode, int klow, int klarge)
        {
            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //��������
            //objConnection.Open();  //������
            //    int count = 0;
         /*   string strCom = "Select * from k_data  where code='" + scode + "' order by t_date ";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbCommand writeCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            try
            {
                reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
                //����ѵõ���ֵ����tempnote����
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
                //throw(new Exception("���ݿ����:" + e.Message)) ;
            }
          * */
        }


        /// <summary>
        /// �����㷨1.1.2
        /// ���ø����Ͻ����㷨������ϴ���Ƿ�
        /// �������ڣ�20%���Ƿ��ǲ�С�������ˣ��ܶ�������������20%֮�󶼻��������θ��ӿ��ٵ��ڴ������ֹ��ƽ���Ǹ����⡣
        /// </summary>
        /// <param name="time"></param>
        /// <param name="scode"></param>
        static void WriteBenifit112(string time, string scode)
        {
            /*
            decimal k30N_1 = 0, k60N_1 = 0, k30N = 0, k60N = 0;
            decimal closeN = 0;//�ź��������
            decimal closeN_10 = 0;//�����ź���֮�������ʮ������һ����β
            decimal closeN_N10_high = 0;//�����ź������˺��ʮ���������г��ֵ����ֵ��
            decimal buy = 0;
            decimal sell = 0;
            string buydate = time;//�������ڡ�
            decimal top = 0;

            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //��������
            //objConnection.Open();  //������
            //N-1�졣
            string strCom = "Select top 2 * from k_data  where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
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

            if ((k30N_1 < k60N_1) && (k30N >= k60N))//�ź������֡�
            {

                //��ȡ֮��ʮ��Ľ���״��
                //strCom = "Select top 20 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ";
                strCom = "SELECT top 1 * FROM  (Select top 20 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ) order by t_date desc";
                //  myCommand = new OleDbCommand(strCom, objConnection);
                myCommand.CommandText = strCom;
                reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
                while (reader.Read())
                {
                    closeN_10 = (decimal)reader["close"];
                    buydate = reader["t_date"].ToString();

                }

                top = closeN_10;//��߼۳�ʼ��Ϊ����ۡ�
                reader.Close();
                if (closeN_10 >= (decimal)1.15 * closeN)//�����ź�
                {
                    buy = closeN_10;
                    //����������
                    strCom = "Select   * from k_data  where code='" + scode + "' and t_date>=#" + buydate + "# order by t_date   ";
                    // myCommand = new OleDbCommand(strCom, objConnection);
                    myCommand.CommandText = strCom;
                    reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
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
                            //ӯ�����֡�
                            //д��־�ļ�����¼code,buy,sell
                            string selltime = reader["t_date"].ToString();
                            string txt = scode + "," + time + ",";
                            txt += buy.ToString() + "," + selltime + "," + sell.ToString();
                            WritToLog(txt);
                            break;
                        }
                    }
                }

            }
            else //ʧ�ܡ�
            {
            }
            reader.Close();
            myCommand.Dispose();
            //   objConnection.Close();
             * 
             * */
        }































        /// <summary>
        /// �����㷨1.1.3
        /// ���ø����Ͻ����㷨������ϴ���Ƿ�
        /// �������ڣ�20%���Ƿ��ǲ�С�������ˣ��ܶ�������������20%֮�󶼻��������θ��ӿ��ٵ��ڴ������ֹ��ƽ���Ǹ����⡣
        /// </summary>
        /// <param name="scode"></param>
        /// <param name="klow"></param>
        /// <param name="klarge"></param>
        static void CountResult113(string scode, int klow, int klarge)
        {
            /*
            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //��������
            //objConnection.Open();  //������
            int count = 0;
            string strCom = "Select * from k_data  where code='" + scode + "' order by t_date ";
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
                    Console.WriteLine(time + "    " + scode);
                    WriteBenifit113(time, scode);
                    //    GetBenifit();
                }
                reader.Close();

            }
            catch (Exception e)
            {
                //throw(new Exception("���ݿ����:" + e.Message)) ;
            }
             * */
        }


        /// <summary>
        /// �����㷨1.1.3
        /// ���ø����Ͻ����㷨������ϴ���Ƿ�
        /// �������ڣ�20%���Ƿ��ǲ�С�������ˣ��ܶ�������������20%֮�󶼻��������θ��ӿ��ٵ��ڴ������ֹ��ƽ���Ǹ����⡣
        /// </summary>
        /// <param name="time"></param>
        /// <param name="scode"></param>
        static void WriteBenifit113(string time, string scode)
        {
            /*
            decimal k30N_1 = 0, k60N_1 = 0, k30N = 0, k60N = 0;
            decimal closeN = 0;//�ź��������
            decimal closeN_10 = 0;//�����ź���֮�������ʮ������һ����β
            decimal closeN_N10_high = 0;//�����ź������˺��ʮ���������г��ֵ����ֵ��
            decimal buy = 0;
            decimal sell = 0;
            string buydate = time;//�������ڡ�
            decimal top = 0;

            //string strConnection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=..\\..\\db_data.mdb";
            //OleDbConnection objConnection = new OleDbConnection(strConnection);  //��������
            //objConnection.Open();  //������
            //N-1�졣
            string strCom = "Select top 2 * from k_data  where code='" + scode + "' and t_date<=#" + time + "# order by t_date desc";
            OleDbCommand myCommand = new OleDbCommand(strCom, objConnection);
            OleDbDataReader reader;
            reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
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

            if ((k30N_1 < k60N_1) && (k30N >= k60N))//�ź������֡�
            {

                //��ȡ֮��ʮ��Ľ���״��
                strCom = "Select top 20 * from k_data  where code='" + scode + "' and  t_date>=#" + time + "# order by t_date   ";
                //  myCommand = new OleDbCommand(strCom, objConnection);
                myCommand.CommandText = strCom;
                reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
                while (reader.Read())
                {

                    closeN_10 = (decimal)reader["close"];
                    buydate = reader["t_date"].ToString();
                }
                top = closeN_10;//��߼۳�ʼ��Ϊ����ۡ�
                reader.Close();
                if (closeN_10 >= (decimal)1.15 * closeN)//�����ź�
                {
                    buy = closeN_10;
                    //����������
                    strCom = "Select   * from k_data  where code='" + scode + "' and t_date>=#" + buydate + "# order by t_date   ";
                    // myCommand = new OleDbCommand(strCom, objConnection);
                    myCommand.CommandText = strCom;
                    reader = myCommand.ExecuteReader(); //ִ��command���õ���Ӧ��DataReader
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
                            //ӯ�����֡�
                            //д��־�ļ�����¼code,buy,sell
                            string selltime = reader["t_date"].ToString();
                            string txt = scode + "," + time + ",";
                            txt += buy.ToString() + "," + selltime + "," + sell.ToString();
                            WritToLog(txt);
                            break;
                        }
                    }
                }

            }
            else //ʧ�ܡ�
            {
            }
            reader.Close();
            myCommand.Dispose();
            //   objConnection.Close();
             *  * */
        } 
            

    }





}
