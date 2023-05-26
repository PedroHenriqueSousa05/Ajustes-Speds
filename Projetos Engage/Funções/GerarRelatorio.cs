using System.IO;
using System.Text;
using FiscalBr.EFDContribuicoes;
using static FiscalBr.EFDContribuicoes.BlocoC;
using static FiscalBr.EFDContribuicoes.BlocoD;
using static FiscalBr.EFDContribuicoes.BlocoA;
using FiscalBr.EFDFiscal;
using static FiscalBr.EFDFiscal.BlocoC;
using System;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
namespace ProjetoSpeds.Funções
{
    internal class gerarRelatorio
    {
        string linha = "";
        public void relatorioCSV(ArquivoEFDContribuicoes _SpedContribuicoes)
        {

            StringBuilder RelatorioSped = new();
            String competencia = Convert.ToString(_SpedContribuicoes.Bloco0.Reg0000.DtFin).Replace("/", "").Replace(" ", "").Replace(":", "").Substring(2, 6);

            RelatorioSped.AppendLine("BLOCO;CNPJ;NUMERO_DOCUMENTO;IND_OPERACAO;CHAVENFE;VL_FRETE;CFOP;COD_ITEM;VALOR_ITEM;VL_BC_ICMS;VL_ICMS;ALIQ_ICMS;VL_BC_PIS;AL_PIS;VL_PIS;VL_CST_PIS;VL_BC_COFINS;VL_COFINS;CST_COFINS;BC_IPI;AL_IPI;VL_IPI");

            foreach (RegistroC010 rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            {
                foreach (FiscalBr.EFDContribuicoes.BlocoC.RegistroC100 rc100 in rc010.RegC100s)
                {

                    if (rc100.RegC170s is not null)
                    {
                        foreach (FiscalBr.EFDContribuicoes.BlocoC.RegistroC170 rc170 in rc100.RegC170s)
                        {
                            RelatorioSped.AppendLine("BLOCO C;" +
                                                       $"{rc010.Cnpj};" +
                                                       $"{rc100.NumDoc};" +
                                                       $"{rc100.IndOper};" +
                                                       $"{rc100.ChvNfe};" +
                                                       $"{rc100.VlFrt};" +
                                                       $"{rc170.Cfop};" +
                                                       $"{rc170.CodItem};" +
                                                       $"{rc170.VlItem};" +
                                                       $"{rc170.VlBcIcms};" +
                                                       $"{rc170.VlIcms};" +
                                                       $"{rc170.AliqIcms};" +
                                                       $"{rc170.VlBcPis};" +
                                                       $"{rc170.AliqPis};" +
                                                       $"{rc170.VlPis};" +
                                                       $"{rc170.CstPis};" +
                                                       $"{rc170.VlBcCofins};" +
                                                       $"{rc170.VlCofins};" +
                                                       $"{rc170.CstCofins}" +
                                                       $"{rc170.VlBcIpi}" +
                                                       $"{rc170.AliqIpi}" +
                                                       $"{rc170.VlIpi}"
                                                       );

                        }
                    }
                }
            }

            //foreach (RegistroC010 rc010 in _SpedContribuicoes.BlocoC.RegC001.RegC010s)
            //{
            //    foreach (FiscalBr.EFDContribuicoes.BlocoC.RegistroC100 rc100 in rc010.RegC100s)
            //    {

            //        if (rc100.RegC170s is not null)
            //        {
            //            foreach (FiscalBr.EFDContribuicoes.BlocoC.RegistroC170 rc170 in rc100.RegC170s)
            //            {
            //                linha = (  "BLOCO C;" +
            //                                           $"{rc010.Cnpj};" +
            //                                           $"{rc100.NumDoc};" +
            //                                           $"{rc100.IndOper};" +
            //                                           $"{rc100.ChvNfe};" +
            //                                           $"{rc100.VlFrt};" +
            //                                           "valordocumento;" +
            //                                           $"{rc170.Cfop};"+
            //                                           $"{rc170.CodItem};" +
            //                                           $"{rc170.VlItem};" +
            //                                           $"{rc170.VlBcIcms};" +
            //                                           $"{rc170.VlIcms};" +
            //                                           $"{rc170.AliqIcms};" +
            //                                           $"{rc170.VlBcPis};" +
            //                                           $"{rc170.AliqPis};" +
            //                                           $"{rc170.VlPis};" +
            //                                           $"{rc170.CstPis};" +
            //                                           $"{rc170.VlBcCofins};" +
            //                                           $"{rc170.VlCofins};" +
            //                                           $"{rc170.CstCofins}" +
            //                                           $"{rc170.VlBcIpi}" +
            //                                           $"{rc170.AliqIpi}" +
            //                                           $"{rc170.VlIpi}"
            //                                           );

            //            }
            //            string novalinha = linha.Replace("valordocumento;", $"{rc100.VlDoc};");
            //            RelatorioSped.AppendLine(novalinha);
            //        }
            //    }                
            //}

            foreach (RegistroD010 rd010 in _SpedContribuicoes.BlocoD.RegD001.RegD010s)
            {
                if (rd010.RegD100s is not null)
                {
                    foreach (RegistroD100 rd100 in rd010.RegD100s)
                    {
                        if (rd100.RegD101s is not null)
                        {
                            foreach (RegistroD101 rd101 in rd100.RegD101s)
                            {
                                RelatorioSped.AppendLine("BLOCO D101;" +
                                                           $"{rd010.Cnpj};" +
                                                           $"{rd100.NumDoc};" +
                                                           $"{rd100.IndOper};" +
                                                           $"{rd100.ChvCTe};" +
                                                           "-;" +
                                                           "-;" +
                                                           "-;" +
                                                           $"{rd101.VlItem};" +
                                                           "-;" +
                                                           "-;" +
                                                           "-;" +
                                                           $"{rd101.AliqPis};" +
                                                           $"{rd101.VlPis};" +
                                                           $"{rd101.CstPis};" +
                                                           "-;" +
                                                           $"{rd101.VlBcPis};" +
                                                           "-;" +
                                                           "-;" +
                                                           "-;" +
                                                           "-;"
                                                          );
                            }
                        }
                    }
                }
            }
            if (_SpedContribuicoes.BlocoA.RegA001.RegA010s is not null)
            {
                foreach (RegistroA010 ra010 in _SpedContribuicoes.BlocoA.RegA001.RegA010s)
                {
                    foreach (RegistroA100 ra100 in ra010.RegA100s)
                    {
                        if (ra100.RegA170s is not null)
                        {
                            foreach (RegistroA170 ra170 in ra100.RegA170s)
                            {
                                RelatorioSped.AppendLine("BLOCO A;" +
                                                          $"{ra010.Cnpj};" +
                                                          $"{ra100.NumDoc};" +
                                                          $"{ra100.IndOper};" +
                                                          "-;" +
                                                          "-;" +
                                                          "-;" +
                                                          "-;" +
                                                          $"{ra170.VlItem};" +
                                                          "-;" +
                                                          "-;" +
                                                          $"{ra170.VlBcPis};" +
                                                          $"{ra170.AliqPis};" +
                                                          $"{ra170.VlPis};" +
                                                          $"{ra170.CstPis};" +
                                                          $"{ra170.VlBcCofins};" +
                                                          $"{ra170.VlCofins}" +
                                                          $"{ra170.CstCofins};" +
                                                          "-;" +
                                                          "-;" +
                                                          "-;"
                                                          );
                            }
                        }
                    }
                }
            }

            File.WriteAllText(path: $"C:\\Users\\Micro\\Documents\\Relatório_SPED\\relatorioSped_{competencia}.csv", RelatorioSped.ToString());
        }
        public void relatorioCSV(ArquivoEFDFiscal _Sped)
        {

            StringBuilder csvFiscal = new();
            String competencia = Convert.ToString(_Sped.Bloco0.Reg0000.DtFin).Replace("/", "").Replace(" ", "").Replace(":", "").Substring(2, 6);

            csvFiscal.AppendLine("BLOCO;CHAVE;NUMERO_NOTA;SERIE;CFOP;CST_ICMS;VALOR OPERACAO;BC ICMS;ALIQ ICMS;ICMS");

            foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 rc100 in _Sped.BlocoC.RegC001.RegC100s)
            {
                if (rc100.RegC190s is not null)
                {
                    foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC190 rc190 in rc100.RegC190s)
                    {
                        csvFiscal.AppendLine($"Bloco C;" +
                                             $"{rc100.ChvNfe};" +
                                             $"{rc100.NumDoc};" +
                                             $"{rc100.Ser};" +
                                             $"{rc190.Cfop};" +
                                             $"{rc190.CstIcms};" +
                                             $"{rc190.VlOpr};" +
                                             $"{rc190.VlBcIcms};" +
                                             $"{rc190.AliqIcms};" +
                                             $"{rc190.VlIcms};");

                    }
                }
            }

