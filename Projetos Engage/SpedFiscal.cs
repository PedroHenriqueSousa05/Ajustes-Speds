using FiscalBr.EFDFiscal;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using FiscalBr.EFDContribuicoes;
using ProjetoSpeds.Common;
using ProjetoSpeds.Funções;

namespace ProjetoSpeds
{
    public partial class SpedFiscal : Form
    {
        public SpedFiscal()
        {
            InitializeComponent();
        }
        private ArquivoEFDFiscal _Sped = new();
        private ArquivoEFDContribuicoes _SpedContribuicoes = new();
        private ArquivoEFDFiscal _Sped_Inventario = new();
        private FuncoesAjustes ajustesFuncoes = new();
        private GravarArquivo gravarArquivo = new();
        private LerSped lerSped = new();
        private gerarRelatorio relatorio = new();

        private async void BtnSelSped_Click(object sender, EventArgs e)
        {
            var arquivo = Common.Common.EscolheArquivo(Multiselect: false);
            if (arquivo != null)
            {
                await LeSped(arquivo);
                processarSpedFiscal();
            }
        }
        private async Task LeSped(string caminho)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                await Task.Run(() => _Sped.Ler(caminho, Encoding.GetEncoding(1252)));
                var bloco0 = _Sped.Bloco0.Reg0000;
                
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Arquivo Nao Encontrado");
            }
            catch
            {
                MessageBox.Show("Arquivo Invalido");
            }
        }

        private void processarSpedFiscal()
        {
            bool VerificaCb = false;

            if (CbAjustec190.Checked)
            {
                ajustesFuncoes.ajusteC190(this._Sped);
                VerificaCb = true;
            }
            if (CbAjustec100.Checked)
            {
                ajustesFuncoes.ajusteC100(this._Sped);
                VerificaCb = true;
            }
            if (CbAjuste0205.Checked)
            {
                ajustesFuncoes.ajuste0205Fiscal(this._Sped);
                VerificaCb = true;
            }
            if (CbAjusteIcmsVl.Checked)
            {
                ajustesFuncoes.ajusteVlIcmsC100(this._Sped);
                VerificaCb = true;
            }
            if (CbExcluirListadosFiscal.Checked)
            {
                ajustesFuncoes.excluirListadoFiscal(this._Sped);
                VerificaCb = true;
            }
            if (CbAjusteIE.Checked)
            {
                ajustesFuncoes.ajusteIEFiscal(this._Sped);
                VerificaCb = true;
            }
           
            if (CbGerarReg.Checked)
            {
                ajustesFuncoes.registrofaltamcontri(this._Sped);
            }
            if (CbGerarFiscalReg.Checked)
            {
                ajustesFuncoes.registrofaltamFiscal(this._Sped);
            }
            if (CbRelatorio.Checked)
            {
                relatorio.relatorioCSV(this._Sped);
            }
            if (VerificaCb)
            {
                gravarArquivo.ExecSped(this._Sped, this);
            }
        }
    }
}
