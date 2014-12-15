using System;
using System.Collections.Generic;
using System.Text;

namespace autosystem
{
    /// <summary>
    /// ���ݿ�����࣬���ǵ�Ч�ʺ����ӳصĿ������⣬�ڴ�����Ϊ����ģʽ��
    /// �������߳�unsafe���⡣�������п����ٿ��ǡ�
    /// </summary>
    class DBManager
    {
        private DBManager() { }

        static private DBManager _instance = null;

        public DBManager getInstance() {
            if (_instance == null)
            {
                _instance = new DBManager();

            }
            return _instance;
        }

    }
}
