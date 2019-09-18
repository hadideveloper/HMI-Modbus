using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modbus;

namespace HardwareInterface
{
    public class HardwareInterface
    {
        private static HardwareInterface _instance;
        private SerialPort _sp;
        private Thread _thread = null;
        private SyncList<Packet> _lstSendList = new SyncList<Packet>();
        private byte _mySeqNum = 0;

        private HardwareInterface()
        {

        }

        public static HardwareInterface Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HardwareInterface();

                return _instance;
            }   
        }

        public Result OpenSerialPort(string portName)
        {
            try
            {
                _sp = new SerialPort()
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    PortName = portName.ToUpper(),
                };

                _sp.Open();

            }
            catch
            {
                return Result.CanNotOpenPort;
            }

            return Result.Okay;
        }

        public Result Start()
        {
            if(_sp == null || _sp.IsOpen == false)
            {
                return Result.PortIsNotOpened;
            }

            if(_thread != null)
            {
                return Result.AlreadyStarted;
            }

            _thread = new Thread(ThreadHandler);
            _thread.IsBackground = true;
            _thread.Start();

            return Result.Okay;
        }

        public Result SendPacket(Packet packet, bool force = false)
        {
            if (force)
            {
                SendCommand(packet);
            }
            else
                _lstSendList.Add(packet);

            return Result.Okay;
        }

        public int SendPendingPackets
        {
            get
            {
                return _lstSendList.Count;
            }
        }

        private void ThreadHandler()
        {
            while(true)
            {
                //! Check receive
                int status = 0;
                Commands cmd = Commands.UnknownCommand;
                int tmpDataLenght = 0;
                byte[] data = new byte[1];
                byte checkSum = 0;
                while (true)
                {
                    try
                    {
                        int bytesToRead = _sp.BytesToRead;
                        if (status == 0 && bytesToRead < 5)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        if (bytesToRead > 0)
                        {
                            byte b = (byte)_sp.ReadByte();
                            if (status == 0 && b == 0x55)
                            {
                                status = 1;
                                continue;
                            }

                            if (status == 1 && b == 0xAA)
                            {
                                status = 2;
                                checkSum = 0;
                                tmpDataLenght = 0;
                                continue;
                            }

                            if (status == 2)
                            {
                                if (b >= (byte)Commands.UnknownCommand)
                                {
                                    status = 0;
                                    continue;
                                }

                                cmd = (Commands)b;
                                checkSum += b;
                                status = 3;
                                continue;
                            }

                            if (status == 3)
                            {
                                data = new byte[b];
                                checkSum += b;
                                status = 4;
                                continue;
                            }

                            if (status == 4)
                            {
                                data[tmpDataLenght++] = b;
                                checkSum += b;

                                if (tmpDataLenght >= data.Length)
                                {
                                    status = 5;
                                    continue;
                                }

                                continue;
                            }

                            if (status == 5)
                            {
                                status = 0;
                                if (checkSum == b)
                                {
                                    //var e = new RelayBoardEvent()
                                    //{
                                    //    Command = cmd,
                                    //    Data = data,
                                    //    IsHandled = false,
                                    //};

                                    //if (e.Command == RelayBoardCommands.CMD_READ_INPUT)
                                    //{
                                    //    OnIoInput?.Invoke(this, e);
                                    //    if (e.IsHandled)
                                    //        continue;
                                    //}
                                    //else if (e.Command == RelayBoardCommands.CMD_READ_485)
                                    //{
                                    //    OnSerialInput?.Invoke(this, e);
                                    //    if (e.IsHandled)
                                    //        continue;
                                    //}


                                    //if (OnNewPacket != null)
                                    //{
                                    //    OnNewPacket?.Invoke(this, e);
                                    //}
                                    //else
                                    //    LstRelayBoardEvents.Add(e);
                                }


                            }
                        }
                        else
                            Thread.Sleep(10);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.ToLower() == "the port is closed.")
                        {

                        }
                    }

                }
            }
        }

        private void SendCommand(Packet packet)
        {
            if (_sp == null || _sp.IsOpen == false)
                return;

            byte[] data;
            if (packet.Data != null && packet.Data.Length > 0)
            {
                data = new byte[6 + packet.Data.Length];
            }
            else
                data = new byte[6];

            int n = 0;
            byte checkSum = 0;

            //! Start Byte 1
            data[n++] = 0x55;
            //! Start Byte 2
            data[n++] = 0xAA;

            data[n++] = _mySeqNum;
            checkSum += _mySeqNum;

            //! Command
            data[n++] = (byte)(packet.Command);
            checkSum += (byte)(packet.Command);

            if( packet.Data != null && packet.Data.Length > 0)
            {
                data[n++] = (byte)(packet.Data.Length); // +1 for checksum
                checkSum += (byte)packet.Data.Length;

                for (int i = 0; i < packet.Data.Length; i++)
                {
                    //! Data
                    data[n++] = packet.Data[i];

                    checkSum += packet.Data[i];
                }
            }
            else
            {
                data[n++] = 0;
            }
            
            //! Checksum
            data[n++] = checkSum;

            _sp.Write(data, 0, n);

            _mySeqNum++;
        }

    }
}
