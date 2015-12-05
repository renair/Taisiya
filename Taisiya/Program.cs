using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Taisiya
{
    class Program
    {
        private static FileStream file;

        static void Main(string[] args)
        {
            Initialize();
            int readed;
            String[] fileData;
            Socket mainSock = Server.CreateSocket(1998);
            while(true)
            {
                byte[] buff = new byte[1024];
                try
                {
                    Socket user = mainSock.Accept();
                    user.ReceiveTimeout = 5000;
                    user.SendTimeout = 5000;
                    readed = user.Receive(buff);
                    fileData = Encoding.UTF8.GetString(buff, 0, readed).Split('\n');
                    if (fileData[0] == "test")
                    {
                        user.Send(Encoding.UTF8.GetBytes("OK"));
                        Console.WriteLine(fileData[0] + " OK");
                        user.Close();
                        Server.WriteLog("test OK");
                        continue;
                    }
                    Console.WriteLine(fileData[1]+" bytes");
                    Console.WriteLine(fileData[0]);
                    Server.WriteLog(fileData[0]+" \t "+fileData[1]);
                    file = new FileStream("files\\"+fileData[0], FileMode.OpenOrCreate);
                    Int64 fileSize = Int64.Parse(fileData[1]);
                    user.Send(Encoding.UTF8.GetBytes("OK"));
                    while(fileSize > 0)
                    {
                        readed = user.Receive(buff);
                        fileSize -= readed;
                        file.Write(buff, 0, readed);
                        file.Flush();
                    }
                    user.Send(Encoding.UTF8.GetBytes("OK"));
                    user.Close(3);
                    file.Close();
                    Server.WriteLog("Saved!\n");
                    Console.WriteLine("\tFinished");
                }
                catch(SocketException exception)
                {
                    Console.WriteLine(exception.Message);
                    Server.WriteLog(exception.Message);
                    file.Close();
                }
                catch(IOException exception)
                {
                    Console.WriteLine(exception.Message);
                    Server.WriteLog(exception.Message);
                    file.Close();
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Server.WriteLog(exception.Message);
                    file.Close();
                }
            }
        }

        private static void Initialize()
        {
            if(!Directory.Exists("files"))
            {
                Directory.CreateDirectory("files");
            }
        }
    }
    class Server
    {
        private static String LogFile = "log.txt";

        public static Socket CreateSocket(int port)
        {
            IPEndPoint addr = new IPEndPoint(IPAddress.Any,port);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(addr);
            sock.Listen(15);
            return sock;
        }

        public static void WriteLog(String fileName, String message)
        {
            FileStream file = new FileStream(fileName,FileMode.Append);
            message = message + "\n";
            byte[] bufer = UTF8Encoding.UTF8.GetBytes(message);
            file.Write(bufer,0,bufer.Length);
            file.Flush();
            file.Close();
        }

        public static void WriteLog(String message)
        {
            FileStream file = new FileStream(LogFile, FileMode.Append);
            message = message + "\n";
            byte[] bufer = UTF8Encoding.UTF8.GetBytes(message);
            file.Write(bufer, 0, bufer.Length);
            file.Flush();
            file.Close();
        }
    }
}
