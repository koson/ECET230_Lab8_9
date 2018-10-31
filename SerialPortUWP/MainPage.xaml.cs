using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;   //fuck yeah tasks are cool
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SerialPortUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private SerialDevice serialPort = null;     //Our port/device

        DataWriter dataWriterObject = null;         //So we can write
        DataReader dataReaderObject = null;         //So we can read
            
        private ObservableCollection<DeviceInformation> listOfDevices;      //Our device list

        private CancellationToken readCancellationTokenSource;      //Cancelation Token


        public MainPage()
        {
            this.InitializeComponent();

            listOfDevices = new ObservableCollection<DeviceInformation>();  //Prepare our list

            ListAvailablePorts();   //Get a port
        }

        //Get every connected device in a list
        private async void ListAvailablePorts() {
            try {   //I love try catch
                string aqs = SerialDevice.GetDeviceSelector();          
                var dis = await DeviceInformation.FindAllAsync(aqs);    //get all the devices

                for(int i = 0; i < dis.Count; i++) {        //This is a for loop
                    listOfDevices.Add(dis[i]);      //Add them to our list 1 by 1 (so we dont have to await to fetch it each time?)
                }

                lstSerialDevices.ItemsSource = listOfDevices; //Show list in XAML

                lstSerialDevices.SelectedIndex = -1;
            }
            catch(Exception ex) {
                txtMessage.Text = ex.Message;       //Dont message your ex, bad idea
            }
        }

        private void btnConnectToDevice_Click(object sender, RoutedEventArgs e) {

        }
    }
}
