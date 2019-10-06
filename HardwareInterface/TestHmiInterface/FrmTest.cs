using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HardwareInterface;
using Newtonsoft.Json;

namespace TestHmiInterface
{
    public partial class FrmTest : Form
    {
        ModBus ModBus;
        public FrmTest()
        {
            InitializeComponent();

            
            ModBus = new ModBus(ModBus.ModBusMode.RTU, 1, 115200, SerialPortMode.RS232);

            Result result = ModBus.Connect("COM2");
            if (result.Success == false)
                MessageBox.Show(result.ErrorMessage);

            ModBus.OnReceiveNewResponse += ModBus_OnReceiveNewResponse;

        }

        private void ModBus_OnReceiveNewResponse(object sender, ModbusFunctions function, object response)
        {
            string json = "";
            switch (function)
            {
                case ModbusFunctions.ReadCoils:
                    var rc = (ModBusReadCoilResponse)response;
                    json = JsonConvert.SerializeObject(rc, Formatting.Indented);
                    break;
                case ModbusFunctions.ReadInputs:
                    var ri = (ModBusReadInputResponse)response;
                    json = JsonConvert.SerializeObject(ri, Formatting.Indented);
                    break;
                case ModbusFunctions.ReadHoldingRegisters:
                    var rhr = (ModBusReadHoldingRegisterResponse)response;
                    json = JsonConvert.SerializeObject(rhr, Formatting.Indented);
                    break;
                case ModbusFunctions.ReadInputRegisters:
                    var rir = (ModBusReadInputRegisterResponse)response;
                    json = JsonConvert.SerializeObject(rir, Formatting.Indented);
                    break;
                case ModbusFunctions.WriteSingleCoil:
                    break;
                case ModbusFunctions.WriteSingleRegister:
                    break;
                case ModbusFunctions.WriteMultipleCoils:
                    break;
                case ModbusFunctions.WriteMultipleRegisters:
                    break;
                case ModbusFunctions.ReadWriteMultipleRegisters:
                    break;
            }

            this.Invoke(new MethodInvoker(() => {
                textBox1.Text = json;
            }));

            
        }

        private void BtnReadCoils_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            //ModBus.ReadCoils(new ModBusReadRequest()
            //{
            //    SlaveAddress = 01,
            //    StartAddress = 02,
            //    NumberOfPoints = 1,
            //});

            ModBus.ReadHoldingRegisters(new ModBusReadRequest()
            {
                SlaveAddress = 01,
                StartAddress = 02,
                NumberOfPoints = 1,
            });
        }
    }
}
