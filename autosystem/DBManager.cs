using System;
using System.Collections.Generic;
using System.Text;

namespace autosystem
{
    /// <summary>
    /// 数据库管理类，考虑到效率和连接池的开闭问题，在此声明为单例模式。
    /// 可能有线程unsafe问题。待后面有空了再考虑。
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
