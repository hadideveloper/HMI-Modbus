using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Utility;

namespace HardwareInterface
{
    public partial class ModBus
    {
        public ModBusMode Mode { set; get; } 
        public int Port { set; get; }
        public int Baudrate { set; get; }
        public SerialPortMode SerialPortMode { set; get; }
        public HardwareInterface HardwareInterface { set; get; }

        public SyncList<ModBusReadRequest> PendingReadRequestList { set; get; }

        public event ReceiveNewModBudResponse OnReceiveNewResponse;

        private byte _seqNum = 0;

        public ModBus(ModBusMode mode, int port, int baudrate, SerialPortMode serialPortMode)
        {
            Mode = mode;
            Port = port;
            Baudrate = baudrate;
            SerialPortMode = serialPortMode;

            HardwareInterface = new HardwareInterface(port, baudrate, new Commands[] { Commands.SendOverSerialPort, Commands.InitSerialPort });

            PendingReadRequestList = new SyncList<ModBusReadRequest>();

            HardwareInterface.OnReceiceNewPacket += HardwareInterface_OnReceiceNewPacket;
        }

        public Result Connect(string hostSerialPort)
        {
            if (string.IsNullOrEmpty(hostSerialPort))
                hostSerialPort = SettingManager.Instance.Settings.ComPort;

            Result openingPcSerialPortResult = HardwareInterface.OpenHostSerialPort(hostSerialPort);
            if (openingPcSerialPortResult.Success == false)
            {
                return openingPcSerialPortResult;
            }

            return HardwareInterface.InitSerialPort(Port, Baudrate, SerialPortMode);
        }

        private bool HardwareInterface_OnReceiceNewPacket(object sender, Packet packet)
        {
            if (packet.Data == null || packet.Data.Length < 7)
                return false;

            if (packet.Data[0] == ':' && packet.Data[packet.Data.Length-2] == 0x0D && packet.Data[packet.Data.Length-1] == 0x0A)
            {
                //res.Mode = ModBusMode.ASCII;
                
            }
            else
            {
                ModbusFunctions function = (ModbusFunctions)packet.Data[1];

                ModBusReadRequest req = null;
                if(function == ModbusFunctions.ReadCoils || function == ModbusFunctions.ReadHoldingRegisters ||
                    function == ModbusFunctions.ReadInputRegisters || function == ModbusFunctions.ReadInputs)
                {
                    for (int i = 0; i < PendingReadRequestList.Count; i++)
                    {
                        ModBusReadRequest pr = PendingReadRequestList.GetAt(i);
                        if (packet.SeqNum == pr.SeqNum)
                        {
                            req = pr;
                            PendingReadRequestList.RemoveAt(i);
                            break;
                        }
                    }

                    if (req == null)
                        return false;
                }

                if (function == ModbusFunctions.ReadCoils)
                {
                    var res = new ModBusReadCoilResponse()
                    {
                        Mode = ModBusMode.RTU,
                        SlaveAddress = packet.Data[0],
                        Function = function,
                        ByteCount = packet.Data[2],
                        StartAddress = req.StartAddress,
                    };

                    res.Data = new byte[res.ByteCount];
                    for (int i = 0; i < res.ByteCount; i++)
                    {
                        res.Data[i] = packet.Data[i + 3];
                    }

                    var crc16 = ModbusUtility.ComputeCRC16(packet.Data, 0, packet.Data.Length - 2);

                    if (crc16[0] == packet.Data[packet.Data.Length - 2] && crc16[1] == packet.Data[packet.Data.Length - 1])
                        OnReceiveNewResponse?.Invoke(this, function, res);
                }
                else if (function == ModbusFunctions.ReadInputs)
                {
                    var res = new ModBusReadInputResponse()
                    {
                        Mode = ModBusMode.RTU,
                        SlaveAddress = packet.Data[0],
                        Function = function,
                        ByteCount = packet.Data[2],
                        StartAddress = req.StartAddress,
                    };

                    res.Data = new byte[res.ByteCount];
                    for (int i = 0; i < res.ByteCount; i++)
                    {
                        res.Data[i] = packet.Data[i + 3];
                    }

                    var crc16 = ModbusUtility.ComputeCRC16(packet.Data, 0, packet.Data.Length - 2);

                    if (crc16[0] == packet.Data[packet.Data.Length - 2] && crc16[1] == packet.Data[packet.Data.Length - 1])
                        OnReceiveNewResponse?.Invoke(this, function, res);
                }
                else if (function == ModbusFunctions.ReadHoldingRegisters)
                {
                    var res = new ModBusReadHoldingRegisterResponse()
                    {
                        Mode = ModBusMode.RTU,
                        SlaveAddress = packet.Data[0],
                        Function = function,
                        ByteCount = packet.Data[2],
                        StartAddress = req.StartAddress,
                    };

                    res.Data = new ushort[res.ByteCount / 2];
                    int n = 0;
                    for (int i = 0; i < res.ByteCount; i += 2)
                    {
                        res.Data[n++] = (ushort)((packet.Data[i + 3] << 8) | (packet.Data[i + 4]));
                    }

                    var crc16 = ModbusUtility.ComputeCRC16(packet.Data, 0, packet.Data.Length - 2);

                    if (crc16[0] == packet.Data[packet.Data.Length - 2] && crc16[1] == packet.Data[packet.Data.Length - 1])
                        OnReceiveNewResponse?.Invoke(this, function, res);
                }
                else if (function == ModbusFunctions.ReadInputs)
                {
                    var res = new ModBusReadInputRegisterResponse()
                    {
                        Mode = ModBusMode.RTU,
                        SlaveAddress = packet.Data[0],
                        Function = function,
                        ByteCount = packet.Data[2],
                        StartAddress = req.StartAddress,
                    };

                    res.Data = new ushort[res.ByteCount / 2];
                    int n = 0;
                    for (int i = 0; i < res.ByteCount; i += 2)
                    {
                        res.Data[n++] = (ushort)((packet.Data[i + 3] << 8) | (packet.Data[i + 4]));
                    }

                    var crc16 = ModbusUtility.ComputeCRC16(packet.Data, 0, packet.Data.Length - 2);

                    if (crc16[0] == packet.Data[packet.Data.Length - 2] && crc16[1] == packet.Data[packet.Data.Length - 1])
                        OnReceiveNewResponse?.Invoke(this, function, res);
                }
                else if(function == ModbusFunctions.WriteSingleCoil || function == ModbusFunctions.WriteSingleRegister)
                {
                    var res = new ModBusWriteSingleResponse()
                    {
                        Mode = ModBusMode.RTU,
                        SlaveAddress = packet.Data[0],
                        Function = function,
                    };

                    res.Address = (ushort)((packet.Data[2] << 8) | (packet.Data[3]));
                    res.Value = (ushort)((packet.Data[4] << 8) | (packet.Data[4]));

                    var crc16 = ModbusUtility.ComputeCRC16(packet.Data, 0, packet.Data.Length - 2);

                    if (crc16[0] == packet.Data[packet.Data.Length - 2] && crc16[1] == packet.Data[packet.Data.Length - 1])
                        OnReceiveNewResponse?.Invoke(this, function, res);
                }
            }

            return true;
        }

