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
        private SolarCalc solarCalc = new SolarCalc();

        DataWriter dataWriterObject = null;         //So we can write
        DataReader dataReaderObject = null;         //So we can read
            
        private ObservableCollection<DeviceInformation> listOfDevices;      //Our device list

        private CancellationTokenSource readCancellationTokenSource;      //Cancelation Token

        string received = "";
        private Int32 an0;
        private Int32 an1;
        private Int32 an2;
        private Int32 an3;
        private Int32 an4;
        private Int32 an5;
        private Int32 recChkSum;


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
                var dis = await DeviceInformation.FindAllAsync(aqs);    //wait until done] get all the devices

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


        //CONNECT TO DEVICE
        //Click to initiate
        private void btnConnectToDevice_Click(object sender, RoutedEventArgs e) {
            SerialPortConfiguration();
        }
        //Serial Config
        private async void SerialPortConfiguration() {
            var selection = lstSerialDevices.SelectedItems;
            //Nothing selected
            if (selection.Count <= 0) {
                txtMessage.Text = "You forgot to pick a device";
                return;
            }
            //Somthing selected
            DeviceInformation entry = (DeviceInformation)selection[0]; //only use first selection

            try {   //Try to set up the serial port to 8and1N configuration
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);  //how long to try before giving up
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);   //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                serialPort.BaudRate = 115200;                   //baud rate, must match device
                serialPort.Parity = SerialParity.None;          //n
                serialPort.StopBits = SerialStopBitCount.One;   //and1
                serialPort.DataBits = 8;                        //8
                serialPort.Handshake = SerialHandshake.None;    //or is this n?
                txtMessage.Text = "Serial port correctly configured!";      //Report success to user

                readCancellationTokenSource = new CancellationTokenSource();        //cancellation token

                Listen();   //begin listening
            }
            catch (Exception ex) {
                txtMessage.Text = ex.Message;   //Shows your error // fitting?
            }
        }
        //Listen
        private async void Listen() {//   !!while(true) inside!!
            try {
                if(serialPort != null) { //Ensure there's actually a device
                    dataReaderObject = new DataReader(serialPort.InputStream); //get ready to read

                    while (true) {
                        await ReadData(readCancellationTokenSource.Token);
                    }
                }
            }
            catch(Exception ex) {
                txtMessage.Text = ex.Message;

                //if (ex.GetType.Name=="TaskCanelledExcption")      //Wayne left this here commented
            }
            finally {

            }
        }


        //Read Data] Runs continuously in a while(true) loop
        private async Task ReadData(CancellationToken cancellationToken) {
            Task<UInt32> loadAsyncTask;

            int calChkSum = 0;

            uint ReadBufferLength = 1;

            cancellationToken.ThrowIfCancellationRequested();

            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            UInt32 bytesRead = await loadAsyncTask;

            if (bytesRead > 0) {
                received += dataReaderObject.ReadString(bytesRead);
                //txtReceived.Text = received + txtReceived.Text;

                if(received[0] == '#') {    //checking the packet follows protocol    ###
                    if (received.Length > 3) {      //Is it a complete packet
                        if(received[2] == '#') {        //Is it still following protocol?
                            if (received.Length > 42) {     //Full length?

                                #region Display Raw Data
                                txtReceived.Text = received + txtReceived.Text;
                                ////////////////   ### is being matched on the arduino, both sides expect this key
                                //Parsing code//   ###,packetId,A0,A1,A2,A3,A4,A5
                                ////////////////                                
                                txtPacketNum.Text = received.Substring(3, 3);   // 3 is where packet num start and it is 3 long] 0,1,2 are ###
                                txtAN0.Text = received.Substring(6, 4);         //Start at 6, 4 long] Analog 0
                                txtAN1.Text = received.Substring(10, 4);        //Start at 10, 4 long] Analog 0
                                txtAN2.Text = received.Substring(14, 4);        //Start at 14, 4 long] Analog 0
                                txtAN3.Text = received.Substring(18, 4);        //Start at 18, 4 long] Analog 0
                                txtAN4.Text = received.Substring(22, 4);        //Start at 22, 4 long] Analog 0
                                txtAN5.Text = received.Substring(26, 4);        //Start at 26, 4 long] Analog 0
                                txtBinOut.Text = received.Substring(30, 8);     //8 bit binary inputs
                                txtChkSum.Text = received.Substring(38, 3);     //Check Sum
                                #endregion
                                

                                #region Calculate the Check Sum and prepare to compare
                                for (int i = 3; i < 38; i++) {
                                    calChkSum += (byte)received[i];
                                }
                                txtCalChkSum.Text = Convert.ToString(calChkSum);
                                
                                recChkSum = Convert.ToInt32(received.Substring(38, 3));
                                calChkSum %= 1000;
                                #endregion
                                #region Packet is Legit
                                if (recChkSum == calChkSum) {
                                    //Start Reading packet
                                    #region Get Analog Values
                                    an0 = Convert.ToInt32(received.Substring(6, 4));    //Solar Voltage
                                    an1 = Convert.ToInt32(received.Substring(10, 4));   //Reference V for Current
                                    an2 = Convert.ToInt32(received.Substring(14, 4));   //Battery Voltage
                                    an3 = Convert.ToInt32(received.Substring(18, 4));   //Led1
                                    an4 = Convert.ToInt32(received.Substring(22, 4));   //Led2
                                    an5 = Convert.ToInt32(received.Substring(26, 4));
                                    #endregion

                                    txtSolarVolt.Text = solarCalc.GetSolarVoltage(an0);             //evoke solar calc classes to do our math
                                    txtBatteryVolt.Text = solarCalc.GetBatteryVoltage(an2);
                                    txtBatteryCurrent.Text = solarCalc.GetBatteryCurrent(an1, an2);
                                    txtLed1Current.Text = solarCalc.GetLedCurrent(an3, an1);
                                    txtLed2Current.Text = solarCalc.GetLedCurrent(an4, an1);
                                }
                                #endregion
                                received = "";//Clear the buffer for the next pass
                            }
                            
                        }
                        else {                          //It's not yo
                            received = "";
                        }
                    }
                }
                else {                    //Otherwise clear the buffer
                    received = "";
                }
            }
        }



        //Send Data]
        private async void btnWrite_Click(object sender, RoutedEventArgs e) {
            if(serialPort != null) {    //dont send if nothing to send to
                var dataPacket = txtSend.Text.ToString();
                dataWriterObject = new DataWriter(serialPort.OutputStream);
                await SendPacket(dataPacket);

                if (dataWriterObject != null) {     //Clear the datawriter
                    dataWriterObject.DetachStream();
                    dataWriterObject = null;
                }
            }
        }

        private async Task SendPacket(string value) {
            var dataPacket = value;

            Task<UInt32> storeAsyncTask;

            if (dataPacket.Length != 0) {
                dataWriterObject.WriteString(dataPacket);

                storeAsyncTask = dataWriterObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if(bytesWritten > 0) {
                    txtMessage.Text = "Value sent correctly";
                }
            }
            else {
                txtMessage.Text = "No value sent yo";
            }
        }
    }
}
