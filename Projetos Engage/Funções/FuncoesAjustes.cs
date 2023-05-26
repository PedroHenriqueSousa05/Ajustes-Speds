using FiscalBr.EFDContribuicoes;
using System;
using System.Linq;
using System.IO;
using static FiscalBr.EFDContribuicoes.BlocoD;
using static FiscalBr.EFDContribuicoes.Bloco0;
using static FiscalBr.EFDContribuicoes.BlocoC;
using static FiscalBr.EFDContribuicoes.BlocoA;
using FiscalBr.EFDFiscal;
using System.Collections.Generic;
using FiscalBr.Common.Sped;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Data;
using System.Text;
using static FiscalBr.EFDFiscal.BlocoC;
using System.Xml;
using System.Globalization;
using System.Drawing;
using System.Xml.Serialization;
using ProjetoSpeds.Common;
using ExcelDataReader;
using System.Configuration;
using NFe.Classes;

namespace ProjetoSpeds.Funções
{
    internal class FuncoesAjustes
    {
        public NFe.Classes.nfeProc PopulaNFe(string xml)
        {
            var nf = CarregarDeArquivoXml(xml);

            return nf;
            /*_NFe = new Invoice_Body();
            XmlNodeList parentNode = nfe.GetElementsByTagName("transp");

            foreach (XmlNode childrenNode in parentNode)
            {
                _NFe.transportadora[0] = new Transportadora();
                _NFe.transportadora[0].incricaoestadual = childrenNode.SelectSingleNode("//IE").Value;
            }*/

        }

        public static NFe.Classes.nfeProc CarregarDeArquivoXml(string arquivoXml)
        {
            //var s = ObterNodeDeArquivoXml(typeof(nfeProc).Name, arquivoXml);
            return XmlStringParaClasse<NFe.Classes.nfeProc>(arquivoXml);
        }

        /*public static string ObterNodeDeArquivoXml(string nomeDoNode, string arquivoXml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(arquivoXml);
            //var xmlDoc = XDocument.Load(arquivoXml);
           // var xmlString = (from d in xmlDoc.Descendants()
           //                  where d.Name.LocalName == nomeDoNode
           //                  select d).FirstOrDefault();

            if (arquivoXml == null)
                throw new Exception(String.Format("Nenhum objeto {0} encontrado no arquivo {1}!", nomeDoNode, arquivoXml));
            return arquivoXml.ToString();
        }*/

        public static T XmlStringParaClasse<T>(string input) where T : class
        {
            var ser = new XmlSerializer(typeof(T));

            using (var sr = new StringReader(input))
                return (T)ser.Deserialize(sr);
        }