            foreach (FiscalBr.EFDFiscal.BlocoD.RegistroD100 rd100 in _Sped.BlocoD.RegD001.RegD100s)
            {
                if (rd100.RegD190s is not null)
                {
                    foreach (FiscalBr.EFDFiscal.BlocoD.RegistroD190 rd190 in rd100.RegD190s)
                    {
                        csvFiscal.AppendLine($"Bloco D;" +
                                             $"{rd100.ChvCte};" +
                                             $"{rd100.NumDoc};" +
                                             $"{rd100.Ser};" +
                                             $"{rd190.Cfop};" +
                                             $"{rd190.CstIcms};" +
                                             $"{rd190.VlOpr};" +
                                             $"{rd190.VlBcIcms};" +
                                             $"{rd190.AliqIcms};" +
                                             $"{rd190.VlIcms};");

                    }
                }
            }
            /*foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC100 rc100 in _Sped.BlocoC.RegC001.RegC100s)
            {
                if (rc100.RegC170s is not null)
                {
                    foreach (FiscalBr.EFDFiscal.BlocoC.RegistroC170 rc170 in rc100.RegC170s)
                    {
                        csvFiscal.AppendLine($"Bloco C;" +
                                             $"{rc100.ChvNfe};" +
                                             $"{rc100.NumDoc};" +
                                             $"{rc100.Ser};" +
                                             $"{rc170.Cfop};"+
                                             $"{rc170.CstIcms};" +
                                             $"{rc170.VlItem};" +
                                             $"{rc170.VlBcIcms};" +
                                             $"{rc170.AliqIcms};" +
                                             $"{rc170.VlIcms};");      
                        
                    }
                }
            }*/

            File.WriteAllText(path: $"C:\\Users\\Micro\\Documents\\Relatório_SPED\\relatorioSPED_FISCAL_{competencia}.csv", csvFiscal.ToString());
        }
       
    }
}
