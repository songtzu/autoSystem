using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using autosystem;

namespace autosystem
{
    class Program
    {
        static void Main(string[] args)
        {
            //下载线程
            DownloadGGData downloader = new DownloadGGData(true);
            Thread thread=new Thread(new ThreadStart(downloader.DownloadThread));
            thread.Start();

            //执行算法线程

        }
    }
}
