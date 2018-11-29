using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using LibUsbDotNet.DeviceNotify;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public DateTime LastDataEventDate = DateTime.Now;
        public UsbDevice MyUsbDevice;
        public UsbEndpointReader reader;
        public UsbEndpointWriter writer;

        #region SET YOUR USB Vendor and Product ID!

        public UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1155, 22352);
        public IDeviceNotifier UsbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        void USBDeviceDeinit()
        {
            ErrorCode ec = ErrorCode.None;
            if (MyUsbDevice != null && !MyUsbDevice.IsOpen)
            {
                try
                {
                    MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                    // If the device is open and ready
                    if (MyUsbDevice == null) throw new Exception("Device Not Found.");
                    IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // This is a "whole" USB device. Before it can be used, 
                        // the desired configuration and interface must be selected.

                        // Select config #1
                        wholeUsbDevice.SetConfiguration(1);

                        // Claim interface #0.
                        wholeUsbDevice.ClaimInterface(0);
                    }

                    // open read endpoint 1.
                    reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                    // open write endpoint 1.
                    writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                    reader.DataReceived += (OnRxEndPointData);
                    reader.DataReceivedEnabled = true;

                    Thread.Sleep(100);

                    // Always disable and unhook event when done.
                    //reader.DataReceivedEnabled = false;
                    //reader.DataReceived -= (OnRxEndPointData);

                    Console.WriteLine("\r\nDone!\r\n");
                }
                catch (Exception ex)
                {

                }
            }
        }

        void readOnly()
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);


                byte[] readBuffer = new byte[1024];
                while (ec == ErrorCode.None)
                {
                    int bytesRead;

                    // If the device hasn't sent data in the last 5 seconds,
                    // a timeout error (ec = IoTimedOut) will occur. 
                    ec = reader.Read(readBuffer, 5000, out bytesRead);

                    if (bytesRead == 0) throw new Exception(string.Format("{0}:No more bytes!", ec));
                    Console.WriteLine("{0} bytes read", bytesRead);

                    // Write that output to the console.
                    Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));
                }

                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }
            }
        }

        void readWriteERventDrivent()
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");


                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                reader.ReadThreadPriority = System.Threading.ThreadPriority.Highest;
                reader.ReadBufferSize = 64;
                // open write endpoint 1.
                writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                // Remove the exepath/startup filename text from the begining of the CommandLine.
                string cmdLine = Regex.Replace(
                    Environment.CommandLine, "^\".+?\"^.*? |^.*? ", "", RegexOptions.Singleline);

                if (!String.IsNullOrEmpty(cmdLine))
                {
                    reader.DataReceived += (OnRxEndPointData);
                    reader.DataReceivedEnabled = true;

                    int bytesWritten;
                    ec = writer.Write(Encoding.Default.GetBytes(cmdLine), 2000, out bytesWritten);
                    if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);

                    LastDataEventDate = DateTime.Now;
                    while ((DateTime.Now - LastDataEventDate).TotalMilliseconds < 100)
                    {
                    }

                    // Always disable and unhook event when done.
                    //reader.DataReceivedEnabled = false;
                    //reader.DataReceived -= (OnRxEndPointData);

                    Console.WriteLine("\r\nDone!\r\n");
                }
                else
                    throw new Exception("Nothing to do.");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                //if (MyUsbDevice != null)
                //{
                //    if (MyUsbDevice.IsOpen)
                //    {
                //        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                //        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                //        // 'wholeUsbDevice' variable will be null indicating this is 
                //        // an interface of a device; it does not require or support 
                //        // configuration and interface selection.
                //        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                //        if (!ReferenceEquals(wholeUsbDevice, null))
                //        {
                //            // Release interface #0.
                //            wholeUsbDevice.ReleaseInterface(0);
                //        }
                //        MyUsbDevice.Close();
                //    }
                //}
                //MyUsbDevice = null;

                // Free usb resources
                //UsbDevice.Exit();
            }
        }

        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            // A Device system-level event has occured
            if(e.EventType == EventType.DeviceArrival)
            {
                richTextBoxReceive.AppendText("Device arrival\n");
                USBDeviceDeinit();
            }
            else if(e.EventType == EventType.DeviceRemoveComplete)
            {
                richTextBoxReceive.AppendText("Device remove complete\n");
            }
            else
            {
                richTextBoxReceive.AppendText("Deveice Event: " + e.EventType.ToString() + "\n");
            }
            richTextBoxReceive.AppendText(e.ToString() + "\n");
        }

        private  void OnRxEndPointData(object sender, EndpointDataEventArgs e)
        {

            LastDataEventDate = DateTime.Now;

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        richTextBoxReceive.AppendText(Encoding.Default.GetString(e.Buffer, 0, e.Count) + "\n");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
            }
            else
            {
                richTextBoxReceive.AppendText(Encoding.Default.GetString(e.Buffer, 0, e.Count) + "\n");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int bytesWritten;
            ErrorCode ec = writer.Write(Encoding.ASCII.GetBytes(textBoxSend.Text), 2000, out bytesWritten);
            if (ec != ErrorCode.None)
            {
                richTextBoxReceive.AppendText("Error send\n");
            }
            else
            {
                richTextBoxReceive.AppendText("Sent " + bytesWritten + " bytes\n");
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            richTextBoxReceive.Text = "";
        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            ErrorCode ecRead;
            byte[] readBuffer = new byte[1024];
            string strRead;
            int bytesRead;
            ecRead = reader.Read(readBuffer, 1000, out bytesRead);
            strRead = Encoding.Default.GetString(readBuffer, 0, bytesRead);

            if(bytesRead != 0)
            {
                richTextBoxReceive.AppendText(strRead + "\n");
            }
            else
            {
                richTextBoxReceive.AppendText("No more byte read\n");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                reader.DataReceivedEnabled = false;
                reader.DataReceived -= (OnRxEndPointData);
                reader.Dispose();
                writer.Dispose();

                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            wholeUsbDevice.ReleaseInterface(0);
                        }
                        MyUsbDevice.Close();

                    }
                    MyUsbDevice = null;
                    UsbDevice.Exit();
                }
            
            }
            catch(Exception ex)
            {

            }
        }

        private void Init_Click(object sender, EventArgs e)
        {
            // Hook the device notifier event
            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;

            readWriteERventDrivent();
            //readOnly();

            if(MyUsbDevice != null && MyUsbDevice.IsOpen)
            {
                buttonInit.Enabled = false;
                richTextBoxReceive.AppendText("Init USB success\n");
            }
            else
            {
                richTextBoxReceive.AppendText("Init USB Fail\n");
            }
        }
    }
}
