using UnityEngine;
using System.Collections;
//引入庫
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System;

public class serialReceiver:MonoBehaviour
{
    //以下預設都是私有的成員
    Socket serverSocket; //伺服器端socket
    Socket clientSocket; //客戶端socket
    IPEndPoint ipEnd; //偵聽埠
    string recvStr; //接收的字串
    byte[] recvData=new byte[1024]; //接收的資料，必須為位元組
    int recvLen; //接收的資料長度
    Thread connectThread; //連線執行緒

    public int rotaryTicks;
    public bool throttleOnOff;

    //初始化
    void InitSocket()
    {
        //定義偵聽埠,偵聽任何IP
        ipEnd=new IPEndPoint(IPAddress.Any,8000);
        //定義套接字型別,在主執行緒中定義
        serverSocket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        //連線
        serverSocket.Bind(ipEnd);
        //開始偵聽,最大10個連線
        serverSocket.Listen(10);
        System.Diagnostics.Process.Start("python", "./serialTCP.py");
             
        //開啟一個執行緒連線，必須的，否則主執行緒卡死
        connectThread=new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    //連線
    void SocketConnet()
    {
        if(clientSocket!=null)
            clientSocket.Close();
        //控制檯輸出偵聽狀態
        print("Waiting for a client");
        //一旦接受連線，建立一個客戶端
        clientSocket=serverSocket.Accept();
        //獲取客戶端的IP和埠
        IPEndPoint ipEndClient=(IPEndPoint)clientSocket.RemoteEndPoint;
    }

    //伺服器接收
    void SocketReceive()
    {
        //連線
        SocketConnet();      
        //進入接收迴圈
        while(true)
        {
            //對data清零
            recvData=new byte[1024];
            //獲取收到的資料的長度
            recvLen=clientSocket.Receive(recvData);
            //如果收到的資料長度為0，則重連並進入下一個迴圈
            if(recvLen==0)
            {
                SocketConnet();
                continue;
            }
            //輸出接收到的資料
            recvStr=Encoding.ASCII.GetString(recvData,0,recvLen);
            if(recvStr[0] != 't')
                rotaryTicks = Int32.Parse(recvStr);
            else
                throttleOnOff = !throttleOnOff;
        }
    }

    //連線關閉
    void SocketQuit()
    {
        //先關閉客戶端
        if(clientSocket!=null)
            clientSocket.Close();
        //再關閉執行緒
        if(connectThread!=null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最後關閉伺服器
        serverSocket.Close();
        print("diconnect");
    }

    // Use this for initialization
    void Start()
    {
        throttleOnOff = false;
        InitSocket(); //在這裡初始化server
    }


    // Update is called once per frame
    void Update()
    {
        print(throttleOnOff);
        print(rotaryTicks);
    }

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}