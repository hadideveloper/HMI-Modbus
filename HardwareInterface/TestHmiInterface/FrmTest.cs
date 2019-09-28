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

namespace TestHmiInterface
{
    public partial class FrmTest : Form
    {
        ModBus ModBus;
        public FrmTest()
        {
            InitializeComponent();

            ModBus = new ModBus(ModBus.ModBusMode.RTU, 1, 19200);

            Result result = ModBus.Connect("COM1");
            if (result.Success == false)
                MessageBox.Show(result.ErrorMessage);

            ModBus.OnReceiveNewResponse += ModBus_OnReceiveNewResponse;

        }

        private void ModBus_OnReceiveNewResponse(object sender, ModBusResponse response)
        {
            
        }

        private void BtnReadCoils_Click(object sender, EventArgs e)
        {
            ModBus.ReadCoils(new ModBusRequest()
            {
                SlaveAddress = 17,
                StartAddress = 19,
                NumberOfPoints = 37,
            });
        }
    }
}
