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
    public partial class SpedContri : Form
    {
        public SpedContri()
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
        private async void BtnSelSpedContribucoes_Click(object sender, EventArgs e)
        {
            var arquivo = Common.Common.EscolheArquivo(Multiselect: false);
            if (arquivo != null)
            {

                await lerSped.LeSpedContribuicoes(arquivo, this._SpedContribuicoes, this);
                processarSpedContribuicoes();
            }
        }
        private void processarSpedContribuicoes()
        {
            bool VerificaCb = false;

            if (CbAjustec100.Checked)
            {
                ajustesFuncoes.ajusteC100(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbAjuste0150.Checked)
            {
                ajustesFuncoes.ajuste0150(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbAjusted500.Checked)
            {
                ajustesFuncoes.ajusteD500(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbAjustarIE.Checked)
            {
                ajustesFuncoes.ajusteIE(this._SpedContribuicoes);
                VerificaCb = true;
            }
            
            if (CbAjuste0205.Checked)
            {
                ajustesFuncoes.ajuste0205(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbAjusted101.Checked)
            {
                ajustesFuncoes.ajusteD101(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbAjustea170.Checked)
            {
                ajustesFuncoes.ajusteA170(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbExcC100.Checked)
            {
                ajustesFuncoes.excluirC100(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbAjusteDCst.Checked)
            {
                ajustesFuncoes.ajusteDCst(this._SpedContribuicoes);
                VerificaCb = true;
            }

            if (CbReg0200.Checked)
            {
                ajustesFuncoes.ajuste0200(this._SpedContribuicoes);
                VerificaCb = true;
            }
            if (CbAjusteCcst.Checked)
            {
                ajustesFuncoes.ajusteCstC170(this._SpedContribuicoes);
                VerificaCb = true;
            }
            
            if (CbExcluirList.Checked)
            {
                ajustesFuncoes.excluirListado(this._SpedContribuicoes);
                VerificaCb = true;
            }
            if (CbExcluirCFOP.Checked)
            {
                ajustesFuncoes.excluirCFOPsListados(this._SpedContribuicoes);
                VerificaCb = true;
            }
            if (CbAjustePisConfins.Checked)
            {
                ajustesFuncoes.corrgirVlPisCofins(this._SpedContribuicoes);
                VerificaCb = true;
            }
            if (CbAjustePneus.Checked)
            {
                ajustesFuncoes.ajusteCstCofins(this._SpedContribuicoes);
                VerificaCb = true;
            }
            if (CbGerarTxt.Checked)
            {
                ajustesFuncoes.registrosFaltam(this._SpedContribuicoes);
            }

            if (CbRelatorio.Checked)
            {
                relatorio.relatorioCSV(this._SpedContribuicoes);
                
            }
            //ajustesFuncoes.ajusteTemporario(this._SpedContribuicoes);
            //VerificaCb = true;
            if (VerificaCb)
            {
                gravarArquivo.ExecSped(this._SpedContribuicoes, this);
            }
        }

        private void selectAllCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control control in Controls)
            {
                if (control is CheckBox checkBox && checkBox != selectAllCheckBox)
                {
                    checkBox.Checked = selectAllCheckBox.Checked;
                }
            }
        }
    }
}