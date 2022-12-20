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
    public class ComManager
    {
        #region Private variables
        private static ComManager _instance;
        private SerialPort _sp;
        private Thread _readThread = null;
        private Thread _writeThread = null;
        private SyncList<Packet> _lstSendList = new SyncList<Packet>();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        bool _isRep = false;
        private Settings _settings;
        #endregion

        #region Events
        public event ReceiveNewPacketEvent OnReceiveNewPacket;
        public event TimeoutEvent OnTimeout;
        #endregion

        private ComManager()
        {
            _settings = SettingManager.Instance.Settings;
        }

        public bool IsConnected
        {
            get
            {
                if (_sp == null)
                    return false;

                return _sp.IsOpen;
            }
        }

        public static ComManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ComManager();

                return _instance;
            }
        }

        public Result OpenSerialPort(string portName)
        {
            try
            {
                if (_sp != null && _sp.IsOpen)
                    return Result.Okay;

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
            if (_sp == null || _sp.IsOpen == false)
            {
                return Result.PortIsNotOpened;
            }

            if (_readThread != null)
            {
                return Result.AlreadyStarted;
            }

            _readThread = new Thread(ReadThreadHandler);
            _readThread.IsBackground = true;
            _readThread.Start();

            _writeThread = new Thread(WriteThreadHandler);
            _writeThread.IsBackground = true;
            _writeThread.Start();

            return Result.Okay;
        }

        public Result SendPacket(Packet packet, bool force = false)
        {
            if (_lstSendList.Count > _settings.MaxSendListCapacity)
                return Result.FullBuffer;

            if (force)
            {
                SendCommand(packet, 0);
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

        private bool IsRep
        {
            get
            { 
                _lock.EnterReadLock();
                try
                {
                    return _isRep;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _isRep = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        private void WriteThreadHandler()
        {
            byte seqNum = 0;
            int tryCount = 0;
            while (true)
            {
                try
                {
                    if (_lstSendList.Count > 0)
                    {
                        Packet p = _lstSendList.TakeFirst();

                        IsRep = false;
                        if (p.SeqNum == 0)
                            SendCommand(p, seqNum++);
                        else
                            SendCommand(p, p.SeqNum);

                        if (p.IsWaitForResponse)
                        {
                            tryCount = 0;
                            while (true)
                            {
                                if (!IsRep)
                                {
                                    if (tryCount++ > _settings.TryToSendPacket)
                                    {
                                        OnTimeout?.Invoke(this, p);
                                        break;
                                    }

                                    Thread.Sleep(_settings.DelayBetweenTwoPacket);
                                }
                                else
                                {
                                    Thread.Sleep(_settings.DelayBetweenTwoPacket);
                                    break;
                                }
                            }
                        }
                        else
                            Thread.Sleep(_settings.DelayBetweenTwoPacket);

                        seqNum++;
                    }
                    else
                        Thread.Sleep(_settings.DelayBetweenTwoPacket);
                }
                catch(Exception e)
                {
                    Logger.Instance.AddError(this, "WriteThreadHandler", e.Message);
                }
            }
        }

        private void ReadThreadHandler()
        {
            //! Check receive
            int status = 0;
            Commands tmpCommand = Commands.UnknownCommand;
            byte tmpCommandParam = 0;
            int tmpDataIndex = 0;
            int tmpDataLenght = 0;
            byte tmpSeqNum = 0;
            byte[] tmpData = new byte[1];
            byte checkSum = 0;
            while (true)
            {
                try
                {
                    int bytesToRead = _sp.BytesToRead;
                    if (status == 0 && bytesToRead < 8)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    if (bytesToRead > 0)
                    {
                        byte b = (byte)_sp.ReadByte();
                        if (status == 0 && b == 0x55)
                        {
                            status++;
                            continue;
                        }

                        if (status == 1 && b == 0xAA)
                        {
                            status++;
                            checkSum = 0;
                            tmpDataLenght = 0;
                            tmpDataIndex = 0;
                            continue;
                        }

                        if (status == 2)
                        {
                            status++;
                            tmpSeqNum = b;
                            checkSum += b;
                            continue;
                        }

                        if (status == 3)
                        {
                            if (b >= (byte)Commands.UnknownCommand)
                            {
                                status = 0;
                                continue;
                            }

                            tmpCommand = (Commands)b;
                            checkSum += b;
                            status++;
                            continue;
                        }

                        if (status == 4)
                        {
                            tmpCommandParam = b;
                            checkSum += b;
                            status++;
                            continue;
                        }

                        if (status == 5)
                        {
                            tmpDataLenght = (int)(b << 8);
                            checkSum += b;
                            status++;
                            continue;
                        }

                        if (status == 6)
                        {
                            tmpDataLenght |= b;
                            checkSum += b;
                            tmpData = new byte[tmpDataLenght];
                            status++;
                            continue;
                        }

                        if (status == 7)
                        {
                            tmpData[tmpDataIndex++] = b;
                            checkSum += b;

                            if (tmpDataIndex >= tmpData.Length)
                            {
                                status++;
                                continue;
                            }

                            continue;
                        }

                        if (status == 8)
                        {
                            status = 0;
                            if (checkSum == b)
                            {
                                var p = new Packet()
                                {
                                    Command = tmpCommand,
                                    CommandParam = tmpCommandParam,
                                    Data = tmpData,
                                    SeqNum = tmpSeqNum,
                                };

                                IsRep = true;

                                OnReceiveNewPacket?.Invoke(this, p);
                            }
                        }
                    }
                    else
                        Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Logger.Instance.AddError(this, "ReadPortThreadHandler", e.Message);
                    status = 0;
                }
            }
        }

        private void SendCommand(Packet packet, byte seqNum)
        {
            if (_sp == null || _sp.IsOpen == false)
                return;

            byte[] data;
            if (packet.Data != null && packet.Data.Length > 0)
            {
                data = new byte[8 + packet.Data.Length];
            }
            else
                data = new byte[8];

            int n = 0;
            byte checkSum = 0;

            //! Start Byte 1 (Source Address)
            data[n++] = 0x55;
            //! Start Byte 2 (Destination Address)
            data[n++] = 0xAA;

            data[n++] = seqNum;
            checkSum += seqNum;

            //! Command
            data[n++] = (byte)(packet.Command);
            checkSum += (byte)(packet.Command);

            //! Command Parameter
            data[n++] = packet.CommandParam;
            checkSum += packet.CommandParam;

            if ( packet.Data != null && packet.Data.Length > 0)
            {
                data[n++] = (byte)(packet.Data.Length >> 8); // MSB of Data Lenght
                checkSum += (byte)(packet.Data.Length >> 8);

                data[n++] = (byte)(packet.Data.Length); // LSB of Data Lenght
                checkSum += (byte)(packet.Data.Length);

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
                data[n++] = 0;
            }
            
            //! Checksum
            data[n++] = checkSum;

            _sp.Write(data, 0, n);
        }

    }
}