        private Listas_Ajuste dic_aux = new();
        public void ajusteD100(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            var d100 = _SpedContribuicoes.BlocoD.RegD001.RegD010s.SelectMany(x => x.RegD100s).Where(x => x.DtAP == Convert.ToDateTime("01/01/2050")).ToList();
            d100.ForEach(x => { x.DtAP = x.DtDoc; x.CodSit = 1; });
        }
        public void ajusteD101(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            foreach (RegistroD010 rd010 in _SpedContribuicoes.BlocoD.RegD001.RegD010s)
            {
                if (rd010.RegD100s is not null)
                {
                    foreach (RegistroD100 d101 in rd010.RegD100s)
                    {
                        if (d101.RegD101s is not null)
                        {
                            foreach (RegistroD101 regd101 in d101.RegD101s)
                            {
                                if (regd101.CstPis == 01)
                                {
                                    regd101.CstPis = 99;
                                }
                            }
                        }
                    }
                }
            }

            foreach (RegistroD010 rd101 in _SpedContribuicoes.BlocoD.RegD001.RegD010s)
            {
                if (rd101.RegD100s is not null)
                {
                    foreach (RegistroD100 d100 in rd101.RegD100s)
                    {
                        if (d100.RegD105s is not null)
                        {
                            foreach (RegistroD105 regd101 in d100.RegD105s)
                            {
                                if (regd101.CstCofins == 01)
                                {
                                    regd101.CstCofins = 99;
                                }
                            }
                        }
                    }
                }
            }
        }
        public void ajusteD500(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            var d500 = _SpedContribuicoes.BlocoD.RegD001.RegD010s.Where(x => x.RegD500s is not null)
                .SelectMany(x => x.RegD500s).Where(x => x.CodSit == Convert.ToDecimal(0)).ToList();
            d500.ForEach(x => x.CodSit = 0);
        }
        public void ajusteDCst(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            // lista de chaves de CTe que devem ser substituídas
            List<String> temp = new() { "42220601618261000132570000000517301960623005",
                                "42220601618261000132570000000520261503356725",
                                "42220601618261000132570000000520271299682205",
                                "42220601618261000132570000000521361458232882",
                                "42220601618261000132570000000521411162695365",
                                "42220601618261000132570000000521481736973720",
                                "42220601618261000132570000000521491533299207",
                                "42220601618261000132570000000521501052459763"};


            // percorre todos os registros D010 do arquivo EFD Contribuições
            foreach (RegistroD010 rd010 in _SpedContribuicoes.BlocoD.RegD001.RegD010s)
            {
                // verifica se o registro D010 tem registros D100
                if (rd010.RegD100s is not null)
                {
                    // percorre todos os registros D100 do registro D010 atual
                    foreach (RegistroD100 rd100 in rd010.RegD100s)
                    {
                        // verifica se a chave de CTe atual deve ser substituída
                        if (temp.Contains((rd100.ChvCTe)))
                        {
                            rd100.ChvCTe = "xxxxxxxxxxxxx"; // substitui a chave de CTe
                        }

                        // atualiza os valores dos campos de PIS para registros com CSTs 98, 99 e 50
                        var cstPis = rd100.RegD101s.ToList();
                        cstPis.Where(x => x.CstPis == 99 || x.CstPis == 98).ToList().ForEach(x =>
                        {
                            x.CstPis = 50;
                            x.NatBcCred = "07";
                            x.AliqPis = Convert.ToDecimal(1.65);
                        });

                        // atualiza os valores dos campos de COFINS para registros com CSTs 98, 99 e 50
                        var cstCofins = rd100.RegD105s.ToList();
                        cstCofins.Where(x => x.CstCofins == 99 || x.CstCofins == 98).ToList().ForEach(x =>
                        {
                            x.CstCofins = 50;
                            x.NatBcCred = "07";
                            x.AliqCofins = Convert.ToDecimal(7.60);
                        });
                    }
                }
            }
        }
        public void ajuste0150(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            List<string> listaCod = new  List<string>();
            foreach (Registro0140 r0140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                if (r0140.Reg0150s is not null)
                {
                    foreach (var r0150 in r0140.Reg0150s)
                    {
                        listaCod.Add(r0150.CodPart);
                        if (r0150.CodPais == "0")
                        {
                            r0150.CodPais = "1058";
                        }
                        if (r0150.CodMun is null)
                        {
                            r0150.CodMun = "3550308";
                        }
                        
                    }
                }
            }
        }
        public void ajuste0205(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            foreach (var r0140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                if (r0140.Reg0200s is not null)
                {
                    foreach (var r0200 in r0140.Reg0200s)
                    {
                        if (r0200.Reg0205s != null && r0200.Reg0205s.Count > 1)
                        {
                            for (int i = 0; i < r0200.Reg0205s.Count; i++)
                            {
                                for (int j = i + 1; j < r0200.Reg0205s.Count; j++)
                                {
                                    var r205A = r0200.Reg0205s[i];
                                    var r205B = r0200.Reg0205s[j];

                                    if (r205A.DtIni <= r205B.DtFin && r205B.DtIni <= r205A.DtFin)
                                    {
                                        if (r205A.DtFin.Day == DateTime.DaysInMonth(r205A.DtFin.Year, r205A.DtFin.Month))
                                        {
                                            r205B.DtIni = r205A.DtFin.AddDays(1);
                                        }
                                        else if (r205B.DtFin.Day == DateTime.DaysInMonth(r205B.DtFin.Year, r205B.DtFin.Month))
                                        {
                                            r205A.DtFin = r205B.DtIni.AddDays(-1);
                                        }
                                        else
                                        {
                                            var midDate = r205A.DtFin.AddDays(1);
                                            r205B.DtIni = midDate;
                                            r205B.DtFin = r205B.DtFin.AddDays(midDate.Month == r205B.DtFin.Month ? 1 : -r205B.DtFin.Day + 1);
                                        }
                                    }
                                }
                            }
                        }
                        if (r0200.Reg0205s is not null)
                        {
                            foreach (var r205 in r0200.Reg0205s)
                            {
                                if (r205.CodAntItem is not null && r205.DescrAntItem is not null)
                                {
                                    if (r0200.Reg0205s.Count == 1 || r205.Equals(r0200.Reg0205s.First()))
                                    {
                                        r205.CodAntItem = null;
                                    }
                                    else
                                    {
                                        r205.DescrAntItem = null;
                                    }
                                }
                                else if (r205.CodAntItem is not null)
                                {
                                    if (r0200.Reg0205s.Count == 1 || r205.Equals(r0200.Reg0205s.First()))
                                    {
                                        r205.CodAntItem = null;
                                    }
                                    else
                                    {
                                        r205.DescrAntItem = null;
                                    }
                                }
                                else if (r205.DescrAntItem is not null)
                                {
                                    if (r0200.Reg0205s.Count == 1 || r205.Equals(r0200.Reg0205s.First()))
                                    {
                                        r205.DescrAntItem = null;
                                    }
                                    else
                                    {
                                        r205.CodAntItem = null;
                                    }
                                }

                                if (r205.DtIni < new DateTime(2003, 01, 01))
                                {
                                    r205.DtIni = new DateTime(2022, 09, 01);
                                }
                            }
                        }
                    }
                }
            }
        }
        public void ajuste0200(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            List<string> listaCodItem = new List<string>();
            
            foreach(RegistroC010 c010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var c100 in c010.RegC100s)
                {
                    if (c100.RegC170s is not null)
                    {
                        foreach (var c170 in c100.RegC170s)
                        {
                            listaCodItem.Add(c170.CodItem);
                        }
                    }
                }
            }
            foreach(Registro0140 rc0140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                if (rc0140.Reg0200s is not null)
                {
                    foreach (var rc0200 in rc0140.Reg0200s)
                    {
                        if (listaCodItem.Contains(rc0200.CodBarra))
                        {
                            foreach (RegistroC010 c010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
                            {
                                foreach (var c100 in c010.RegC100s)
                                {
                                    if (c100.RegC170s is not null)
                                    {
                                        foreach (var c170 in c100.RegC170s)
                                        {
                                            if (c170.CodItem == rc0200.CodBarra)
                                            {
                                                c170.CodItem = rc0200.CodItem;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            List<string> lista0150 = new List<string>();
            foreach(Registro0140 rc140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                if (rc140.Reg0150s is not null)
                {
                    foreach (var rc0150 in rc140.Reg0150s)
                    {
                        lista0150.Add(rc0150.CodPart);
                    }
                }
            }
            foreach(RegistroC010 rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach(var c100 in rc010.RegC100s)
                {
                    if (!lista0150.Contains(c100.CodPart))
                    {
                        string linha0150 = $"{c100.CodPart}\n";
                        File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\falta0150.txt", linha0150, Encoding.UTF8);
                    }
                }
            }
        }
        public void ajusteC100(ArquivoEFDContribuicoes _SpedContribuicoes)
        {

            //Define um Código do participante padrão para os registros C100 com CodPart nulo
            if (_SpedContribuicoes == null || _SpedContribuicoes.BlocoC == null)
                return;

            foreach (RegistroC010 reg010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                if (reg010.RegC100s == null)
                    continue;

                foreach (var regC100 in reg010.RegC100s)
                {
                    if (regC100.CodPart is null)
                    {
                        regC100.CodPart = "999999";
                    }
                }
            }

            /*var c100 = _SpedContribuicoes.BlocoC.RegC001.RegC010s.SelectMany(x => x.RegC100s).Where(x => x.DtEs == Convert.ToDateTime("02/02/2002")).ToList();
            c100.ForEach(x => { x.DtEs = x.DtDoc; x.CodSit = 1; });*/
        }
        public void ajusteCstC170(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            //ajustes COD SIT para notas sem valor de mercadorias
            var c170a = _SpedContribuicoes.BlocoC.RegC001.RegC010s.Where(x => x.RegC100s is not null).
                SelectMany(x => x.RegC100s).ToList();
            c170a.ForEach(x => x.CodSit = 6);

            //ajustes CST notas complementares
            var c170b = _SpedContribuicoes.BlocoC.RegC001.RegC010s.Where(x => x.RegC100s is not null).
                SelectMany(x => x.RegC100s).ToList().Where(x => x.RegC170s is not null).
                SelectMany(x => x.RegC170s).ToList().Where(x => ((x.Cfop == 5102 || x.Cfop == 5106 || x.Cfop == 6102 || x.Cfop == 6106 || x.Cfop == 6108) && (x.CstPis != 1 || x.CstCofins != 1))).ToList();
            c170b.ForEach(x => { x.CstPis = 1; x.CstCofins = 1; });
        }
        public void ajusteC190(ArquivoEFDFiscal _SpedFiscal)
        {
            // Verifica se o arquivo do SPED Fiscal e o bloco C foram preenchidos corretamente
            if (_SpedFiscal == null || _SpedFiscal.BlocoC == null)
                return;

            // Percorre todos os registros C100 do bloco C
            foreach (var c100 in _SpedFiscal.BlocoC.RegC001.RegC100s)
            {
                // Verifica se o registro C100 possui registros C190
                if (c100.RegC190s != null)
                {
                    // Agrupa os registros C190 que possuem o mesmo CST_ICMS, CFOP e ALIQ_ICMS e possuem mais de um registro
                    var registrosC190 = c100.RegC190s
                        .GroupBy(r => new { r.CstIcms, r.Cfop, r.AliqIcms })
                        .Where(g => g.Count() > 1);

                    // Percorre todos os grupos de registros C190 duplicados
                    foreach (var grupoC190 in registrosC190)
                    {
                        // Soma os valores dos registros C190 iguais
                        var valorTotalCst = grupoC190.Sum(r => r.VlOpr);
                        var valorTotalIcms = grupoC190.Sum(r => r.VlIcms);
                        var valorTotalBCIcms = grupoC190.Sum(r => r.VlBcIcms);

                        // Pega o primeiro registro C190 do grupo e atualiza os valores de acordo com a soma
                        var primeiroC190 = grupoC190.First();

                        primeiroC190.VlOpr += valorTotalCst - primeiroC190.VlOpr;
                        primeiroC190.VlIcms += valorTotalIcms - primeiroC190.VlIcms;
                        primeiroC190.VlBcIcms += valorTotalBCIcms - primeiroC190.VlBcIcms;

                        // Exclui os registros C190 duplicados, deixando apenas o primeiro
                        foreach (var registroC190 in grupoC190.Skip(1))
                        {
                            c100.RegC190s.Remove(registroC190);
                        }
                    }
                }
            }
        }
        public void ajusteA170(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            if (_SpedContribuicoes.BlocoA.RegA001.RegA010s is not null)
            {

                var a100 = _SpedContribuicoes.BlocoA.RegA001.RegA010s.SelectMany(x => x.RegA100s).ToList();
                var a170 = _SpedContribuicoes.BlocoA.RegA001.RegA010s.SelectMany(x => x.RegA100s).SelectMany(x => x.RegA170s).ToList();

                a100.ForEach(x =>
                {
                    x.VlBcCofins = x.VlDoc;
                    x.VlCofins = Math.Round((x.VlDoc * Convert.ToDecimal(0.076)), 2);
                    x.VlBcPis = x.VlDoc;
                    x.VlPis = Math.Round((x.VlDoc * Convert.ToDecimal(1.65)), 2);
                });

                a170.ForEach(x =>
                {
                    x.CstPis = 50;
                    x.AliqPis = Convert.ToDecimal(1.65);
                    x.VlBcPis = x.VlItem;
                    x.VlPis = Math.Round((x.VlItem * Convert.ToDecimal(0.0165)), 2);
                    x.CstCofins = 50;
                    x.AliqCofins = Convert.ToDecimal(7.6);
                    x.VlBcCofins = x.VlItem;
                    x.VlCofins = Math.Round((x.VlItem * Convert.ToDecimal(0.076)), 2);
                    x.NatBcCred = "03";
                });
            };
        }
        public void bloco0Redundante(ArquivoEFDContribuicoes _SpedContribuicoes)
        {

            _SpedContribuicoes.Bloco0.Reg0001.Reg0140s = _SpedContribuicoes.Bloco0.Reg0001.Reg0140s.Where(a => a.Reg0150s is not null).ToList();

            foreach (Registro0140 r0140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                r0140.Reg0150s = r0140.Reg0150s.GroupBy(a => a.CodPart).Select(b => b.First()).ToList();
                r0140.Reg0200s = r0140.Reg0200s.GroupBy(a => a.CodItem).Select(b => b.First()).ToList();
                r0140.Reg0400s = r0140.Reg0400s.GroupBy(a => a.CodNat).Select(b => b.First()).ToList();

                foreach (Registro0200 r0200 in r0140.Reg0200s)
                {
                    if (r0200.Reg0205s is not null)
                    {
                        r0200.Reg0205s = r0200.Reg0205s.GroupBy(a => a.CodAntItem).Select(b => b.First()).ToList();
                    }
                }
            }
        }
        public void bloco0Redundante(ArquivoEFDFiscal _SpedFiscal)
        {
            if (_SpedFiscal.BlocoH != null)
            {
                var blocoH001 = _SpedFiscal.BlocoH.RegH001;
                if (blocoH001 != null)
                {
                    var blocoH005s = blocoH001.RegH005s;
                    if (blocoH005s != null)
                    {
                        foreach (var blocoH005 in blocoH005s)
                        {
                            // Acessa os registros do bloco H e do registro H010
                            blocoH005.RegH010s = blocoH005.RegH010s.GroupBy(a => new { a.CodItem }).Select(b => b.First()).ToList();
                            // ...outras operações no bloco H e/ou no registro H010
                        }
                    }
                }
            }

            _SpedFiscal.Bloco0.Reg0001.Reg0200s = _SpedFiscal.Bloco0.Reg0001.Reg0200s.GroupBy(a => a.CodItem).Select(b => b.First()).ToList();
            _SpedFiscal.Bloco0.Reg0001.Reg0150s = _SpedFiscal.Bloco0.Reg0001.Reg0150s.GroupBy(a => a.CodPart).Select(b => b.First()).ToList();
            _SpedFiscal.Bloco0.Reg0001.Reg0400s = _SpedFiscal.Bloco0.Reg0001.Reg0400s.GroupBy(a => a.CodNat).Select(b => b.First()).ToList();
        }
        public void ajusteC100(ArquivoEFDFiscal _SpedFiscal)
        {
            //Remove o valor do fcp do valor final do ICMS
            decimal aux = 0;

            //percorre todos os registros C100 do sped
            foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 rc100 in _SpedFiscal.BlocoC.RegC001.RegC100s)
            {
                if (rc100.RegC101 is not null)
                {
                    var fcp = rc100.RegC101.VlFcpUfDest;

                    //percorre todos os registros C190 do sped
                    foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC190 rc190 in rc100.RegC190s)
                    {
                        rc190.VlIcms -= fcp;
                        aux = aux + rc190.VlIcms;
                        rc100.VlIcms = rc190.VlIcms;
                    }
                }

                aux = 0;
            }

        }
        public void registrosFaltam(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            string arquivoExcel = "C:\\Users\\Micro\\Desktop\\Speds\\Pasta1.xlsx";

            // Define a codificação a ser usada
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");

            // Lê o arquivo do Excel e carrega os valores de ID em uma lista
            var listaId = new List<string>();
            using (var stream = File.Open(arquivoExcel, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = encoding }))
                {

                    DataSet dataSet = reader.AsDataSet();
                    System.Data.DataTable dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string id = row.Field<string>("Column0");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            listaId.Add(id);
                        }
                    }
                }
            }

            List<string> listaCodPar = new List<string>();



            foreach (FiscalBr.EFDContribuicoes.BlocoC.RegistroC010 rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var c100 in rc010.RegC100s)
                {
                    if (c100 is not null && c100.DtDoc is not null && c100.DtEs is not null)
                    {
                        if (listaId.Contains(c100.ChvNfe))
                        {
                            listaCodPar.Add(c100.CodPart);
                            string linha = $"|C100|{c100.IndOper}|{c100.IndEmit}|{c100.CodPart}|{c100.CodMod}|0{c100.CodSit}|{c100.Ser}|{c100.NumDoc}|{c100.ChvNfe}|{c100.DtDoc.Value.ToString("ddMMyyyy")}|{c100.DtEs.Value.ToString("ddMMyyyy")}|{c100.VlDoc:0.00}|{c100.IndPgto}|{c100.VlDesc:0.00}|{c100.VlAbatNt:0}|{c100.VlMerc}|{c100.IndFrt}|{c100.VlFrt}|{c100.VlSeg:0}|{c100.VlOutDa:0}|{c100.VlBcIcms:0.00}|{c100.VlIcms}|{c100.VlBcIcmsSt:0}|{c100.VlIcmsSt:0}|{c100.VlIpi:0}|{c100.VlPis:0.00}|{c100.VlCofins:0.00}|{c100.VlPisSt:0}|{c100.VlCofinsSt:0}|\n";
                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linha, Encoding.UTF8);

                            if (c100.RegC170s is not null)
                            {
                                foreach (var c170 in c100.RegC170s)
                                {
                                    if (c170.Cfop.ToString().StartsWith("5") || c170.Cfop.ToString().StartsWith("6"))
                                    {
                                        if (c170.IndMov.ToString() == "Sim")
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|0|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|0{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisQuant}|{c170.VlPis}|0{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsQuant}|{c170.VlCofins}|{c170.CodCta}|\n";
                                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                        else
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|1|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|0{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisQuant}|{c170.VlPis}|0{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsQuant}|{c170.VlCofins}|{c170.CodCta}|\n"; File.AppendAllText(@"C:\Users\Micro\Documents\origm\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                    }
                                    else
                                    {
                                        if (c170.IndMov.ToString() == "Sim")
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|0|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisQuant}|{c170.VlPis}|{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsQuant}|{c170.VlCofins}|{c170.CodCta}|\n";
                                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                        else
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|1|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisQuant}|{c170.VlPis}|{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsQuant}|{c170.VlCofins}|{c170.CodCta}|\n"; File.AppendAllText(@"C:\Users\Micro\Documents\origm\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (FiscalBr.EFDContribuicoes.Bloco0.Registro0140 r140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                string linha0140 = $"|0140|{r140.CodEst}|{r140.Nome}|{r140.Cnpj}|{r140.Uf}|{r140.Ie}|{r140.CodMun}|{r140.Im}|{r140.Suframa}|\n";
                File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\0150.txt", linha0140, Encoding.UTF8);

                foreach (var r0150 in r140.Reg0150s)
                {

                    if (listaCodPar.Contains(r0150.CodPart))
                    {
                        string linha0150 = $"|0150|{r0150.CodPart}|{r0150.Nome}|{r0150.CodPais}|{r0150.Cnpj}|{r0150.Cpf}|{r0150.Ie}|{r0150.CodMun}|{r0150.Suframa}|{r0150.End}|{r0150.Num}|{r0150.Compl}|{r0150.Bairro}|\n";
                        File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\0150.txt", linha0150, Encoding.UTF8);
                    }

                }
            }
        }
        public void corrgirVlPisCofins(ArquivoEFDContribuicoes _SpedContribuicoes)
        {

            var valorIPI = dic_aux.valor_IPI_Compras_chave_item();
            var baseIPI = dic_aux.base_IPI_Compras_chave_item();
            var percentualIPI = dic_aux.percentual_IPI_Compras_chave_item();
            var listaFrete = dic_aux.ajusteFrete();

            foreach (var rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var rc100 in rc010.RegC100s)
                {

                    if (rc100.RegC170s is not null)
                    {
                        foreach (var rc170 in rc100.RegC170s)
                        {
                            if (rc170.CstCofins == 01 && rc170.Cfop == 6108 && rc170.AliqCofins == 0 && rc170.AliqPis == 0)
                            {
                                double aliqPis = 1.65;
                                rc170.AliqPis = (decimal)aliqPis;

                                double aliqCofins = 7.60;
                                rc170.AliqCofins = (decimal)aliqCofins;
                            }

                            if (rc170.CstCofins == 01 && rc170.Cfop == 5102 && rc170.AliqCofins == 0 && rc170.AliqPis == 0)
                            {
                                double aliqPis = 1.65;
                                rc170.AliqPis = (decimal)aliqPis;

                                double aliqCofins = 7.60;
                                rc170.AliqCofins = (decimal)aliqCofins;
                            }

                            if (string.IsNullOrEmpty(rc170.CodCta))
                            {

                                rc170.CodCta = "3-1-01-01-00001";
                            }
                        }
                    }
                }
            }

            foreach (var rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var rc100 in rc010.RegC100s)
                {

                    Decimal rc100_vlmercadoria = 0;
                    Decimal rc100_vlpis = 0;
                    Decimal rc100_vlcofins = 0;

                    if (rc100.DtDoc > rc100.DtEs)
                    {
                        rc100.DtEs = rc100.DtDoc;
                    }

                    if (rc100.RegC170s is not null && rc100.IndOper == 0)
                    {
                        int contadorC170 = 0;
                        foreach (var rc170 in rc100.RegC170s)
                        {
                            contadorC170++;
                            if (rc170.CstCofins != 05 && rc170.CstCofins != 75)
                            {
                                decimal totalPis = 0;
                                decimal totalCofins = 0;
                                foreach (var c170 in rc100.RegC170s)
                                {
                                    totalPis += c170.VlPis;
                                    totalCofins += c170.VlCofins;
                                }

                                rc100.VlPis = totalPis;
                                rc100.VlCofins = totalCofins;
                            }
                            /* cst menor que 50 é para notas de saida - cst 99 são outras entradas*/
                            if (rc170.CstPis < 50 | rc170.CstCofins < 50)
                            {
                                rc170.CstPis = 99;
                                rc170.CstCofins = 99;
                            }

                            if (rc170.Cfop == 1102 || rc170.Cfop == 2102)
                            {
                                string aux = String.Concat(rc100.ChvNfe, rc170.CodItem);

                                if (valorIPI.TryGetValue(aux, out double value))
                                {
                                    rc170.VlIpi = Convert.ToDecimal(value);
                                }

                                if (baseIPI.TryGetValue(aux, out double valuebase))
                                {
                                    rc170.VlBcIpi = Convert.ToDecimal(valuebase);
                                }

                                if (percentualIPI.TryGetValue(aux, out double valuepercentual))
                                {
                                    rc170.AliqIpi = Convert.ToDecimal(valuepercentual);
                                }
                                if (rc170.VlBcIcms != 0)
                                {
                                    if (rc170.VlItem == 0)
                                    {
                                        rc170.VlItem = rc170.VlBcIcms;
                                    }
                                }
                                if (rc170.Cfop == 1102)
                                {
                                    rc170.VlBcCofins = rc170.VlItem;
                                    rc170.VlBcPis = rc170.VlItem;
                                }
                                else if (rc170.Cfop == 2102)
                                {
                                    if (contadorC170 == 1)
                                    {
                                        rc170.VlBcCofins = rc170.VlItem - rc170.VlIcms + Convert.ToDecimal(rc100.VlFrt);
                                        rc170.VlBcPis = rc170.VlItem - rc170.VlIcms + Convert.ToDecimal(rc100.VlFrt);
                                    }
                                    else
                                    {
                                        rc170.VlBcCofins = rc170.VlItem - rc170.VlIcms;
                                        rc170.VlBcPis = rc170.VlItem - rc170.VlIcms;
                                    }
                                }
                                
                            }
                            else if (rc170.Cfop == 2202 | rc170.Cfop == 1202)
                            {
                                if (rc170.VlBcIcms != 0)
                                {
                                    if (rc170.VlItem == 0)
                                    {
                                        rc170.VlItem = rc170.VlBcIcms;
                                    }
                                }

                                rc170.VlBcPis = rc170.VlItem - rc170.VlIcms;
                                rc170.VlBcCofins = rc170.VlItem - rc170.VlIcms;
                            }
                            else if (rc170.VlItem != 0 && rc170.VlBcIcms != 0)
                            {
                                rc170.VlBcPis = rc170.VlBcIcms - rc170.VlIcms;
                                rc170.VlBcCofins = rc170.VlBcIcms - rc170.VlIcms;
                                rc170.AliqPis = 1.65M;
                                rc170.AliqCofins = 7.60M;
                            }
                            rc170.VlPis = rc170.VlBcPis * Convert.ToDecimal(0.0165);
                            rc170.VlCofins = rc170.VlBcCofins * Convert.ToDecimal(0.0760);

                            if (listaFrete.IndexOf(Convert.ToString(rc100.ChvNfe)) > -1)
                            {
                                rc170.VlItem = Math.Round(Convert.ToDecimal(rc170.VlItem) + Convert.ToDecimal(rc100.VlFrt), 2);
                                rc170.VlBcPis = rc170.VlItem - rc170.VlIcms;
                                rc170.VlBcCofins = rc170.VlItem - rc170.VlIcms;
                            }
                           

                            rc100_vlmercadoria = rc100_vlmercadoria + rc170.VlItem;
                        }
                        rc100.VlMerc = rc100_vlmercadoria;
                        rc100.VlPis = rc100_vlpis;
                        rc100.VlCofins = rc100_vlcofins;
                        rc100.VlMerc = rc100_vlmercadoria;
                    }
                }
            }
        }
        public void excluirC100(ArquivoEFDContribuicoes _SpedContribuicoes)
        {

            var dicionarioC100 = new Dictionary<string, int>();

            foreach (RegistroC010 c010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {

                foreach (var c100 in c010.RegC100s)
                {
                    var chave = $"{c100.IndOper}{c100.IndEmit}{c100.CodPart}{c100.CodMod}{c100.Ser}{c100.NumDoc}{c100.ChvNfe}";

                    if (dicionarioC100.ContainsKey(chave))
                    {
                        dicionarioC100[chave]++;
                    }
                    else
                    {
                        dicionarioC100[chave] = 1;
                    }
                }
            }

            foreach (RegistroC010 c010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var c100 in c010.RegC100s.ToList())
                {
                    var chave = $"{c100.IndOper}{c100.IndEmit}{c100.CodPart}{c100.CodMod}{c100.Ser}{c100.NumDoc}{c100.ChvNfe}";

                    if (dicionarioC100[chave] > 1)
                    {
                        for (int i = c100.RegC170s.Count - 1; i >= 0; i--)
                        {
                            c100.RegC170s.RemoveAt(i);
                        }
                        c010.RegC100s.Remove(c100);

                        dicionarioC100[chave]--;
                    }
                }
            }
        }
        public void excluirListado(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            string arquivoExcel = "C:\\Users\\Micro\\Documents\\Conciliação\\Pasta1.xlsx";

            // Define a codificação a ser usada
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");

            // Lê o arquivo do Excel e carrega os valores de ID em uma lista
            var listaId = new List<string>();
            using (var stream = File.Open(arquivoExcel, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = encoding }))
                {

                    DataSet dataSet = reader.AsDataSet();
                    System.Data.DataTable dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string id = row.Field<string>("Column0");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            listaId.Add(id);
                        }
                    }
                }
            }

            foreach (RegistroC010 c010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var c100 in c010.RegC100s.ToList())
                {
                    if (listaId.Contains(c100.ChvNfe))
                    {
                        for (int i = c100.RegC170s.Count - 1; i >= 0; i--)
                        {
                            c100.RegC170s.RemoveAt(i);
                        }
                        c010.RegC100s.Remove(c100);


                    }
                }
            }
           