        public Result ReadCoils(ModBusReadRequest req)
        {
            _seqNum++;
            if (_seqNum == 0)
                _seqNum++;

            if (Mode == ModBusMode.RTU)
            {
                req.SeqNum = _seqNum;
                PendingReadRequestList.Add(req);

                byte[] message = RtuMakeFunction4(req, ModbusFunctions.ReadCoils);
                return HardwareInterface.SendOverSerialPort(Port, _seqNum, message);
            }
            
            return Result.NotImplemented;
        }

        public Result ReadInputs(ModBusReadRequest req)
        {
            _seqNum++;
            if (_seqNum == 0)
                _seqNum++;

            if (Mode == ModBusMode.RTU)
            {
                req.SeqNum = _seqNum;
                PendingReadRequestList.Add(req);

                byte[] message = RtuMakeFunction4(req, ModbusFunctions.ReadInputs);
                return HardwareInterface.SendOverSerialPort(Port, _seqNum, message);
            }

            return Result.NotImplemented;
        }

        public Result ReadHoldingRegisters(ModBusReadRequest req)
        {
            _seqNum++;
            if (_seqNum == 0)
                _seqNum++;
            if (Mode == ModBusMode.RTU)
            {
                req.SeqNum = _seqNum;
                PendingReadRequestList.Add(req);

                byte[] message = RtuMakeFunction4(req, ModbusFunctions.ReadHoldingRegisters);
                return HardwareInterface.SendOverSerialPort(Port, _seqNum, message);
            }

            return Result.NotImplemented;
        }

        public Result ReadInputRegisters(ModBusReadRequest req)
        {
            _seqNum++;
            if (_seqNum == 0)
                _seqNum++;
            if (Mode == ModBusMode.RTU)
            {
                req.SeqNum = _seqNum;
                PendingReadRequestList.Add(req);

                byte[] message = RtuMakeFunction4(req, ModbusFunctions.ReadInputRegisters);
                return HardwareInterface.SendOverSerialPort(Port, _seqNum, message);
            }

            return Result.NotImplemented;
        }

        public Result WriteSingleCoil(ModBusSingleWriteRequest req)
        {
            if (Mode == ModBusMode.RTU)
            {
                byte[] message = RtuMakeFunction5(req, ModbusFunctions.WriteSingleCoil);
                return HardwareInterface.SendOverSerialPort(Port, message);
            }

            return Result.NotImplemented;
        }

        public Result WriteSingleRegister(ModBusSingleWriteRequest req)
        {
            if (Mode == ModBusMode.RTU)
            {
                byte[] message = RtuMakeFunction5(req, ModbusFunctions.WriteSingleRegister);
                return HardwareInterface.SendOverSerialPort(Port, message);
            }

            return Result.NotImplemented;
        }

        private byte[] RtuMakeFunction5(ModBusSingleWriteRequest req, ModbusFunctions function)
        {
            var message = new byte[8];

            message[0] = (byte)req.SlaveAddress;
            message[1] = (byte)function;
            message[2] = (byte)(req.StartAddress >> 8);
            message[3] = (byte)req.StartAddress;
            message[4] = (byte)(req.NewValue >> 8);
            message[5] = (byte)req.NewValue;

            var crc16 = ModbusUtility.ComputeCRC16(message, 0, message.Length - 2);
            message[message.Length - 2] = crc16[0];
            message[message.Length - 1] = crc16[1];

            return message;
        }

        private byte[] RtuMakeFunction4(ModBusReadRequest req, ModbusFunctions function)
        {
            var message = new byte[8];

            message[0] = (byte)req.SlaveAddress;
            message[1] = (byte)function;
            message[2] = (byte)(req.StartAddress >> 8);
            message[3] = (byte)req.StartAddress;
            message[4] = (byte)(req.NumberOfPoints >> 8);
            message[5] = (byte)req.NumberOfPoints;

            var crc16 = ModbusUtility.ComputeCRC16(message, 0, message.Length - 2);
            message[message.Length - 2] = crc16[0];
            message[message.Length - 1] = crc16[1];

            return message;
        }

    }
}
