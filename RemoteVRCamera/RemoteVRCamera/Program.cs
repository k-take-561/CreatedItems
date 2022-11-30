using System;
using System.Threading;

namespace RemoteVRCamera
{
    class Program
    {
        //電動車いすの入力値をそのまま入力し，変更する必要がない場合はこのプログラムが使いやすい
        //もし入力する値を変更する(ヒューマンアシスト制御を加えるなど)のであればAssistedRemotewhillcrListenerのプログラムを使用すること

        //Severの引数として最初に電動車いすのシリアル通信のポート番号，二つ目に電動車いすのTCPのポート番号である．
        //デフォルトとしてシリアル通信のポート番号は"COM3"，電動車いすのTCPのポート番号は20000である．
        //シリアル通信のポート番号のみの変更の場合は　Sever sever = new Sever("COM4");　と書き，
        //電動車いすのTCPのポート番号のみの変更の場合は　Sever sever = new Sever(whillTcpPort : 10000);　と書くことで変更が可能である．
        //sever.emergencyStopSignal = tureになったら危険信号として電動車いすはストップする．

        static RSControl rs;
        static ThetaL theta;
        static bool usewhill = false;
        static void Main()
        {
            //Sever sever = new Sever("COM3", 20000);
            //sever.Start();

            bool flg = true;
            Thread rsThr;
            Thread thetaThr;

            //rsThr = new Thread(new ThreadStart(RealSenseOperate)); rsThr.Start();
            thetaThr = new Thread(new ThreadStart(ThetaOperate)); thetaThr.Start();

            while (flg)
            {
                Thread.Sleep(1000);
            }

            //rs.Exit();
            theta.Exit();
        }
        static void RealSenseOperate()
        {
            Thread pcThr;
            Thread rgbThr;

            rs = new RSControl(2001, 2002, usewhill);
            rs.Initialize();
            pcThr = new Thread(new ThreadStart(rs.GetAndSendDepth)); pcThr.Start();
            rgbThr = new Thread(new ThreadStart(rs.GetAndSendColor)); rgbThr.Start();
        }
        static void ThetaOperate()
        {
            theta = new ThetaL(0, 2003);
            theta.GetAndSend();
            //thetaThr = new Thread(new ThreadStart(theta.GetAndSend)); thetaThr.Start();
        }
    }
}
