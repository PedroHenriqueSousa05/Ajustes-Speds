using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProjetoSpeds;

namespace ProjetoSpeds

{
    public partial class TelaPrincipal : Form
    {
        public TelaPrincipal()
        {
            InitializeComponent();
        }

        private void SpedContriBt_Click(object sender, EventArgs e)
        {
            SpedContri spedcontri = new SpedContri();
            spedcontri.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SpedFiscal SpedFiscal = new SpedFiscal();
            SpedFiscal.Show();
        }
    }
}
