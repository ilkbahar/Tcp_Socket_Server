using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Data.SqlClient;
using System.Data;

namespace Server
{
     class Server
    {
        private static byte[] _buffer = new byte[1024];
        private static List<Socket> _clientSockets = new List<Socket>();
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Bağlantı Bekleniyor...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8080));
            _serverSocket.Listen(5);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),null);
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);
            Console.WriteLine("Client Bağlandı");
            socket.BeginReceive(_buffer,0, _buffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback),socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

       
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            string text = Encoding.ASCII.GetString(dataBuf);

           // 
     

            if(text != "")
            {
                //
                dynamic jsondata = JsonConvert.DeserializeObject(text);// jsondata...barcodeno
                
                
                 if(jsondata.islem == 0){
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    Console.WriteLine("Barcode no : " + jsondata.barcodeno);
                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else if(jsondata.islem == 1)//Barcode // Önemli!! 
                    //Bilgiler geldiğinde önce barcode no ile database den o barcode a ait bilgileri
                {//al(açıklama,kod,fiyat) gibi sonrasında bu bilgiler ile birlikte database e ekle. 
                    //Geriye "barcodeno"-"fiyat"-"miktar"(eklenmiş olan miktarı) gönder ve telefonda da ayrı db tut ekranda göster
                    //eğer silinmesi istenirse işlem 0 ile barcode ve miktar gönder. gelen miktar kadar barcode numarasına göre db den kaldır
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    string barcodeno = jsondata.barcodeno;  int miktar = jsondata.miktar;
                    string sayimfisi = jsondata.sayimfisi, depo = jsondata.depo, tarih = jsondata.tarih, sg = jsondata.sayimgorevlisi, reyon = jsondata.reyon, pn = jsondata.personelnotu;
                    DateTime time = DateTime.Now;
                    
                    try
                    {
                        //@"Data Source=(DESKTOP-AM0TU5O)\(SQLEXPRESS);Initial Catalog=(faturalar);Integrated Security=True;"
                        SqlConnection sqlCon = new SqlConnection(@"Server=localhost\SQLEXPRESS;Database=faturalar;Trusted_Connection=True;");

                        sqlCon.Open();
                        Console.WriteLine("Connection Open ! ");
                        /* SqlCommand sqlCommand = new SqlCommand("INSERT INTO fatura (barcodeno,aciklama,[kod(stok)],miktar,sayimfisi,depo,tarih,sayimgorevlisi,reyon,personelnotu)" +
                             "Values ("+jsondata.barcodeno+", 'açıklama','kodstok',"+jsondata.miktar+","+jsondata.sayimfisi+ ","+jsondata.depo+","+jsondata.tarih+","+jsondata.sayimgorevlisi+ ","+jsondata.reyon+","+jsondata.personelnotu+ ")", sqlCon);*/
                        /*  SqlCommand sqlCommand = new SqlCommand("INSERT INTO fatura (barcodeno,aciklama,[kod(stok)],miktar,sayimfisi,depo,tarih,sayimgorevlisi,reyon,personelnotu)" +
                               "Values (" + barcodeno + ", 'açıklama','kodstok'," + miktar + "," + sayimfisi+ "," + depo + "," + tarih + "," + sg + "," + reyon + "," + pn + ")", sqlCon);*/
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO fatura (barcodeno,aciklama,[kod(stok)],miktar,sayimfisi,depo,tarih,sayimgorevlisi,reyon,personelnotu)" +
                      "Values ("+barcodeno+", 'Açıklama','Kod(STOK)',"+miktar+",'"+sayimfisi+"','"+depo+"','"+time.ToString(tarih)+"','"+sg+"','"+reyon+"','"+pn+"')", sqlCon);
                        sqlCommand.ExecuteNonQuery();
                        // insert into tablename (barcodeno) values "2123134566");
                        sqlCon.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Eklenmezse geri bildirim gönder! ");
                    }

                    Console.ReadKey();

                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);

                    

                }
                else if (jsondata.islem == 2)//Açıklama
                {
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    string aciklama = jsondata.barcodeno; int miktar = jsondata.miktar;
                    string sayimfisi = jsondata.sayimfisi, depo = jsondata.depo, tarih = jsondata.tarih, sg = jsondata.sayimgorevlisi, reyon = jsondata.reyon, pn = jsondata.personelnotu;
                    DateTime time = DateTime.Now;

                    try
                    {
                        //@"Data Source=(DESKTOP-AM0TU5O)\(SQLEXPRESS);Initial Catalog=(faturalar);Integrated Security=True;"
                        SqlConnection sqlCon = new SqlConnection(@"Server=localhost\SQLEXPRESS;Database=faturalar;Trusted_Connection=True;");

                        sqlCon.Open();
                        Console.WriteLine("Connection Open ! ");
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO fatura (barcodeno,aciklama,[kod(stok)],miktar,sayimfisi,depo,tarih,sayimgorevlisi,reyon,personelnotu)" +
                      "Values ( 'Barkodno' , '"+ aciklama + "','Kod(STOK)'," + miktar + ",'" + sayimfisi + "','" + depo + "','" + time.ToString(tarih) + "','" + sg + "','" + reyon + "','" + pn + "')", sqlCon);
                        sqlCommand.ExecuteNonQuery();

                        sqlCon.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Eklenmezse geri bildirim gönder! ");
                    }

                    Console.ReadKey();

                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else if (jsondata.islem == 3)//Kod(Stok)
                {
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    string kodstok = jsondata.barcodeno; int miktar = jsondata.miktar;
                    string sayimfisi = jsondata.sayimfisi, depo = jsondata.depo, tarih = jsondata.tarih, sg = jsondata.sayimgorevlisi, reyon = jsondata.reyon, pn = jsondata.personelnotu;
                    DateTime time = DateTime.Now;

                    try
                    {
                        //@"Data Source=(DESKTOP-AM0TU5O)\(SQLEXPRESS);Initial Catalog=(faturalar);Integrated Security=True;"
                        SqlConnection sqlCon = new SqlConnection(@"Server=localhost\SQLEXPRESS;Database=faturalar;Trusted_Connection=True;");

                        sqlCon.Open();
                        Console.WriteLine("Connection Open ! ");
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO fatura (barcodeno,aciklama,[kod(stok)],miktar,sayimfisi,depo,tarih,sayimgorevlisi,reyon,personelnotu)" +
                      "Values ( 'Barkodno' , 'Açıklama','"+ kodstok + "'," + miktar + ",'" + sayimfisi + "','" + depo + "','" + time.ToString(tarih) + "','" + sg + "','" + reyon + "','" + pn + "')", sqlCon);
                        sqlCommand.ExecuteNonQuery();

                        sqlCon.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Eklenmezse geri bildirim gönder! ");
                    }

                    Console.ReadKey();

                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else if (jsondata.islem == 4)
                {
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    Console.WriteLine("Depo : " + jsondata.depo);
                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else if (jsondata.islem == 5)
                {
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    Console.WriteLine("Sayım Görevlisi : " + jsondata.sayimgorevlisi);
                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }
                else
                {
                    Console.WriteLine("İşlem No : " + jsondata.islem);
                    Console.WriteLine("Reyon : " + jsondata.reyon);
                    byte[] data = Encoding.ASCII.GetBytes(text);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }


            }
            
            
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

    }
}
