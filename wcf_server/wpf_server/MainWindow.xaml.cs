using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace wpf_server
{
    public partial class MainWindow : Window
    {
        Dictionary<IPEndPoint, Border> _users = new Dictionary<IPEndPoint, Border>(); // Image
        public static MainWindow _;
        //  static int index = 0;
        UdpClient udpClient = new UdpClient();
        private static IPEndPoint _consumerEndPoint;


        public MainWindow()
        {
            InitializeComponent();
            _ = this;
            // CloseButton.Enabled = false;
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            MessageBox.Show(string.Join("\n", host.AddressList
                .Where(i => i.AddressFamily == AddressFamily.InterNetwork)));

            Task.Run(OnUserConnect);
            Task.Run(OnGetSreen);
        }

        private async void OnGetSreen()
        {
            var port = int.Parse(ConfigurationManager.AppSettings.Get("port")); // ошибка
            var client = new UdpClient(port);
            while (true)
            {
                if (_consumerEndPoint == null) continue;

                try
                {
                    var data = await client.ReceiveAsync();

                    if (_users.ContainsKey(data.RemoteEndPoint))
                    {
                        SetImage(data.RemoteEndPoint, data.Buffer);
                    }
                }
                catch
                {

                }

            }
        }

        private async void OnUserConnect()
        {
            var playerConnectedPort = int.Parse(ConfigurationManager.AppSettings.Get("playerConnectedPort"));
            var playerConnectedClient = new UdpClient(playerConnectedPort);

            while (true)
            {
                if (_consumerEndPoint != null) continue;

                try
                {
                    var playerConnectedData = await playerConnectedClient.ReceiveAsync();

                    if (!_users.ContainsKey(playerConnectedData.RemoteEndPoint))
                    {
                        _users.Add(playerConnectedData.RemoteEndPoint, AddPictureBox());


                        //TheardForm.Call(() =>
                        //{
                        //    _users[playerConnectedData.RemoteEndPoint].Click += (sender1, e1) =>
                        //    {
                        //        if (_consumerEndPoint == null)
                        //        {
                        //            _consumerEndPoint = playerConnectedData.RemoteEndPoint;
                        //            _users[_consumerEndPoint].Dock = DockStyle.Fill;
                        //            _users[_consumerEndPoint].SizeMode = PictureBoxSizeMode.StretchImage;
                        //            CloseButton.Enabled = true;
                        //            SendMessage("true");
                        //        }
                        //    };
                        //    //_users[playerConnectedData.RemoteEndPoint].Click += pictureBox1_DoubleClick;
                        //});
                    }
                    SetImage(playerConnectedData.RemoteEndPoint, playerConnectedData.Buffer);
                }
                catch
                {
                }


            }
        }


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (_consumerEndPoint == null) return;
                var consumerEndPoint = new IPEndPoint(_consumerEndPoint.Address, 48655);

                try
                {
                    // int CursorX = Cursor.Position.X;
                    // int CursorY = Cursor.Position.Y;

                    string pose = ""; // $"{CursorX} {CursorY}";
                    SendMessageInActiveClient(pose);
                }
                catch
                {

                }
            }
        }



        public void SetImage(IPEndPoint ipEndPoint, byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var pictureBox = _users[ipEndPoint];
                // TheardForm.Call(() => pictureBox.Image = new Bitmap(ms));
            }
        }

        public Border AddPictureBox()
        {
            Border border = new Border
            {
                Width = 100,
                Height = 160,
                Margin = new Thickness(10),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(1),
            };

            Grid grid = new Grid();
            border.Child = grid;

            Image image = new Image
            {
                Stretch = Stretch.Fill,
            };
            grid.Children.Add(image);

            Border border2 = new Border()
            {
                Height = 25,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#35150a"),
                VerticalAlignment = VerticalAlignment.Bottom,
                Opacity = 0.7,
            };
            grid.Children.Add(border2);

            Label label = new Label
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = Brushes.White,
                Content = "Label"
            }; // можно заменить на x:Name, вроде обращение будет легче
            grid.Children.Add(label);



            // TheardForm.Call(() => Controls.Add(temp));
            return border;
        }



        private void SendMessageInActiveClient(string message)
        {
            var consumerEndPoint = new IPEndPoint(_consumerEndPoint.Address, 48655);
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            udpClient.Send(bytes, bytes.Length, consumerEndPoint);
        }

        private void SendMessage(string message)
        {
            foreach (var user in _users.Keys)
            {
                var consumerEndPoint = new IPEndPoint(user.Address, 48655);
                byte[] bytes = Encoding.ASCII.GetBytes(message);
                udpClient.Send(bytes, bytes.Length, consumerEndPoint);
            }
        }

        private void CloseButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_consumerEndPoint == null) return;
            // _users[_consumerEndPoint].Dock = DockStyle.None;
            // _users[_consumerEndPoint].SizeMode = PictureBoxSizeMode.Normal;

            SendMessage("false");
            _consumerEndPoint = null;
            // CloseButton.Enabled = false;
        }

        //private void Form1_FormClosing(object sender, EventArgs e)
        //{
        //    SendMessage("false");
        //    //// это мое - this.Close() или this.Hide()?
        //}
    }
}
