using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Button[] but;
        bool isCreated = false;
        string nameChat;
        Image img;
        //AI
        EvalBoard eBoard;
        public int[,] BoardArr = new int[12, 12]; //Nguoi 1 - May(or player 2) 2 - Chua 0
        public int[,] BoardArrVt = new int[12, 12];
        int playerFlag = 1; //Biến cờ xác định máy đi hay người đi.
        int onlPcFlag = 0; // 1 - pc may minh,  2 - doi phuong
        int x, y,_x, _y; //Tọa độ nước cờ mà máy đi.

        public static int maxDepth = 11;
        public static int maxMove = 3;
        public int depth = 0;

        public bool fWin = false;
        public int fEnd = 0;
        int endFlag = 0;

        public int[] DScore = new int[5] { 0, 1, 9, 81, 729 };
        public int[] AScore = new int[5] { 0, 2, 18, 162, 1458 };

        Point[] PCMove = new Point[maxMove + 2];
        Point[] HumanMove = new Point[maxMove + 2];
        Point[] WinMove = new Point[maxDepth + 2];
        Point[] LoseMove = new Point[maxDepth + 2];

        int vtx = -1, vty = -1, chat = 0;

        void choiCoOnline()
        {
            var socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + "Connected" + "\n" + "......................................................................................................................" + "\n";
                chatHistory.ScrollToEnd();
                }));   
            });
            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {                     
                    chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + data.ToString() + "\n" + "......................................................................................................................" + "\n";
                    chatHistory.ScrollToEnd();
                })); 
            });
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + data.ToString() + "\n" + "......................................................................................................................" + "\n";
                    chatHistory.ScrollToEnd();
                })); 
            });
            //loi
            socket.On("ChatMessage", (data) => 
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                   if (((Newtonsoft.Json.Linq.JObject)data)["from"] == null)
                        chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() + "\n" + "......................................................................................................................" + "\n";
                   else
                       chatHistory.Text = chatHistory.Text + ((Newtonsoft.Json.Linq.JObject)data)["from"].ToString() + "  (" + DateTime.Now.ToString() + ") \n" + ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() + "\n" + "......................................................................................................................" + "\n";
                   chatHistory.ScrollToEnd();
                })); 
                if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                {          
                    socket.Emit("MyNameIs", nameChat.ToString());
                    socket.Emit("ConnectToOtherPlayer");
                }
            });
                
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {                
                    chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + data.ToString() + "\n" + "......................................................................................................................" + "\n";
                  
                })); 
            });

            //thong bao buoc vua di 
            socket.On("NextStepIs", (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {                 
                    if (((Newtonsoft.Json.Linq.JObject)data)["player"].ToString() == "0")// luot cua minh
                    {                      
                        int d = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString()) -1;
                        int c = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString()) -1;
                        chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + "Bạn đã chọn dòng " + d.ToString() + " cột " + c.ToString() + "\n" + "......................................................................................................................" + "\n";       
                        img = new Image();
                        img.Source = new BitmapImage(new Uri("Image/1.png", UriKind.Relative));
                        img.Stretch = Stretch.Fill;
                        but[d * 12 + c].Content = img;
                        BoardArr[d, c] = 1;
                        if (CheckEnd(d, c) == 1) { MessageBox.Show("Bạn đã thắng :) !"); endFlag = 1; EndBatte(); return; }
                        playerFlag = 2;
                    }
                    else
                    {
                        int d = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString()) -1;
                        int c = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString()) -1;
                        chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + "Đối phương đã chọn dòng " + d.ToString() + " cột " + c.ToString() + "\n" + "......................................................................................................................" + "\n";                      
                        img = new Image();
                        img.Source = new BitmapImage(new Uri("Image/2.png", UriKind.Relative));
                        img.Stretch = Stretch.Fill;
                        but[d * 12 + c].Content = img;
                        BoardArr[d, c] = 2;
                        if (CheckEnd(d, c) == 2) { MessageBox.Show("Bạn đã thua :( !"); endFlag = 1; EndBatte(); return; }
                    }
                    chatHistory.ScrollToEnd();
                })); 
            });
            //socket.Connect();
            while (true)
            {
                if (endFlag == 1)
                    break;
                if (vtx != -1)
                {
                    socket.Emit("MyStepIs", JObject.FromObject(new { row = vtx + 1, col = vty + 1}));
                    vtx = -1;
                }

                if (chat != 0)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        socket.Emit("ChatMessage", Chatbox.Text);
                        Chatbox.Text = "";
                        chatHistory.ScrollToEnd();
                        chat = 0;
                    })); 
                    
                }
            }
        }

        void choiPcOnline()
        {
            var socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + "Connected" + "\n" + "......................................................................................................................" + "\n";
                    chatHistory.ScrollToEnd();
                }));
            });
            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + data.ToString() + "\n" + "......................................................................................................................" + "\n";
                    chatHistory.ScrollToEnd();
                }));
            });
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + data.ToString() + "\n" + "......................................................................................................................" + "\n";
                    chatHistory.ScrollToEnd();
                }));
            });
            //loi
            socket.On("ChatMessage", (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (((Newtonsoft.Json.Linq.JObject)data)["from"] == null)
                        chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() + "\n" + "......................................................................................................................" + "\n";
                    else
                        chatHistory.Text = chatHistory.Text + ((Newtonsoft.Json.Linq.JObject)data)["from"].ToString() + "  (" + DateTime.Now.ToString() + ") \n" + ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() + "\n" + "......................................................................................................................" + "\n";
                    chatHistory.ScrollToEnd();
                }));
                if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                {
                    socket.Emit("MyNameIs", nameChat.ToString());
                    socket.Emit("ConnectToOtherPlayer");
                    
                }
            });
            //socket.Emit("MyStepIs", JObject.FromObject(new { row = 12, col = 12 }));
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + data.ToString() + "\n" + "......................................................................................................................" + "\n";

                }));
            });
            //thong bao buoc vua di 
            socket.On("NextStepIs", (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    //chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + "NextStepIs: " + data.ToString() + "\n" + "......................................................................................................................" + "\n";                  
                    if (((Newtonsoft.Json.Linq.JObject)data)["player"].ToString() == "0")// luot cua minh
                    {
                        int d = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString()) - 1;
                        int c = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString()) - 1;
                        chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + "Bạn đã chọn dòng " + d.ToString() + " cột " + c.ToString() + "\n" + "......................................................................................................................" + "\n";
                        img = new Image();
                        img.Source = new BitmapImage(new Uri("Image/1.png", UriKind.Relative));
                        img.Stretch = Stretch.Fill;
                        but[d * 12 + c].Content = img;
                        BoardArr[d, c] = 1;
                        BoardArrVt[d, c] = 1;
                        if (CheckEnd(d, c) == 1) { MessageBox.Show("Bạn đã thắng :) !"); endFlag = 1; EndBatte(); return; }
                        onlPcFlag = 2; // chuyen den luot nguoi choi onl
                    }
                    else
                    {
                        int d = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString()) - 1;
                        int c = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString()) - 1;
                        chatHistory.Text = chatHistory.Text + "Server" + "  (" + DateTime.Now.ToString() + ") \n" + "Đối phương đã chọn dòng " + d.ToString() + " cột " + c.ToString() + "\n" + "......................................................................................................................" + "\n";
                        img = new Image();
                        img.Source = new BitmapImage(new Uri("Image/2.png", UriKind.Relative));
                        img.Stretch = Stretch.Fill;
                        but[d * 12 + c].Content = img;
                        BoardArr[d, c] = 2;
                        BoardArrVt[d, c] = 1;
                        if (CheckEnd(d, c) == 2) { MessageBox.Show("Bạn đã thua :( !"); endFlag = 1; EndBatte(); return; } 
                        Thread AIThread = new Thread(AI);
                        AIThread.Start();
                        AIThread.Join();
                        lock (tlock)
                        {
                            if (fWin)
                            {
                                _x = (int)WinMove[0].X;
                                _y = (int)WinMove[0].Y;
                            }
                            else
                            {
                                EvalChessBoard(2, ref eBoard);
                                Point temp = new Point();
                                temp = eBoard.MaxPos();
                                _x = (int)temp.X;
                                _y = (int)temp.Y;
                            }
                            if (BoardArrVt[_x, _y] == 1)
                            {
                                for (int i = 0; i < 144; i++)
                                    if (BoardArrVt[i / 12, i % 12] == 0)
                                    {
                                        BoardArrVt[i / 12, i % 12] = 1;
                                        _x = i / 12;
                                        _y = i % 12;
                                        break;
                                    }
                            }
                            onlPcFlag = 1; // quay ve luot minh                     
                        }
                    }
                    chatHistory.ScrollToEnd();
                }));
            });
            //socket.Connect();
            while (true)
            {
                if (endFlag == 1)
                    break;
                if (onlPcFlag == 1)
                {
                    onlPcFlag = 2;
                    socket.Emit("MyStepIs", JObject.FromObject(new { row = _x + 1, col = _y + 1 }));
                }

                if (chat != 0)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        socket.Emit("ChatMessage", Chatbox.Text);
                        Chatbox.Text = "";
                        chatHistory.ScrollToEnd();
                        chat = 0;
                    }));

                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            nameChat = Name.Text;         
        }

        void TaoButton1()
        {
            nameChat = Name.Text;
            bool color = false;
            but = new Button[144];
            double h = 0, w = 0;
            double mh, mw;
            mh = (int)(mainWindow.Height - 60)/ 12;
            mw = (int)(mainWindow.Width -10 - 400)/ 12;
            for (int i = 0; i < 12; i++)
            {
                if (i % 2 == 0)
                    color = true;
                else
                    color = false;
                h = 10 + i*mh;
                w = 10;
                for (int j = 0; j < 12; j++)
                {             
                    but[i * 12 + j] = new Button();
                    but[i * 12 + j].Name = "bt" + (i * 12 + j).ToString();
                    but[i * 12 + j].Width = mw;
                    but[i * 12 + j].Height = mh;
                    but[i * 12 + j].HorizontalAlignment = HorizontalAlignment.Left;
                    but[i * 12 + j].VerticalAlignment = VerticalAlignment.Top;
                    but[i * 12 + j].Margin = new Thickness(w, h, 0, 0);
                    but[i * 12 + j].Click += Button_Click1;
                    if (color == true)
                    {
                        but[i * 12 + j].Background = Brushes.Gray;
                        color = false;
                    }
                    else
                    {
                        but[i * 12 + j].Background = Brushes.White;
                        color = true;
                    }
                    myGrid.Children.Add(but[i * 12 + j]);
                    w = w + mw;
                }
            }
        }

        void TaoButton2()
        {
            nameChat = Name.Text;
            bool color = false;
            but = new Button[144];
            double h = 0, w = 0;
            double mh, mw;
            mh = (int)(mainWindow.Height - 60) / 12;
            mw = (int)(mainWindow.Width - 10 - 400) / 12;
            for (int i = 0; i < 12; i++)
            {
                if (i % 2 == 0)
                    color = true;
                else
                    color = false;
                h = 10 + i * mh;
                w = 10;
                for (int j = 0; j < 12; j++)
                {
                    but[i * 12 + j] = new Button();
                    but[i * 12 + j].Name = "bt" + (i * 12 + j).ToString();
                    but[i * 12 + j].Width = mw;
                    but[i * 12 + j].Height = mh;
                    but[i * 12 + j].HorizontalAlignment = HorizontalAlignment.Left;
                    but[i * 12 + j].VerticalAlignment = VerticalAlignment.Top;
                    but[i * 12 + j].Margin = new Thickness(w, h, 0, 0);
                    but[i * 12 + j].Click += Button_Click2;
                    if (color == true)
                    {
                        but[i * 12 + j].Background = Brushes.Gray;
                        color = false;
                    }
                    else
                    {
                        but[i * 12 + j].Background = Brushes.White;
                        color = true;
                    }
                    myGrid.Children.Add(but[i * 12 + j]);
                    w = w + mw;
                }
            }
        }

        void TaoButton3()
        {
            nameChat = Name.Text;
            bool color = false;
            but = new Button[144];
            double h = 0, w = 0;
            double mh, mw;
            mh = (int)(mainWindow.Height - 60) / 12;
            mw = (int)(mainWindow.Width - 10 - 400) / 12;
            for (int i = 0; i < 12; i++)
            {
                if (i % 2 == 0)
                    color = true;
                else
                    color = false;
                h = 10 + i * mh;
                w = 10;
                for (int j = 0; j < 12; j++)
                {
                    but[i * 12 + j] = new Button();
                    but[i * 12 + j].Name = "bt" + (i * 12 + j).ToString();
                    but[i * 12 + j].Width = mw;
                    but[i * 12 + j].Height = mh;
                    but[i * 12 + j].HorizontalAlignment = HorizontalAlignment.Left;
                    but[i * 12 + j].VerticalAlignment = VerticalAlignment.Top;
                    but[i * 12 + j].Margin = new Thickness(w, h, 0, 0);
                    but[i * 12 + j].Click += Button_Click3;
                    if (color == true)
                    {
                        but[i * 12 + j].Background = Brushes.Gray;
                        color = false;
                    }
                    else
                    {
                        but[i * 12 + j].Background = Brushes.White;
                        color = true;
                    }
                    myGrid.Children.Add(but[i * 12 + j]);
                    w = w + mw;
                }
            }
        }

        void ChangeSize()
        {
            if(isCreated == true)
            {
                double h = 0, w = 0;
                double mh, mw;
                mh = (int)(mainWindow.Height - 60) / 12;
                mw = (int)(mainWindow.Width - 10 - 420) / 12;
                for (int i = 0; i < 12; i++)
                {
                
                    h = 10 + i * mh;
                    w = 10;
                    for (int j = 0; j < 12; j++)
                    {
                        but[i * 12 + j].Width = mw;
                        but[i * 12 + j].Height = mh;
                        but[i * 12 + j].HorizontalAlignment = HorizontalAlignment.Left;
                        but[i * 12 + j].VerticalAlignment = VerticalAlignment.Top;
                        but[i * 12 + j].Margin = new Thickness(w, h, 0, 0);
                        w = w + mw;
                    }
                }
                if (this.WindowState == WindowState.Maximized)
                {
                    mh = 53;
                    mw = 78;
                    for (int i = 0; i < 12; i++)
                    {
                        h = 10 + i * mh;
                        w = 10;
                        for (int j = 0; j < 12; j++)
                        {
                            but[i * 12 + j].Width = mw;
                            but[i * 12 + j].Height = mh;
                            but[i * 12 + j].HorizontalAlignment = HorizontalAlignment.Left;
                            but[i * 12 + j].VerticalAlignment = VerticalAlignment.Top;
                            but[i * 12 + j].Margin = new Thickness(w, h, 0, 0);
                            w = w + mw;
                        }
                }
            }
            }
        }
 
        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeSize();
        }
        //AI
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name.ToString();
            string a = name.Remove(0, 2);
            int vt = int.Parse(a);
            int dong = vt/12;
            int cot = vt%12;
            //AI
            if (fEnd == 0 && playerFlag == 1)
            {             
                x = dong;
                y = cot;
                if (BoardArr[x, y] == 0) // vi tri nay chua chon
                {
                    
                    BoardArr[x, y] = 1; // danh dau vi tri nay do nguoi chon
                    BoardArrVt[x, y] = 1;
                    img = new Image();
                    img.Source = new BitmapImage(new Uri("Image/1.png", UriKind.Relative));
                    img.Stretch = Stretch.Fill;
                    (sender as Button).Content = img;  
                    if (CheckEnd(x, y) == 1) { MessageBox.Show("Chien thang!!!"); fEnd = 1; EndBatte(); return; }
                    
                    //May di
                    Thread AIThread = new Thread(AI);
                    AIThread.Start();
                    AIThread.Join();
                    lock (tlock)
                    {
                        if (fWin)
                        {
                            _x = (int)WinMove[0].X;
                            _y = (int)WinMove[0].Y;
                        }
                        else
                        {
                            EvalChessBoard(2, ref eBoard);
                            Point temp = new Point();
                            temp = eBoard.MaxPos();
                            _x = (int)temp.X;
                            _y = (int)temp.Y;
                        }
                        if (BoardArrVt[_x, _y] == 1)
                        {
                            for (int i = 0; i < 144; i++)
                                if (BoardArrVt[i / 12, i % 12] == 0)
                                {
                                    BoardArr[i / 12, i % 12] = 2;
                                    BoardArrVt[i / 12, i % 12] = 1;                                   
                                    img = new Image();
                                    img.Source = new BitmapImage(new Uri("Image/2.png", UriKind.Relative));
                                    img.Stretch = Stretch.Fill;
                                    but[i].Content = img;
                                    if (CheckEnd(i / 12, i % 12) == 2) { MessageBox.Show("Bạn đã thua!"); fEnd = 2; EndBatte(); return; }
                                    break;
                                }
                        }
                        else
                        {
                            BoardArr[_x, _y] = 2;
                            BoardArrVt[_x, _y] = 1;
                            img = new Image();
                            img.Source = new BitmapImage(new Uri("Image/2.png", UriKind.Relative));
                            img.Stretch = Stretch.Fill;
                            but[_x * 12 + _y].Content = img;
                            if (CheckEnd(_x, _y) == 2) { MessageBox.Show("Bạn đã thua!"); fEnd = 2; EndBatte(); return; }
                        }
                    }
                }
            }
        }
        //TwoPlayer
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name.ToString();
            string a = name.Remove(0, 2);
            int vt = int.Parse(a);
            int dong = vt / 12;
            int cot = vt % 12;
            if (fEnd == 0 && playerFlag == 1)
            {

                int x = dong;
                int y = cot;
                if (BoardArr[x, y] == 0) // vi tri nay chua chon
                {
                    vtx = x; vty = y;
                    //chatHistory.Text = chatHistory.Text   + "Player 1 turn" + "\n";
                    BoardArr[x, y] = 1; // danh dau vi tri nay do nguoi choi 1 chon
                    img = new Image();
                    img.Source = new BitmapImage(new Uri("Image/1.png", UriKind.Relative));
                    img.Stretch = Stretch.Fill;
                    (sender as Button).Content = img;
                    if (CheckEnd(x, y) == 1) { MessageBox.Show("Người chơi 1 thắng"); fEnd = 1; EndBatte(); return; }
                    playerFlag = 2;
                }
            }
            else
            {
                int x = dong;
                int y = cot;
                if (BoardArr[x, y] == 0) // vi tri nay chua chon
                {
                    vtx = x; vty = y;
                    //chatHistory.Text = chatHistory.Text  + "Player 2 turn" + "\n";
                    BoardArr[x, y] = 2; // danh dau vi tri nay do nguoi choi 2 cho
                    img = new Image();
                    img.Source = new BitmapImage(new Uri("Image/2.png", UriKind.Relative));
                    img.Stretch = Stretch.Fill;
                    (sender as Button).Content = img;
                    if (CheckEnd(x, y) == 2) { MessageBox.Show("Người chơi 2 thắng"); fEnd = 1; EndBatte(); return; }
                    playerFlag = 1;
                }
            }
        }
        // player online
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name.ToString();
            string a = name.Remove(0, 2);
            int vt = int.Parse(a);
            int dong = vt / 12;
            int cot = vt % 12;
            vtx = dong; vty = cot;
            _x = vtx; _y = vty;
            onlPcFlag = 1;
        }
                      
        private void Changbt_Click(object sender, RoutedEventArgs e)
        {
            if (Name.IsEnabled == false)
            {
                Name.IsEnabled = true;
                Name.Focus();
                Changbt.Content = "Done";
            }
            else
            {
                Name.IsEnabled = false;
                Changbt.Content = "Change!";
                nameChat = Name.Text;
            }
        }

        private void Sent_Click(object sender, RoutedEventArgs e)
        {
            chat = 1;
            //chatHistory.Text = chatHistory.Text + nameChat + "  (" + DateTime.Now.ToString() + ") \n" + Chatbox.Text + "\n" + "......................................................................................................................" + "\n";
            //Chatbox.Text = "";
            //chatHistory.ScrollToEnd();
            
        }

        //xu li ai

        //Ham tinh gia tri cho bang luong gia

        private void EvalChessBoard(int player, ref EvalBoard eBoard)
        {
            int rw, cl, ePC, eHuman;
            eBoard.ResetBoard();

            //Danh gia theo hang
            for (rw = 0; rw < 12; rw++)
                for (cl = 0; cl < 8; cl++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BoardArr[rw, cl + i] == 1) eHuman++;
                        if (BoardArr[rw, cl + i] == 2) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BoardArr[rw, cl + i] == 0) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (player == 1)
                                        eBoard.EBoard[rw, cl + i] += DScore[ePC];
                                    else eBoard.EBoard[rw, cl + i] += AScore[ePC];
                                if (ePC == 0)
                                    if (player == 2)
                                        eBoard.EBoard[rw, cl + i] += DScore[eHuman];
                                    else eBoard.EBoard[rw, cl + i] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    eBoard.EBoard[rw, cl + i] *= 2;
                            }
                        }

                    }
                }

            //Danh gia theo cot
            for (cl = 0; cl < 12; cl++)
                for (rw = 0; rw < 8; rw++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BoardArr[rw + i, cl] == 1) eHuman++;
                        if (BoardArr[rw + i, cl] == 2) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BoardArr[rw + i, cl] == 0) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (player == 1)
                                        eBoard.EBoard[rw + i, cl] += DScore[ePC];
                                    else eBoard.EBoard[rw + i, cl] += AScore[ePC];
                                if (ePC == 0)
                                    if (player == 2)
                                        eBoard.EBoard[rw + i, cl] += DScore[eHuman];
                                    else eBoard.EBoard[rw + i, cl] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    eBoard.EBoard[rw + i, cl] *= 2;
                            }
                        }

                    }
                }

            //Danh gia duong cheo xuong
            for (cl = 0; cl < 8; cl++)
                for (rw = 0; rw < 8; rw++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BoardArr[rw + i, cl + i] == 1) eHuman++;
                        if (BoardArr[rw + i, cl + i] == 2) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BoardArr[rw + i, cl + i] == 0) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (player == 1)
                                        eBoard.EBoard[rw + i, cl + i] += DScore[ePC];
                                    else eBoard.EBoard[rw + i, cl + i] += AScore[ePC];
                                if (ePC == 0)
                                    if (player == 2)
                                        eBoard.EBoard[rw + i, cl + i] += DScore[eHuman];
                                    else eBoard.EBoard[rw + i, cl + i] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    eBoard.EBoard[rw + i, cl + i] *= 2;
                            }
                        }

                    }
                }

            //Danh gia duong cheo len
            for (rw = 4; rw < 12; rw++)
                for (cl = 0; cl < 8; cl++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (BoardArr[rw - i, cl + i] == 1) eHuman++;
                        if (BoardArr[rw - i, cl + i] == 2) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (BoardArr[rw - i, cl + i] == 0) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (player == 1)
                                        eBoard.EBoard[rw - i, cl + i] += DScore[ePC];
                                    else eBoard.EBoard[rw - i, cl + i] += AScore[ePC];
                                if (ePC == 0)
                                    if (player == 2)
                                        eBoard.EBoard[rw - i, cl + i] += DScore[eHuman];
                                    else eBoard.EBoard[rw - i, cl + i] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    eBoard.EBoard[rw - i, cl + i] *= 2;
                            }
                        }

                    }
                }

        }

        //Ham tim nuoc di cho may
        private void FindMove()
        {
            if (depth > maxDepth) return;
            depth++;
            fWin = false;
            bool fLose = false;
            Point pcMove = new Point();
            Point humanMove = new Point();
            int countMove = 0;

            EvalChessBoard(2, ref eBoard);

            //Lay ra MaxMove buoc di co diem cao nhat
            Point temp = new Point();
            for (int i = 0; i < maxMove; i++)
            {
                temp = eBoard.MaxPos();
                PCMove[i] = temp;
                eBoard.EBoard[(int)temp.X, (int)temp.Y] = 0;
            }

            //Lay nuoc di trong PCMove[] ra danh thu
            countMove = 0;
            while (countMove < maxMove)
            {

                pcMove = PCMove[countMove++];
                BoardArr[(int)pcMove.X, (int)pcMove.Y] = 2;
                WinMove.SetValue(pcMove, depth - 1);

                //Tim cac nuoc di toi uu cua nguoi
                eBoard.ResetBoard();
                EvalChessBoard(1, ref eBoard);
                //Lay ra maxMove nuoc di co diem cao nhat cua nguoi
                for (int i = 0; i < maxMove; i++)
                {
                    temp = eBoard.MaxPos();
                    HumanMove[i] = temp;
                    eBoard.EBoard[(int)temp.X, (int)temp.Y] = 0;
                }
                //Danh thu cac nuoc di
                for (int i = 0; i < maxMove; i++)
                {
                    humanMove = HumanMove[i];
                    BoardArr[(int)humanMove.X, (int)humanMove.Y] = 1;
                    if (CheckEnd((int)humanMove.X, (int)humanMove.Y) == 2)
                    {
                        fWin = true;
                        //MessageBox.Show("fwin" + fWin.ToString());
                    }
                    if (CheckEnd((int)humanMove.X, (int)humanMove.Y) == 1)
                    {
                        fLose = true;
                        //MessageBox.Show("flose" + fLose.ToString());
                    }
                    if (fLose)
                    {
                        BoardArr[(int)pcMove.X, (int)pcMove.Y] = 0;
                        BoardArr[(int)humanMove.X, (int)humanMove.Y] = 0;
                        break;
                    }
                    if (fWin)
                    {
                        BoardArr[(int)pcMove.X, (int)pcMove.Y] = 0;
                        BoardArr[(int)humanMove.X, (int)humanMove.Y] = 0;
                        return;
                    }
                    FindMove();
                    BoardArr[(int)humanMove.X, (int)humanMove.Y] = 0;
                }
                BoardArr[(int)pcMove.X, (int)pcMove.Y] = 0;
            }

        }
        public object tlock = new object();
        private void AI()
        {
            lock(tlock)
            {
                for (int i = 0; i < maxMove; i++)
                {
                    WinMove[i] = new Point();
                    PCMove[i] = new Point();
                    HumanMove[i] = new Point();
                }
                depth = 0;
                FindMove();
            }
        }

        //Ham kiem tra ket thuc
        private int CheckEnd(int cl, int rw)
        {
            int r = 0, c = 0;
            int i;
            bool human, pc;
            //Check hàng ngang
            while (c < 7)
            {
                human = true; pc = true;
                for (i = 0; i < 5; i++)
                {
                    if (BoardArr[cl, c + i] != 1)
                        human = false;
                    if (BoardArr[cl, c + i] != 2)
                        pc = false;
                }
                if (human) return 1;
                if (pc) return 2;
                c++;
            }

            //Check hàng dọc
            while (r < 7)
            {
                human = true; pc = true;
                for (i = 0; i < 5; i++)
                {
                    if (BoardArr[r + i, rw] != 1)
                        human = false;
                    if (BoardArr[r + i, rw] != 2)
                        pc = false;
                }
                if (human) return 1;
                if (pc) return 2;
                r++;
            }

            //Check duong cheo xuong
            r = rw; c = cl;
            while (r > 0 && c > 0) { r--; c--; }
            while (r <= 7 && c <= 7)
            {
                human = true; pc = true;
                for (i = 0; i < 5; i++)
                {
                    if (BoardArr[c + i, r + i] != 1)
                        human = false;
                    if (BoardArr[c + i, r + i] != 2)
                        pc = false;
                }
                if (human) return 1;
                if (pc) return 2;
                r++; c++;
            }

            //Check duong cheo len
            r = rw; c = cl;
            while (r < 11 && c > 0) { r++; c--; }
            while (r >= 4 && c <= 7)
            {
                human = true; pc = true;
                for (i = 0; i < 5; i++)
                {
                    if (BoardArr[r - i, c + i] != 1)
                        human = false;
                    if (BoardArr[r - i, c + i] != 2)
                        pc = false;
                }
                if (human) return 1;
                if (pc) return 2;
                r--; c++;
            }
            return 0;
        }

        private void twoplayer_Click(object sender, RoutedEventArgs e)
        {
            AIplayer.Visibility = Visibility.Hidden;
            twoplayer.Visibility = Visibility.Hidden;
            PlayOnline.Visibility = Visibility.Hidden;
            PC_online.Visibility = Visibility.Hidden;
            TaoButton2();
            playerFlag = 1;
            isCreated = true;
            ChangeSize();
            fWin = false;
            fEnd = 0;
            for (int i = 0; i < 144; i++)
                BoardArr[i % 12, i / 12] = 0;
        }

        private void AIplayer_Click(object sender, RoutedEventArgs e)
        {
            AIplayer.Visibility = Visibility.Hidden;
            twoplayer.Visibility = Visibility.Hidden;
            PlayOnline.Visibility = Visibility.Hidden;
            PC_online.Visibility = Visibility.Hidden;
            TaoButton1();           
            isCreated = true;
            ChangeSize();
            eBoard = new EvalBoard();
            fWin = false;
            fEnd = 0;
            playerFlag = 1;
            for (int i = 0; i < 144; i++)
            {
                BoardArr[i % 12, i / 12] = 0;
                BoardArrVt[i % 12, i / 12] = 0;
            }
        }
       
        private void PlayOnline_Click(object sender, RoutedEventArgs e)
        {
            AIplayer.Visibility = Visibility.Hidden;
            twoplayer.Visibility = Visibility.Hidden;
            PlayOnline.Visibility = Visibility.Hidden;
            PC_online.Visibility = Visibility.Hidden;
            TaoButton3();
            isCreated = true;
            ChangeSize();
            fWin = false;
            fEnd = 0;
            for (int i = 0; i < 144; i++)
                BoardArr[i % 12, i / 12] = 0;
            Thread AIThread = new Thread(choiCoOnline);
            AIThread.IsBackground = true;
            AIThread.Start();
        }

        private void PC_online_Click(object sender, RoutedEventArgs e)
        {
            AIplayer.Visibility = Visibility.Hidden;
            twoplayer.Visibility = Visibility.Hidden;
            PlayOnline.Visibility = Visibility.Hidden;
            PC_online.Visibility = Visibility.Hidden;
            TaoButton3();
            isCreated = true;
            ChangeSize();
            eBoard = new EvalBoard();
            fWin = false;
            fEnd = 0;
            for (int i = 0; i < 144; i++)
            {
                BoardArr[i % 12, i / 12] = 0;
                BoardArrVt[i % 12, i / 12] = 0;
            }
            Thread AIThread = new Thread(choiPcOnline);
            AIThread.IsBackground = true;
            AIThread.Start();
        }

        private void EndBatte()
        {
            AIplayer.Visibility = Visibility.Visible;
            twoplayer.Visibility = Visibility.Visible;
            PlayOnline.Visibility = Visibility.Visible;
            PC_online.Visibility = Visibility.Visible;
            chatHistory.Text = "";
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++)
                    myGrid.Children.Remove(but[i * 12 + j]);

        }
    }

}