            foreach (RegistroD010 d010 in _SpedContribuicoes.BlocoD.RegD001.RegD010s)
            {
                foreach (var d100 in d010.RegD100s.ToList())
                {
                    if (listaId.Contains(d100.ChvCTe))
                    {
                        for (int i = d100.RegD101s.Count - 1; i >= 0; i--)
                        {
                            d100.RegD101s.RemoveAt(i);
                        }
                        for (int i = d100.RegD105s.Count - 1; i >= 0; i--)
                        {
                            d100.RegD105s.RemoveAt(i);
                        }
                        d010.RegD100s.Remove(d100);


                    }
                }
            }
        }
        public void excluirCFOPsListados(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            string arquivoExcel = "C:\\Users\\Micro\\Documents\\Conciliação\\Pasta2.xlsx";

            // Define a codificação a ser usada
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");

            // Lê o arquivo do Excel e carrega os valores de ID em uma lista
            var listaId = new List<string>();
            using (var stream = File.Open(arquivoExcel, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = encoding }))
                {

                    DataSet dataSet = reader.AsDataSet();
                    System.Data.DataTable dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string id = row.Field<string>("Column0");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            listaId.Add(id);
                        }
                    }
                }
            }
            foreach (RegistroC010 c010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                var c100sToRemove = new List<FiscalBr.EFDContribuicoes.BlocoC.RegistroC100>();
                foreach (var c100 in c010.RegC100s)
                {
                    if (c100.RegC170s is not null)
                    {
                        foreach (var c170 in c100.RegC170s)
                        {
                            if (listaId.Contains(c170.Cfop.ToString()))
                            {
                                var chave = $"{c100.ChvNfe}";

                                c100sToRemove.Add(c100);
                            }
                        }
                    }
                }

                foreach (var c100ToRemove in c100sToRemove)
                {
                    c010.RegC100s.Remove(c100ToRemove);
                }
            }
        }
        public void excluirListadoFiscal(ArquivoEFDFiscal _SpedFiscal)
        {
            string arquivoExcel = "C:\\Users\\Micro\\Documents\\Conciliação\\Pasta1.xlsx";

            // Define a codificação a ser usada
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");

            // Lê o arquivo do Excel e carrega os valores de ID em uma lista
            var listaId = new List<string>();
            using (var stream = File.Open(arquivoExcel, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = encoding }))
                {

                    DataSet dataSet = reader.AsDataSet();
                    System.Data.DataTable dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string id = row.Field<string>("Column0");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            listaId.Add(id);
                        }
                    }
                }
            }

            var listaC101 = new List<FiscalBr.EFDFiscal.BlocoC.RegistroC101>();
            var listaC191 = new List<FiscalBr.EFDFiscal.BlocoC.RegistroC191>();
            List<FiscalBr.EFDFiscal.BlocoC.RegistroC100> listaRemoverC100 = new List<FiscalBr.EFDFiscal.BlocoC.RegistroC100>();
            foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 c100 in _SpedFiscal.BlocoC.RegC001.RegC100s)
            {
                if (listaId.Contains(c100.ChvNfe))
                {
                    if (c100.RegC170s is not null)
                    {
                        if (c100.RegC170s.Count > 0)
                        {
                            for (int i = c100.RegC170s.Count - 1; i >= 0; i--)
                            {
                                c100.RegC170s.RemoveAt(i);
                            }
                        }
                    }

                    // Armazena os registros C101 e C191 do C100 atual para posterior remoção
                    if (c100.RegC101 != null)
                    {
                        listaC101.Add(c100.RegC101);
                        c100.RegC101 = null;
                    }

                    if (c100.RegC190s != null)
                    {
                        foreach (var regC190 in c100.RegC190s)
                        {
                            if (regC190.RegC191 != null)
                            {
                                listaC191.Add(regC190.RegC191);
                                regC190.RegC191 = null;
                            }
                        }
                        c100.RegC190s = null;
                    }
                    listaRemoverC100.Add(c100);

                }
            }
            foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 c100 in listaRemoverC100)
            {
                _SpedFiscal.BlocoC.RegC001.RegC100s.Remove(c100);
            }

            List<FiscalBr.EFDFiscal.BlocoD.RegistroD100> listaRemover = new List<FiscalBr.EFDFiscal.BlocoD.RegistroD100>();

            foreach (FiscalBr.EFDFiscal.BlocoD.RegistroD100 d100 in _SpedFiscal.BlocoD.RegD001.RegD100s)
            {
                if (listaId.Contains(d100.ChvCte))
                {
                    if (d100.RegD190s != null)
                    {
                        d100.RegD190s = null;
                    }
                    listaRemover.Add(d100);
                }
            }

            foreach (FiscalBr.EFDFiscal.BlocoD.RegistroD100 d100 in listaRemover)
            {
                _SpedFiscal.BlocoD.RegD001.RegD100s.Remove(d100);
            }
        }
        public void ajusteIE(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            foreach (Registro0140 r140 in _SpedContribuicoes.Bloco0.Reg0001.Reg0140s)
            {
                if (r140.Reg0150s is not null)
                {
                    foreach (var r150 in r140.Reg0150s)
                    {
                        if (r150.Ie is not null && r150.CodMun is not null)
                        {
                            if (r150.CodMun.StartsWith("31") && r150.Ie.Length == 11)
                            {
                                string aux = "00";
                                r150.Ie = r150.Ie.Insert(0, aux);
                            }

                            else if (r150.CodMun.StartsWith("31") && r150.Ie.Length == 12)
                            {
                                string aux = "0";
                                r150.Ie = r150.Ie.Insert(0, aux);
                            }

                        }
                    }
                }
            }
        }
        public void registrofaltamcontri(ArquivoEFDFiscal _SpedFiscal)
        {
            string arquivoExcel = "C:\\Users\\Micro\\Desktop\\Speds\\Pasta1.xlsx";

            // Define a codificação a ser usada
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");

            // Lê o arquivo do Excel e carrega os valores de ID em uma lista
            var listaId = new List<string>();
            using (var stream = File.Open(arquivoExcel, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = encoding }))
                {

                    DataSet dataSet = reader.AsDataSet();
                    System.Data.DataTable dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string id = row.Field<string>("Column0");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            listaId.Add(id);
                        }
                    }
                }
            }

            List<string> listaCodPar = new List<string>();

            if (_SpedFiscal.BlocoC is not null)
            {
                foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 c100 in _SpedFiscal.BlocoC.RegC001.RegC100s)
                {
                    if (c100 is not null && c100.DtDoc is not null && c100.DtEs is not null)
                    {
                        if (listaId.Contains(c100.ChvNfe))
                        {
                            listaCodPar.Add(c100.CodPart);
                            string linha = $"|C100|{c100.IndOper}|{c100.IndEmit}|{c100.CodPart}|{c100.CodMod}|0{c100.CodSit}|{c100.Ser}|{c100.NumDoc}|{c100.ChvNfe}|{c100.DtDoc.Value.ToString("ddMMyyyy")}|{c100.DtEs.Value.ToString("ddMMyyyy")}|{c100.VlDoc:0.00}|{c100.IndPgto}|{c100.VlDesc:0.00}|{c100.VlAbatNt:0}|{c100.VlMerc}|{c100.IndFrt}|{c100.VlFrt}|{c100.VlSeg:0}|{c100.VlOutDa:0}|{c100.VlBcIcms:0.00}|{c100.VlIcms}|{c100.VlBcIcmsSt:0}|{c100.VlIcmsSt:0}|{c100.VlIpi:0}|{c100.VlPis:0.00}|{c100.VlCofins:0.00}|{c100.VlPisSt:0}|{c100.VlCofinsSt:0}|\n";
                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linha, Encoding.UTF8);

                            if (c100.RegC170s is not null)
                            {
                                foreach (var c170 in c100.RegC170s)
                                {
                                    if (c170.Cfop.ToString().StartsWith("5") || c170.Cfop.ToString().StartsWith("6"))
                                    {
                                        if (c170.IndMov.ToString() == "Sim")
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|0|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|0{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisReais}|{c170.VlPis}|0{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsReais}|{c170.VlCofins}|{c170.CodCta}|\n";
                                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                        else
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|1|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|0{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisReais}|{c170.VlPis}|0{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsReais}|{c170.VlCofins}|{c170.CodCta}|\n"; File.AppendAllText(@"C:\Users\Micro\Documents\origm\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                    }
                                    else
                                    {
                                        if (c170.IndMov.ToString() == "Sim")
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|0|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisReais}|{c170.VlPis}|{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsReais}|{c170.VlCofins}|{c170.CodCta}|\n";
                                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                        else
                                        {
                                            string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|1|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisReais}|{c170.VlPis}|{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsReais}|{c170.VlCofins}|{c170.CodCta}|\n"; File.AppendAllText(@"C:\Users\Micro\Documents\origm\saida.txt", linhaC170, Encoding.UTF8);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (FiscalBr.EFDFiscal.Bloco0.Registro0150 r0150 in _SpedFiscal.Bloco0.Reg0001.Reg0150s)
            {

                if (listaCodPar.Contains(r0150.CodPart))
                {
                    string linha0150 = $"|0150|{r0150.CodPart}|{r0150.Nome}|{r0150.CodPais}|{r0150.Cnpj}|{r0150.Cpf}|{r0150.Ie}|{r0150.CodMun}|{r0150.Suframa}|{r0150.End}|{r0150.Num}|{r0150.Compl}|{r0150.Bairro}|\n";
                    File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\0150.txt", linha0150, Encoding.UTF8);
                }

            }
        }
        public void registrofaltamFiscal(ArquivoEFDFiscal _SpedFiscal)
        {
            string arquivoExcel = "C:\\Users\\Micro\\Desktop\\Speds\\Pasta1.xlsx";

            // Define a codificação a ser usada
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");

            // Lê o arquivo do Excel e carrega os valores de ID em uma lista
            var listaId = new List<string>();
            using (var stream = File.Open(arquivoExcel, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration() { FallbackEncoding = encoding }))
                {

                    DataSet dataSet = reader.AsDataSet();
                    System.Data.DataTable dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string id = row.Field<string>("Column0");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            listaId.Add(id);
                        }
                    }
                }
            }

            List<string> listaCodPar = new List<string>();


            foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 c100 in _SpedFiscal.BlocoC.RegC001.RegC100s)
            {
                if (c100 is not null && c100.DtDoc is not null && c100.DtEs is not null)
                {
                    if (listaId.Contains(c100.ChvNfe))
                    {
                        listaCodPar.Add(c100.CodPart);
                        string linha = $"|C100|{c100.IndOper}|{c100.IndEmit}|{c100.CodPart}|{c100.CodMod}|0{c100.CodSit}|{c100.Ser}|{c100.NumDoc}|{c100.ChvNfe}|{c100.DtDoc.Value.ToString("ddMMyyyy")}|{c100.DtEs.Value.ToString("ddMMyyyy")}|{c100.VlDoc:0.00}|{c100.IndPgto}|{c100.VlDesc:0.00}|{c100.VlAbatNt:0}|{c100.VlMerc}|{c100.IndFrt}|{c100.VlFrt}|{c100.VlSeg:0}|{c100.VlOutDa:0}|{c100.VlBcIcms:0.00}|{c100.VlIcms}|{c100.VlBcIcmsSt:0}|{c100.VlIcmsSt:0}|{c100.VlIpi:0}|{c100.VlPis:0.00}|{c100.VlCofins:0.00}|{c100.VlPisSt:0}|{c100.VlCofinsSt:0}|\n";
                        File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linha, Encoding.UTF8);

                        if (c100.RegC101 is not null)
                        {
                            string linhac101 = $"|C101|{c100.RegC101.VlFcpUfDest}|{c100.RegC101.VlIcmsUfDest}|{c100.RegC101.VlIcmsUfRem}|\n";
                            File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhac101, Encoding.UTF8);
                        }
                        if (c100.RegC170s is not null)
                        {
                            foreach (var c170 in c100.RegC170s)
                            {
                                string linhaC170 = $"|C170|{c170.NumItem}|{c170.CodItem}|{c170.DescrCompl}|{c170.Qtd:0.00000}|{c170.Unid}|{c170.VlItem:0.00}|{c170.VlDesc:0.00}|0|{c170.CstIcms:000}|{c170.Cfop}|{c170.CodNat}|{c170.VlBcIcms:0.00}|{c170.AliqIcms}|{c170.VlIcms:0.00}|{c170.VlBcIcmsSt:0}|{c170.AliqSt:0}|0|0|{c170.CstIpi}|{c170.CodEnq}|{c170.VlBcIpi}|{c170.AliqIpi}|{c170.VlIpi}|{c170.CstPis}|{c170.VlBcPis}|{c170.AliqPis}|{c170.QuantBcPis}|{c170.AliqPisReais}|{c170.VlPis}|{c170.CstCofins}|{c170.VlBcCofins}|{c170.AliqCofins}|{c170.QuantBcCofins}|{c170.AliqCofinsReais}|{c170.VlCofins}|{c170.CodCta}|{c170.VlAbatNt}|\n";
                                File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC170, Encoding.UTF8);
                            }
                        }
                        if (c100.RegC190s is not null)
                        {
                            foreach (var c190 in c100.RegC190s)
                            {
                                string linhaC190 = $"|C190|{c190.CstIcms:000}|{c190.Cfop}|{c190.AliqIcms}|{c190.VlOpr}|{c190.VlBcIcms}|{c190.VlIcms}|{c190.VlBcIcmsSt}|{c190.VlIcmsSt}|{c190.VlRedBc}|{c190.VlIpi}|{c190.CodObs}|\n";
                                File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC190, Encoding.UTF8);

                                if (c190.RegC191 is not null)
                                {
                                    string linhaC191 = $"|C191|{c190.RegC191.VlFcpOp}|{c190.RegC191.VlFcpSt}|{c190.RegC191.VlFcpRet}|\n";
                                    File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\saida.txt", linhaC191, Encoding.UTF8);
                                }
                            }
                        }

                    }
                }
            }

            foreach (FiscalBr.EFDFiscal.Bloco0.Registro0150 r0150 in _SpedFiscal.Bloco0.Reg0001.Reg0150s)
            {

                if (listaCodPar.Contains(r0150.CodPart))
                {
                    string linha0150 = $"|0150|{r0150.CodPart}|{r0150.Nome}|{r0150.CodPais}|{r0150.Cnpj}|{r0150.Cpf}|{r0150.Ie}|{r0150.CodMun}|{r0150.Suframa}|{r0150.End}|{r0150.Num}|{r0150.Compl}|{r0150.Bairro}|\n";
                    File.AppendAllText(@"C:\Users\Micro\Documents\Registros_Faltam\0150.txt", linha0150, Encoding.UTF8);
                }

            }
        }
        public void ajuste0205Fiscal(ArquivoEFDFiscal _SpedFiscal)
        {
            foreach (var r0200 in _SpedFiscal.Bloco0.Reg0001.Reg0200s)
            {
                if (r0200.Reg0205s != null && r0200.Reg0205s.Count > 1)
                {
                    for (int i = 0; i < r0200.Reg0205s.Count; i++)
                    {
                        for (int j = i + 1; j < r0200.Reg0205s.Count; j++)
                        {
                            var r205A = r0200.Reg0205s[i];
                            var r205B = r0200.Reg0205s[j];

                            if (r205A.DtIni <= r205B.DtFin && r205B.DtIni <= r205A.DtFin)
                            {
                                if (r205A.DtFin.Day == DateTime.DaysInMonth(r205A.DtFin.Year, r205A.DtFin.Month))
                                {
                                    r205B.DtIni = r205A.DtFin.AddDays(1);
                                }
                                else if (r205B.DtFin.Day == DateTime.DaysInMonth(r205B.DtFin.Year, r205B.DtFin.Month))
                                {
                                    r205A.DtFin = r205B.DtIni.AddDays(-1);
                                }
                                else
                                {
                                    var midDate = r205A.DtFin.AddDays(1);
                                    r205B.DtIni = midDate;
                                    r205B.DtFin = r205B.DtFin.AddDays(midDate.Month == r205B.DtFin.Month ? 1 : -r205B.DtFin.Day + 1);
                                }
                            }
                        }
                    }


                    foreach (var r205 in r0200.Reg0205s)
                    {
                        if (r205.CodAntItem is not null && r205.DescrAntItem is not null)
                        {
                            if (r0200.Reg0205s.Count == 1 || r205.Equals(r0200.Reg0205s.First()))
                            {
                                r205.CodAntItem = null;
                            }
                            else
                            {
                                r205.DescrAntItem = null;
                            }
                        }
                        else if (r205.CodAntItem is not null)
                        {
                            if (r0200.Reg0205s.Count == 1 || r205.Equals(r0200.Reg0205s.First()))
                            {
                                r205.CodAntItem = null;
                            }
                            else
                            {
                                r205.DescrAntItem = null;
                            }
                        }
                        else if (r205.DescrAntItem is not null)
                        {
                            if (r0200.Reg0205s.Count == 1 || r205.Equals(r0200.Reg0205s.First()))
                            {
                                r205.DescrAntItem = null;
                            }
                            else
                            {
                                r205.CodAntItem = null;
                            }
                        }

                        if (r205.DtIni < new DateTime(2003, 01, 01))
                        {
                            r205.DtIni = new DateTime(2022, 09, 01);
                        }
                    }
                }
            }
        }
        public void ajusteVlIcmsC100(ArquivoEFDFiscal _SpedFiscal)
        {
            foreach (var c100 in _SpedFiscal.BlocoC.RegC001.RegC100s)
            {
                if (c100.RegC190s is not null)
                {
                    decimal valorTotalIcms = 0;

                    var registrosC190 = c100.RegC190s
                        .GroupBy(r => new { r.Cfop });

                    // Soma os valores dos registros c190 iguais
                    foreach (var grupoC190 in registrosC190)
                    {
                        var valorTotalGrupo = grupoC190.Sum(r => r.VlIcms);
                        valorTotalIcms = valorTotalGrupo;
                    }

                    c100.VlIcms = valorTotalIcms;
                }
            }
        }
        public void ajusteIEFiscal(ArquivoEFDFiscal _SpedFiscal)
        {
            foreach (var r150 in _SpedFiscal.Bloco0.Reg0001.Reg0150s)
            {
                if (r150.Ie is not null && r150.CodMun is not null)
                {
                    if (r150.CodMun.StartsWith("31") && r150.Ie.Length == 11)
                    {
                        string aux = "00";
                        r150.Ie = r150.Ie.Insert(0, aux);
                    }

                    else if (r150.CodMun.StartsWith("31") && r150.Ie.Length == 12)
                    {
                        string aux = "0";
                        r150.Ie = r150.Ie.Insert(0, aux);
                    }

                }
            }
        }
        public void ajusteCstCofins(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            List<string> listaCOD = new() { "16206", "16207", "16208", "16211", "16140", "16141", "16163", "16164", "7895345001781", "17239", "17245", "17236", "17242", "17245", "17248", "17324", "17328", "17437", "17450", "17453", "17430", "17434", "17657", "17660", "17700","17921" };
            List<string> listaCODAzeite = new() { "16126", "16127" };
            foreach (FiscalBr.EFDContribuicoes.BlocoC.RegistroC010 r010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var c100 in r010.RegC100s)
                {
                    if (c100.RegC170s is not null)
                    {
                        foreach (var c170 in c100.RegC170s)
                        {
                            if (c100.IndOper == 1)
                            {

                                if (listaCOD.Contains(c170.CodItem))
                                {
                                    c170.CstCofins = 04;
                                    c170.CstPis = 04;
                                    c170.AliqCofins = 0;
                                    c170.AliqCofinsQuant = 0;
                                    c170.VlCofins = 0;
                                    c170.VlBcCofins = 0;
                                    c170.QuantBcCofins = 0;
                                    c170.VlBcPis = 0;
                                    c170.VlPis = 0;
                                    c170.QuantBcPis = 0;
                                    c170.AliqPis = 0;
                                    c170.AliqPisQuant = 0;
                                    c100.VlPis = 0;
                                    c100.VlCofins = 0;
                                    c100.VlCofinsSt = 0;
                                    c100.VlPisSt = 0;
                                }
                                if (listaCODAzeite.Contains(c170.CodItem))
                                {
                                    c170.CstCofins = 06;
                                    c170.CstPis = 06;
                                    c170.AliqCofins = 0;
                                    c170.AliqCofinsQuant = 0;
                                    c170.VlCofins = 0;
                                    c170.VlBcCofins = 0;
                                    c170.QuantBcCofins = 0;
                                    c170.VlBcPis = 0;
                                    c170.VlPis = 0;
                                    c170.QuantBcPis = 0;
                                    c170.AliqPis = 0;
                                    c170.AliqPisQuant = 0;
                                    c100.VlPis = 0;
                                    c100.VlCofins = 0;
                                    c100.VlCofinsSt = 0;
                                    c100.VlPisSt = 0;
                                }
                            }

                            if (c100.IndOper == 0)
                            {

                                if (listaCOD.Contains(c170.CodItem))
                                {
                                    c170.CstCofins = 70;
                                    c170.CstPis = 70;
                                    c170.AliqCofins = 0;
                                    c170.AliqCofinsQuant = 0;
                                    c170.VlCofins = 0;
                                    c170.VlBcCofins = 0;
                                    c170.QuantBcCofins = 0;
                                    c170.VlBcPis = 0;
                                    c170.VlPis = 0;
                                    c170.QuantBcPis = 0;
                                    c170.AliqPis = 0;
                                    c170.AliqPisQuant = 0;
                                    c100.VlPis = 0;
                                    c100.VlCofins = 0;
                                    c100.VlCofinsSt = 0;
                                    c100.VlPisSt = 0;
                                }
                                if (listaCODAzeite.Contains(c170.CodItem))
                                {
                                    c170.CstCofins = 73;
                                    c170.CstPis = 73;
                                    c170.AliqCofins = 0;
                                    c170.AliqCofinsQuant = 0;
                                    c170.VlCofins = 0;
                                    c170.VlBcCofins = 0;
                                    c170.QuantBcCofins = 0;
                                    c170.VlBcPis = 0;
                                    c170.VlPis = 0;
                                    c170.QuantBcPis = 0;
                                    c170.AliqPis = 0;
                                    c170.AliqPisQuant = 0;
                                    c100.VlPis = 0;
                                    c100.VlCofins = 0;
                                    c100.VlCofinsSt = 0;
                                    c100.VlPisSt = 0;
                                }
                            }
                        }
                    }
                }
            }

        }
        public void ajusteTemporario(ArquivoEFDContribuicoes _SpedContribuicoes)
        {
            foreach (RegistroC010 rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (var c100 in rc010.RegC100s)
                {
                    if (c100.RegC170s is not null)
                    {
                        if (rc010.Cnpj == "24867555000608") {
                            foreach (var c170 in c100.RegC170s)
                            {
                                if (c170.Cfop == 5102 || c170.Cfop == 6102 || c170.Cfop == 6108 || c170.Cfop == 6106 || c170.Cfop == 5106)
                                {
                                    c170.VlItem = c170.VlItem + c170.VlIcms;
                                    c170.VlBcCofins = c170.VlBcCofins + c170.VlIcms;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}


