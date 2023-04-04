using ClosedXML.Excel;
using FiscalBr.EFDFiscal;
using Npgsql;
using ProjetosEngage.Classes;
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


namespace ProjetosEngage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

        public partial class MainWindow : Window
    {
        public const string engageConnectionString = "Server=engage.fabricadecodigos.com.br;Port=5433;Database=engage;User Id=sysemp;Password=@@mona!!;";
        private ArquivoEFDFiscal _Sped = new();
        private ArquivoEFDContribuicoes _SpedContribuicoes = new();
        private ArquivoEFDFiscal _Sped_Inventario = new();
        private FuncoesAjustes ajustesFuncoes = new();
        private GravarArquivo gravarArquivo = new();
        private LerSped lerSped = new();
        private gerarRelatorio relatorio = new();
        public MainWindow()
        {
            InitializeComponent();
        }
        private static async Task RelatorioCteAsync(string arquivo, DateTime dataIni, DateTime dataFin)
        {
            float index;
            float maxRows = 500;
            float totalRows = 0;
            StringBuilder csv = new();

            string commandString = " select count(xml) " +
                                   " from sysemp.entrada_nf_xml " +
                                   " where xml is not null " +
                                   " and cfop in ('6353', '6932', '5353', '5932')" +
                                  $" and dtemissao >= '{dataIni.ToString("d")}'" +
                                  $" and dtemissao <= '{dataFin.ToString("d")}'";

            using NpgsqlConnection conn = new(engageConnectionString);
            using NpgsqlCommand cmd = new(commandString, conn);

            try
            {
                await conn.OpenAsync();
                totalRows = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }

            csv.AppendLine("Numero;Chave;Cfop Cte;Cfop Nota;Flag;Cst");

            Directory.CreateDirectory(@"C:\infodesign\TEMP\XML\Cte");
            DirectoryInfo dir = new(@"C:\infodesign\TEMP\XML\Cte");

            for (index = 0; index < totalRows + maxRows; index += maxRows)
            {
                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    file.Delete();
                }

                commandString = "select xml " +
                                "from " +
                                "( " +
                                "   select xml, " +
                                "   row_number() over(order by dt_entrada) as rank " +
                                "   from sysemp.entrada_nf_xml " +
                                "   where xml is not null " +
                                "   and cfop in ('6353', '6932', '5353', '5932') " +
                               $" and dtemissao >= '{dataIni.ToString("d")}'" +
                               $" and dtemissao <= '{dataFin.ToString("d")}'" +
                               $" and id_empresa in ('1', '8')" +
                                ") as LoremIpsum " +
                               $"where rank >= {index} and rank < {index + maxRows}";

                FileStream stream;
                BinaryWriter writer;
                int bufferSize = 12288;
                byte[] buffer = new byte[bufferSize];
                long resultado;
                long startIndex;

                cmd.CommandText = commandString;
                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    await conn.CloseAsync();
                }

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                while (await reader.ReadAsync())
                {
                    string name = Path.GetRandomFileName();
                    stream = new(@$"{dir.FullName}\{name}.xml", FileMode.OpenOrCreate, FileAccess.Write);
                    writer = new(stream);
                    startIndex = 0;
                    resultado = reader.GetBytes(0, startIndex, buffer, 0, bufferSize);
                    while (resultado == bufferSize)
                    {
                        writer.Write(buffer);
                        writer.Flush();

                        startIndex += bufferSize;
                        resultado = reader.GetBytes(0, startIndex, buffer, 0, bufferSize);
                    }

                    writer.Write(buffer, 0, (int)resultado);
                    writer.Flush();

                    writer.Close();
                    stream.Close();
                }
                await reader.CloseAsync();
                await conn.CloseAsync();

                List<Nota> listaCte = new();
                string stringChaves = string.Empty;

                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    var xml = XDocument.Load(file.FullName);
                    XNamespace ns = xml.Root.Name.Namespace;

                    var a = xml.Descendants(ns + "ICMS").Elements().ToList();

                    var cte = xml.Descendants(ns + "ide")
                                   .Select(a => new Nota
                                   {
                                       Numero = (string)a.Element(ns + "nCT"),
                                       UFIni = (string)a.Element(ns + "UFIni"),
                                       UFFim = (string)a.Element(ns + "UFFim"),
                                       UFemit = xml.Descendants(ns + "enderEmit")
                                                   .Select(b => (string)b.Element(ns + "UF")).First(),
                                       Chave = xml.Descendants(ns + "infProt")
                                                  .Select(b => (string)b.Element(ns + "chCTe")).First(),
                                       cst = (string)xml.Descendants(ns + "CST").First()
                                   }).First();
                    cte.UpdateCfop();
                    stringChaves += $@"'{cte.Chave}',";
                    listaCte.Add(cte);
                }

                if (string.IsNullOrEmpty(stringChaves))
                {
                    break;
                }

                stringChaves = stringChaves.Remove(stringChaves.Length - 1, 1);

                commandString = $" select cfop, chavenfe from sysemp.entrada_nf nf " +
                                $" left join nat_operacao nat on nf.id_nat_operacao = nat.id_nat_operacao" +
                                $" where chavenfe in ({stringChaves})";

                NpgsqlDataAdapter da = new(commandString, conn);
                DataTable dt = new();

                await conn.OpenAsync();
                da.Fill(dt);
                await conn.CloseAsync();

                var listaNota = dt.AsEnumerable();

                var listaCteNota = listaCte.Join(listaNota,
                                                 cte => cte.Chave,
                                                 nota => nota.Field<string>("chavenfe"),
                                                 (cte, nota) => new { listaCte = cte, listaNota = nota })
                                           .Select(a => new
                                           {
                                               Numero = a.listaCte.Numero,
                                               Chave = a.listaCte.Chave,
                                               CfopCte = a.listaCte.Cfop,
                                               CfopNota = a.listaNota.Field<string>("cfop").Replace(".", string.Empty),
                                               Flag = a.listaCte.Cfop == a.listaNota.Field<string>("cfop").Replace(".", string.Empty) ? "Cte igual Nota" : "Cte Diferente Nota",
                                               Cst = a.listaCte.cst
                                           }).ToList();

                foreach (var nota in listaCteNota)
                {
                    csv.AppendLine($"{nota.Numero};{nota.Chave.Trim()};{nota.CfopCte};{nota.CfopNota};{nota.Flag};{nota.Cst}");
                }
            }

            foreach (FileInfo file in dir.EnumerateFiles())
            {
                file.Delete();
            }

            File.WriteAllText($"{arquivo}", csv.ToString());
            MessageBox.Show("Feito");
        }
        private static async Task RelatorioNotaSaidaAsync(string arquivo, DateTime dataIni, DateTime dataFin)
        {
            float index;
            float maxRows = 500;
            float totalRows = 0;
            StringBuilder csv = new();

            string commandString = " select count(arquivo) " +
                                   " from sysemp.nota_saida_xml " +
                                   " where arquivo is not null " +
                                  $" and dthr_atualizacao >= '{dataIni:d}'" +
                                  $" and dthr_atualizacao <= '{dataFin:d}'";

            using NpgsqlConnection conn = new(engageConnectionString);
            using NpgsqlCommand cmd = new(commandString, conn);

            try
            {
                await conn.OpenAsync();
                totalRows = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }

            csv.AppendLine("Numero;Chave;Data Emissao Xml;Data Emissao Nota;Flag");

            Directory.CreateDirectory(@"C:\infodesign\TEMP\XML\Cte");
            DirectoryInfo dir = new(@"C:\infodesign\TEMP\XML\Cte");

            for (index = 0; index < totalRows + maxRows; index += maxRows)
            {
                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    file.Delete();
                }

                commandString = "select arquivo " +
                                "from " +
                                "( " +
                                "   select arquivo, " +
                                "   row_number() over(order by dthr_atualizacao) as rank " +
                                "   from sysemp.nota_saida_xml " +
                                "   where arquivo is not null " +
                               $" and dthr_atualizacao >= '{dataIni:d}'" +
                               $" and dthr_atualizacao <= '{dataFin:d}'" +
                                ") as LoremIpsum " +
                               $"where rank >= {index} and rank < {index + maxRows}";

                FileStream stream;
                BinaryWriter writer;
                int bufferSize = 12288;
                byte[] buffer = new byte[bufferSize];
                long resultado;
                long startIndex;

                cmd.CommandText = commandString;
                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    await conn.CloseAsync();
                }

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                while (await reader.ReadAsync())
                {
                    string name = Path.GetRandomFileName();
                    stream = new(@$"{dir.FullName}\{name}.xml", FileMode.OpenOrCreate, FileAccess.Write);
                    writer = new(stream);
                    startIndex = 0;
                    resultado = reader.GetBytes(0, startIndex, buffer, 0, bufferSize);
                    while (resultado == bufferSize)
                    {
                        writer.Write(buffer);
                        writer.Flush();

                        startIndex += bufferSize;
                        resultado = reader.GetBytes(0, startIndex, buffer, 0, bufferSize);
                    }

                    writer.Write(buffer, 0, (int)resultado);
                    writer.Flush();

                    writer.Close();
                    stream.Close();
                }
                await reader.CloseAsync();
                await conn.CloseAsync();

                List<Nota> listaNotasXml = new();
                string stringChaves = string.Empty;

                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    var xml = XDocument.Load(file.FullName);
                    XNamespace ns = xml.Root.Name.Namespace;
                    try
                    {
                        var nota = xml.Descendants(ns + "ide")
                                       .Select(a => new Nota
                                       {
                                           DataEmissao = (string)a.Element(ns + "dhEmi"),
                                           Chave = xml.Descendants(ns + "NFe")
                                                  .Select(b => b.Element(ns + "infNFe").Attribute("Id").Value.Replace("NFe", "")).First()
                                       }).First();
                        stringChaves += $@"'{nota.Chave}',";
                        listaNotasXml.Add(nota);
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(stringChaves))
                {
                    break;
                }

                stringChaves = stringChaves.Remove(stringChaves.Length - 1, 1);

                commandString = "   select data_emissao, chavenfe, id_nr_nf " +
                                "   from sysemp.nota_saida " +
                               $"   where chavenfe in ({stringChaves}) ";

                NpgsqlDataAdapter da = new(commandString, conn);
                DataTable dt = new();

                await conn.OpenAsync();
                da.Fill(dt);
                await conn.CloseAsync();

                var listaNotasBanco = dt.AsEnumerable();

                var listaXmlNota = listaNotasXml.Join(listaNotasBanco,
                                                 xml => xml.Chave,
                                                 nota => nota.Field<string>("chavenfe"),
                                                 (xml, nota) => new { Xml = xml, Notas = nota })
                                           .Select(a => new
                                           {
                                               Numero = Convert.ToString(a.Notas.Field<int>("id_nr_nf")),
                                               Chave = a.Xml.Chave,
                                               DataEmissaoXml = Convert.ToDateTime(a.Xml.DataEmissao).Date,
                                               DataEmissaoNota = a.Notas.Field<DateTime>("data_emissao").Date,
                                               Flag = Convert.ToDateTime(a.Xml.DataEmissao).Date == a.Notas.Field<DateTime>("data_emissao").Date ? "Data Xml igual Nota" : "Data Xml Diferente Nota"
                                           }).ToList();

                foreach (var nota in listaXmlNota)
                {
                    csv.AppendLine($"{nota.Numero};{nota.Chave.Trim()};{nota.DataEmissaoXml};{nota.DataEmissaoNota};{nota.Flag}");
                }
            }

            foreach (FileInfo file in dir.EnumerateFiles())
            {
                file.Delete();
            }

            File.WriteAllText($"{arquivo}", csv.ToString());
            MessageBox.Show("Feito");
        }        
        private async void BtnSelSpedContribuicoesClick(object sender, RoutedEventArgs e)
        {
            var arquivo = Common.EscolheArquivo(Multiselect: false);
            if (arquivo != null)
            {
                BtnSelSpedContribucoes.IsEnabled = false;
                await lerSped.LeSpedContribuicoes(arquivo,this._SpedContribuicoes,this);
                BtnSelSped.IsEnabled = true;
                processarSpedContribuicoes();
            }
        }
        private async void BtnReportSpedContribuicoesClick(object sender, RoutedEventArgs e)
        {
            var arquivo = Common.EscolheArquivo(Multiselect: false);
            if (arquivo != null)
            {
                BtnSelSpedContribucoes.IsEnabled = false;
                await lerSped.LeSpedContribuicoes(arquivo, this._SpedContribuicoes, this);
                BtnSelSped.IsEnabled = true;
                reportSpedContribuicoes();
            }
        }
        private async void BtnSelSped_Click(object sender, RoutedEventArgs e)
        {
            var arquivo = Common.EscolheArquivo(Multiselect: false);
            if (arquivo != null)
            {
                BtnSelSped.IsEnabled = false;
                await LeSped(arquivo);
                BtnSelSped.IsEnabled = true;
                processarSpedFiscal();
            }
        }
        private async void BtnExecSped_Click(object sender, RoutedEventArgs e)
        {
            relatorio.relatorioCSV(this._Sped);
            ajustesFuncoes.ajusteC100(this._Sped);
            var arquivo = Common.SalvaArquivo("txt", "Sped");
            if (arquivo != null)
            {
                BtnExecSped.IsEnabled = false;
                BtnSelSped.IsEnabled = false;
                if ((bool)chck0150.IsChecked)
                {
                   await Task.Run(() => ajustesFuncoes.bloco0Redundante(this._Sped));
                }
                /*if ((bool)chckGeraInventario.IsChecked)
                {
                    _Sped_Inventario = new();
                }*/

                //Copia0200();
                //ArrumaC100();
                ajustesFuncoes.bloco0Redundante(this._Sped);
            
                await Task.Run(() =>
                {
                    _Sped.GerarLinhas();
                    _Sped.CalcularBloco9();
                    _Sped.Escrever(arquivo.Replace(".", " Alterado."), Encoding.GetEncoding(1252));
                });
            
                BlocoTxt.Text += "Feito";
                BtnExecSped.IsEnabled = true;
                BtnSelSped.IsEnabled = true;
            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var arquivo = Common.EscolheArquivo(Multiselect: false);
            if (arquivo != null)
            {
                btnInventario.IsEnabled = false;
                await LeSpedInventario(arquivo);
                btnInventario.IsEnabled = true;
            }
        }
        private async void ButtonCte_Click(object sender, RoutedEventArgs e)
        {
            if (pickerDataFin.SelectedDate is null || pickerDataIni.SelectedDate is null)
            {
                MessageBox.Show("Selecione uma Data");
                return;
            }

            if (((TimeSpan)(pickerDataFin.SelectedDate - pickerDataIni.SelectedDate)).TotalDays > 30)
            {
                MessageBox.Show("Selecione um intervalo igual ou menor que 30 dias");
            }

            var arquivo = Common.SalvaArquivo("csv", "Relatorio Ctes");
            if (arquivo != null)
            {
                panelAguarde.Visibility = Visibility.Visible;
                //progressBar.Visibility = Visibility.Visible;
                await RelatorioCteAsync(arquivo, (DateTime)pickerDataIni.SelectedDate, (DateTime)pickerDataFin.SelectedDate);
                panelAguarde.Visibility = Visibility.Hidden;
            }
        }
        private async void ButtonSaida_Click(object sender, RoutedEventArgs e)
        {
            if (pickerDataFin.SelectedDate is null || pickerDataIni.SelectedDate is null)
            {
                MessageBox.Show("Selecione uma Data");
                return;
            }

            if (((TimeSpan)(pickerDataFin.SelectedDate - pickerDataIni.SelectedDate)).TotalDays > 30)
            {
                MessageBox.Show("Selecione um intervalo igual ou menor que 30 dias");
            }

            var arquivo = Common.SalvaArquivo("csv", "Relatorio Notas Saida");
            if (arquivo != null)
            {
                panelAguarde.Visibility = Visibility.Visible;
                //progressBar.Visibility = Visibility.Visible;
                await RelatorioNotaSaidaAsync(arquivo, (DateTime)pickerDataIni.SelectedDate, (DateTime)pickerDataFin.SelectedDate);
                panelAguarde.Visibility = Visibility.Hidden;
            }
        }
        /*private async void ChckGeraInventario_Check(object sender, RoutedEventArgs e)
        {
            var arquivo = Common.EscolheArquivo();
            if (arquivo != null)
            {
                BtnExecSped.IsEnabled = false;
                chckGeraInventario.IsEnabled = false;
                try
                {
                    //await Task.Run(() => GeraInventario(arquivo));
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Arquivo Nao Encontrado");
                }
                BtnExecSped.IsEnabled = true;
            }
        }*/
        private void GeraInventario(string caminho)
        {
            var blocoH = new BlocoH();
            blocoH.RegH001 = new();
            try
            {
                var xls = new XLWorkbook(caminho);
                var planilha = xls.Worksheets.First(w => w.Name == "INVENTARIO");
                var totalLinhas = planilha.Rows().Count();
                // primeira linha é o cabecalho
                var valortot = planilha.Cell($"I{totalLinhas}").Value.ToString();
                blocoH.RegH001.RegH005s = new();
                blocoH.RegH001.RegH005s.Add(new BlocoH.RegistroH005
                {
                    VlInv = Convert.ToDecimal(valortot),
                    MotInv = 1,
                    DtInv = Convert.ToDateTime("30/06/2022"),
                    RegH010s = new()
                });
                for (int l = 2; l <= totalLinhas - 1; l++)
                {
                    BlocoH.RegistroH010 item = new BlocoH.RegistroH010
                    {
                        CodItem = planilha.Cell($"B{l}").Value.ToString(),
                        Unid = planilha.Cell($"E{l}").Value.ToString(),
                        Qtd = Convert.ToDecimal(planilha.Cell($"G{l}").Value.ToString()),
                        VlUnit = Convert.ToDecimal(planilha.Cell($"H{l}").Value.ToString()),
                        VlItem = Convert.ToDecimal(planilha.Cell($"I{l}").Value.ToString()),
                        IndProp = 0,
                        TxtCompl = planilha.Cell($"D{l}").Value.ToString(),
                        CodCta = "46321"
                    };
                    blocoH.RegH001.RegH005s.First().RegH010s.Add(item);
                }
                _Sped.BlocoH = blocoH;
            }
            catch
            {
                MessageBox.Show("Arquivo Invalido");
            }
        }
        private async Task LeSped(string caminho)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                await Task.Run(() => _Sped.Ler(caminho, Encoding.GetEncoding(1252)));
                var bloco0 = _Sped.Bloco0.Reg0000;
                lblSped.Content = @$"{bloco0.Nome} {bloco0.DtIni} {bloco0.DtIni}";
                PanelChk.IsEnabled = true;
                BtnExecSped.Visibility = Visibility.Visible;
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
        private async Task LeSpedInventario(string caminho)
        {
            try
            {
                await Task.Run(() => _Sped_Inventario.Ler(caminho));

                var bloco0 = _Sped_Inventario.Bloco0.Reg0000;
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
        private void PanelNotas_Click(object sender, RoutedEventArgs e)
        {
            GridNotas.Visibility = Visibility.Visible;
            GridSped.Visibility = Visibility.Hidden;
        }
        private void PanelSped_Click(object sender, RoutedEventArgs e)
        {
            GridSped.Visibility = Visibility.Visible;
            GridNotas.Visibility = Visibility.Hidden;
        }
        private void processarSpedContribuicoes()
        {
            //ajustesFuncoes.ajusteD100(this._SpedContribuicoes);
            //ajustesFuncoes.bloco0Redundante(this._SpedContribuicoes);
            ajustesFuncoes.ajuste0150(this._SpedContribuicoes);
            ajustesFuncoes.ajuste0205(this._SpedContribuicoes);
            ajustesFuncoes.ajusteIE(this._SpedContribuicoes);
            ajustesFuncoes.ajusteA170(this._SpedContribuicoes);
            ajustesFuncoes.ajusteC100(this._SpedContribuicoes);
            ajustesFuncoes.ajusteD101(this._SpedContribuicoes);
            ajustesFuncoes.ajusteD500(this._SpedContribuicoes);
            ajustesFuncoes.ajusteDCst(this._SpedContribuicoes);
            ajustesFuncoes.ajusteC170(this._SpedContribuicoes);
            ajustesFuncoes.excluirC100(this._SpedContribuicoes);
            ajustesFuncoes.corrigirAliq(this._SpedContribuicoes);
            gravarArquivo.ExecSped(this._SpedContribuicoes, this);
            //ajustesFuncoes.registrosFaltam(this._SpedContribuicoes);
            relatorio.relatorioCSV(this._SpedContribuicoes);
            //ajustesFuncoes.ajuste0500(this._SpedContribuicoes);//transferir0500.trasnferir0500(this._SpedContribuicoes);
        }
        private void reportSpedContribuicoes()
        {
            relatorio.relatorioCSV(this._SpedContribuicoes);
        }
        private void processarSpedFiscal()
        {
            //ajustesFuncoes.ajusteC190(this._Sped);
            //ajustesFuncoes.ajusteC100(this._Sped);
            //ajustesFuncoes.bloco0Redundante(this._Sped);
            //ajustesFuncoes.excluirnotas(this._Sped);
            relatorio.relatorioCSV(this._Sped);
            //gravarArquivo.ExecSped(this._Sped, this);
        }
        private void Copia0200()
        {
            var lista0200 = _Sped_Inventario.Bloco0.Reg0001.Reg0200s;

            var listaChaves = _Sped.Bloco0.Reg0001.Reg0200s.Select(a => a.CodItem).ToList();

            _Sped.Bloco0.Reg0001.Reg0200s.AddRange(lista0200.Where(a => !listaChaves.Contains(a.CodItem)).ToList());
        }
        private void ArrumaC100()
        {
            var countC = _Sped.BlocoC.RegC001.RegC100s.Where(a => a.DtEs == new DateTime(2022, 10, 14)).Count();
            var countD = _Sped.BlocoD.RegD001.RegD100s.Where(a => a.DtAP == new DateTime(2022, 10, 07)).Count();
            Dispatcher.BeginInvoke(() =>
                BlocoTxt.Text += ($"Encontrados {countC} BlocosC100 \n")
            );
            Dispatcher.BeginInvoke(() =>
                BlocoTxt.Text += ($"Encontrados {countD} BlocosD100 \n")
            );

            _Sped.BlocoC.RegC001.RegC100s.Where(a => a.DtEs == new DateTime(2022, 10, 14)).ToList().ForEach(b => { b.CodSit = 1; b.DtEs = b.DtDoc; });
            _Sped.BlocoD.RegD001.RegD100s.Where(a => a.DtAP == new DateTime(2022, 10, 07)).ToList().ForEach(b => { b.CodSit = 1; b.DtAP = b.DtDoc; });
        }

    }
}