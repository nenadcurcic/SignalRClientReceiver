using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace SignalRClientReceiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private HubConnection connection;
        private bool isConnected = false;

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void WriteToLog(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                logList.SelectedIndex = logList.Items.Count - 1;
                logList.ScrollIntoView(logList.SelectedItem);
                logList.Items.Add($"{DateTime.Now} >>> {msg}");
            });
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                await Connect();
            }
            else
            {
                await connection.DisposeAsync();
            }
        }

        public async Task Connect()
        {
            connection = new HubConnectionBuilder()
              .WithUrl(txtUrl.Text)
              .Build();
            try
            {
                WriteToLog("Connecting ...");
                await connection.StartAsync();
                WriteToLog("Connected ...");
                IsConnected = true;
                await connection.InvokeAsync("RegisterReceiver");
                WriteToLog("Client registered");

                connection.Closed += (error) =>
                {
                    IsConnected = false;
                    return Task.CompletedTask;
                };

                connection.On<string>("ReceiveMessage", async (msg) =>
                {
                    WriteToLog($"Message received => {msg}");
                    Thread.Sleep(3000);
                    await connection.InvokeAsync("Finished");
                    WriteToLog($"Reported finished");
                });

            }
            catch (Exception ex)
            {
                logList.Items.Add(ex.Message);
            }
        }
    }
}
