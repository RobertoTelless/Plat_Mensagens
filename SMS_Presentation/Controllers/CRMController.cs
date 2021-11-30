using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using EntitiesServices.Work_Classes;
using AutoMapper;
using PlatMensagem_Solution.ViewModels;
using System.IO;
using Correios.Net;
using Canducci.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using EntitiesServices.Attributes;
using OfficeOpenXml.Table;
using EntitiesServices.WorkClasses;
using System.Threading.Tasks;

namespace SMS_Presentation.Controllers
{
    public class CRMController : Controller
    {
        private readonly ICRMAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IConfiguracaoAppService confApp;
        private readonly IMensagemAppService menApp;

        private String msg;
        private Exception exception;
        CRM objeto = new CRM();
        CRM objetoAntes = new CRM();
        List<CRM> listaMaster = new List<CRM>();
        String extensao;

        public CRMController(ICRMAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps, IConfiguracaoAppService confApps, IMensagemAppService menApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
            confApp = confApps;
            menApp = menApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return View();
        }

        public ActionResult Voltar()
        {

            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaCRM()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensCRM"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<CRM>)Session["ListaCRM"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaCRM"] = listaMaster;
            }
            Session["CRM"] = null;
            ViewBag.Listas = (List<CRM>)Session["ListaCRM"];
            ViewBag.Title = "CRM";
            ViewBag.Origem = new SelectList(baseApp.GetAllOrigens().OrderBy(p => p.CROR_NM_NOME), "CROR_CD_ID", "CROR_NM_NOME");
            List<SelectListItem> visao = new List<SelectListItem>();
            visao.Add(new SelectListItem() { Text = "Lista", Value = "1" });
            visao.Add(new SelectListItem() { Text = "Kanban", Value = "2" });
            ViewBag.Visao = new SelectList(visao, "Value", "Text");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Prospecção", Value = "1" });
            status.Add(new SelectListItem() { Text = "Contato Realizado", Value = "2" });
            status.Add(new SelectListItem() { Text = "Proposta Apresentada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Negociação", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrado", Value = "5" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            List<SelectListItem> adic = new List<SelectListItem>();
            adic.Add(new SelectListItem() { Text = "Ativos", Value = "1" });
            adic.Add(new SelectListItem() { Text = "Arquivados", Value = "2" });
            adic.Add(new SelectListItem() { Text = "Cancelados", Value = "3" });
            adic.Add(new SelectListItem() { Text = "Falhados", Value = "4" });
            adic.Add(new SelectListItem() { Text = "Sucesso", Value = "5" });
            ViewBag.Adic = new SelectList(adic, "Value", "Text");
            List<SelectListItem> fav = new List<SelectListItem>();
            fav.Add(new SelectListItem() { Text = "Sim", Value = "1" });
            fav.Add(new SelectListItem() { Text = "Não", Value = "0" });
            ViewBag.Favorito = new SelectList(fav, "Value", "Text");
            Session["IncluirCRM"] = 0;

            // Indicadores
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            if (Session["MensCRM"] != null)
            {
                if ((Int32)Session["MensCRM"] == 2)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensCRM"] == 3)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0035", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensCRM"] == 4)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0036", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            Session["VoltaCRM"] = 1;
            Session["IncluirCliente"] = 0;
            objeto = new CRM();
            if (Session["FiltroCRM"] != null)
            {
                objeto = (CRM)Session["FiltroCRM"];
            }
            return View(objeto);
        }

        public ActionResult RetirarFiltroCRM()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaCRM"] = null;
            Session["FiltroCRM"] = null;
            return RedirectToAction("MontarTelaCRM");
        }

        [HttpPost]
        public ActionResult FiltrarCRM(CRM item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                List<CRM> listaObj = new List<CRM>();
                Session["FiltroCRM"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.CRM1_IN_STATUS, item.CRM1_DT_CRIACAO, item.CRM1_DT_CANCELAMENTO, item.ORIG_CD_ID, item.CRM1_IN_ATIVO, item.CRM1_NM_NOME, item.CRM1_DS_DESCRICAO, item.CRM1_IN_ESTRELA, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCRM"] = 1;
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaCRM"] = listaObj;
                return RedirectToAction("MontarTelaCRM");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaCRM");
            }
        }

        public ActionResult VoltarBaseCRM()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("MontarTelaCRM");
        }

        [HttpGet]
        public ActionResult ExcluirProcesso(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensCRM"] = 2;
                    return RedirectToAction("MontarTelaCRM");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CRM item = baseApp.GetItemById(id);
            objetoAntes = (CRM)Session["CRM"];
            item.CRM1_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensCRM"] = 4;
                return RedirectToAction("MontarTelaCRM");
            }
            Session["ListaCRM"] = null;
            return RedirectToAction("MontarTelaCRM");
        }

        [HttpGet]
        public ActionResult ReativarProcesso(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensCRM"] = 2;
                    return RedirectToAction("MontarTelaCRM");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CRM item = baseApp.GetItemById(id);
            objetoAntes = (CRM)Session["CRM"];
            item.CRM1_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            Session["ListaCRM"] = null;
            return RedirectToAction("MontarTelaCRM");
        }

        [HttpGet]
        public ActionResult EstrelaSim(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensCRM"] = 2;
                    return RedirectToAction("MontarTelaCRM");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CRM item = baseApp.GetItemById(id);
            objetoAntes = (CRM)Session["CRM"];
            item.CRM1_IN_ESTRELA = 1;
            Int32 volta = baseApp.ValidateEdit(item, item);
            Session["ListaCRM"] = null;
            return RedirectToAction("MontarTelaCRM");
        }

        [HttpGet]
        public ActionResult EstrelaNao(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensCRM"] = 2;
                    return RedirectToAction("MontarTelaCRM");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CRM item = baseApp.GetItemById(id);
            objetoAntes = (CRM)Session["CRM"];
            item.CRM1_IN_ESTRELA = 0;
            Int32 volta = baseApp.ValidateEdit(item, item);
            Session["ListaCRM"] = null;
            return RedirectToAction("MontarTelaCRM");
        }

        public ActionResult GerarRelatorioListaCRM()
        {            
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "ProcessosLista" + "_" + data + ".pdf";
            List<CRM> lista = (List<CRM>)Session["ListaCRM"];
            CRM filtro = (CRM)Session["FiltroCRM"];
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFontO = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.ORANGE);
            Font meuFontP = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.BLUE);
            Font meuFontE = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.GREEN);
            Font meuFontD = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
            Font meuFontS = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/favicon_SystemBR.jpg"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Processos - Listagem", meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Grid
            table = new PdfPTable(new float[] { 70f, 150f, 160f, 80f, 150f, 80f, 80f, 80f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Processos selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 8;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Favorito", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cliente", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Título", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Status", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Próxima Ação", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Data Prevista", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Origem", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Situação", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (CRM item in lista)
            {
                if (item.CRM1_IN_ESTRELA == 1)
                {
                    cell = new PdfPCell(new Paragraph("Sim", meuFontE))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_ESTRELA == 0)
                {
                    cell = new PdfPCell(new Paragraph("Não", meuFontO))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(item.CLIENTE.CLIE_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(item.CRM1_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                if (item.CRM1_IN_STATUS == 1)
                {
                    cell = new PdfPCell(new Paragraph("Prospecção", meuFontO))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_STATUS == 2)
                {
                    cell = new PdfPCell(new Paragraph("Contato Realizado", meuFontE))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_STATUS == 3)
                {
                    cell = new PdfPCell(new Paragraph("Proposta Apresentada", meuFontE))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_STATUS == 4)
                {
                    cell = new PdfPCell(new Paragraph("Negociação", meuFontE))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_STATUS == 5)
                {
                    cell = new PdfPCell(new Paragraph("Encerrado", meuFontP))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(item.CRM_ACAO.Where(p => p.CRAC_IN_ATIVO == 1).OrderByDescending(m => m.CRAC_DT_PREVISTA).FirstOrDefault().CRAC_DS_DESCRICAO, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                if (item.CRM_ACAO.Where(p => p.CRAC_IN_ATIVO == 1).OrderByDescending(m => m.CRAC_DT_PREVISTA).FirstOrDefault().CRAC_DT_PREVISTA.Value.Date >= DateTime.Today.Date)
                {
                    cell = new PdfPCell(new Paragraph(item.CRM_ACAO.Where(p => p.CRAC_IN_ATIVO == 1).OrderByDescending(m => m.CRAC_DT_PREVISTA).FirstOrDefault().CRAC_DT_PREVISTA.Value.ToShortDateString(), meuFontE))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else
                {
                    cell = new PdfPCell(new Paragraph(item.CRM_ACAO.Where(p => p.CRAC_IN_ATIVO == 1).OrderByDescending(m => m.CRAC_DT_PREVISTA).FirstOrDefault().CRAC_DT_PREVISTA.Value.ToShortDateString(), meuFontD))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(item.CRM_ORIGEM.CROR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);

                if (item.CRM1_IN_ATIVO == 1)
                {
                    cell = new PdfPCell(new Paragraph("Ativo", meuFontE))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_ATIVO == 2)
                {
                    cell = new PdfPCell(new Paragraph("Arquivado", meuFontO))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_ATIVO == 3)
                {
                    cell = new PdfPCell(new Paragraph("Cancelado", meuFontO))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_ATIVO == 4)
                {
                    cell = new PdfPCell(new Paragraph("Falhado", meuFontD))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.CRM1_IN_ATIVO == 5)
                {
                    cell = new PdfPCell(new Paragraph("Sucesso", meuFontP))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                table.AddCell(cell);
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Rodapé
            Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            String parametros = String.Empty;
            Int32 ja = 0;
            if (filtro != null)
            {
                if (filtro.CRM1_IN_STATUS > 0)
                {
                    if (filtro.CRM1_IN_STATUS == 1)
                    {
                        parametros += "Status: Prospecção";
                    }
                    else if (filtro.CRM1_IN_STATUS == 2)
                    {
                        parametros += "Status: Contato Realizado";
                    }
                    else if (filtro.CRM1_IN_STATUS == 3)
                    {
                        parametros += "Status: Proposta Apresentada";
                    }
                    else if (filtro.CRM1_IN_STATUS == 4)
                    {
                        parametros += "Status: Em Negociação";
                    }
                    else if (filtro.CRM1_IN_STATUS == 5)
                    {
                        parametros += "Status: Encerrado";
                    }
                    ja = 1;
                }

                if (filtro.CRM1_DT_CRIACAO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Data Início: " + filtro.CRM1_DT_CRIACAO.Value.ToShortDateString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += "e Data Início: " + filtro.CRM1_DT_CRIACAO.Value.ToShortDateString();
                    }
                }

                if (filtro.CRM1_DT_CANCELAMENTO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Data Final: " + filtro.CRM1_DT_CANCELAMENTO.Value.ToShortDateString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += "e Data Final: " + filtro.CRM1_DT_CANCELAMENTO.Value.ToShortDateString();
                    }
                }

                if (filtro.ORIG_CD_ID > 0)
                {
                    if (ja == 0)
                    {
                        parametros += "Origem: " + filtro.CRM_ORIGEM.CROR_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Origem: " + filtro.CRM_ORIGEM.CROR_NM_NOME;
                    }
                }

                if (filtro.CRM1_IN_ATIVO > 0)
                {
                    if (ja == 0)
                    {
                        if (filtro.CRM1_IN_ATIVO == 1)
                        {
                            parametros += "Situação: Ativo";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 2)
                        {
                            parametros += "Situação: Arquivado";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 3)
                        {
                            parametros += "Situação: Cancelado";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 4)
                        {
                            parametros += "Situação: Falhado";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 5)
                        {
                            parametros += "Situação: Sucesso";
                        }
                        ja = 1;
                    }
                    else
                    {
                        if (filtro.CRM1_IN_ATIVO == 1)
                        {
                            parametros += "e Situação: Ativo";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 2)
                        {
                            parametros += "e Situação: Arquivado";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 3)
                        {
                            parametros += "e Situação: Cancelado";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 4)
                        {
                            parametros += "e Situação: Falhado";
                        }
                        else if (filtro.CRM1_IN_ATIVO == 5)
                        {
                            parametros += "e Situação: Sucesso";
                        }
                    }
                }

                if (filtro.CRM1_IN_ESTRELA > 0)
                {
                    if (ja == 0)
                    {
                        if (filtro.CRM1_IN_ESTRELA == 1)
                        {
                            parametros += "Favorito: Sim";
                        }
                        else if (filtro.CRM1_IN_ESTRELA == 0)
                        {
                            parametros += "Favorito: Não";
                        }
                        ja = 1;
                    }
                    else
                    {
                        if (filtro.CRM1_IN_ESTRELA == 1)
                        {
                            parametros += "e Favorito: Sim";
                        }
                        else if (filtro.CRM1_IN_ESTRELA == 0)
                        {
                            parametros += "e Favorito: Não";
                        }
                    }
                }

                if (filtro.CRM1_NM_NOME != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Título: " + filtro.CRM1_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Título: " + filtro.CRM1_NM_NOME;
                    }
                }

                if (filtro.CRM1_DS_DESCRICAO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Geral: " + filtro.CRM1_DS_DESCRICAO;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Geral: " + filtro.CRM1_DS_DESCRICAO;
                    }
                }
                if (ja == 0)
                {
                    parametros = "Nenhum filtro definido.";
                }
            }
            else
            {
                parametros = "Nenhum filtro definido.";
            }
            Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            // Linha Horizontal
            Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line3);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();
            return RedirectToAction("MontarTelaCRM");
        }

        [HttpGet]
        public ActionResult IncluirProcessoCRM()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensCRM"] = 2;
                    return RedirectToAction("MontarTelaCRM", "CRM");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Origem = new SelectList(baseApp.GetAllOrigens().OrderBy(p => p.CROR_NM_NOME), "CROR_CD_ID", "CROR_NM_NOME");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Prospecção", Value = "1" });
            status.Add(new SelectListItem() { Text = "Contato Realizado", Value = "2" });
            status.Add(new SelectListItem() { Text = "Proposta Apresentada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Negociação", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrado", Value = "5" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            List<SelectListItem> fav = new List<SelectListItem>();
            fav.Add(new SelectListItem() { Text = "Sim", Value = "1" });
            fav.Add(new SelectListItem() { Text = "Não", Value = "0" });
            ViewBag.Favorito = new SelectList(fav, "Value", "Text");
            Session["IncluirCRM"] = 0;
            Session["CRM"] = null;

            // Prepara view
            Session["CRMNovo"] = 0;
            CRM item = new CRM();
            CRMViewModel vm = Mapper.Map<CRM, CRMViewModel>(item);
            vm.ASSI_CD_ID = idAss;
            vm.CRM1_DT_CRIACAO = DateTime.Today.Date;
            vm.CRM1_IN_ATIVO = 1;
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirProcessoCRM(CRMViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ViewBag.Origem = new SelectList(baseApp.GetAllOrigens().OrderBy(p => p.CROR_NM_NOME), "CROR_CD_ID", "CROR_NM_NOME");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Prospecção", Value = "1" });
            status.Add(new SelectListItem() { Text = "Contato Realizado", Value = "2" });
            status.Add(new SelectListItem() { Text = "Proposta Apresentada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Negociação", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrado", Value = "5" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            List<SelectListItem> fav = new List<SelectListItem>();
            fav.Add(new SelectListItem() { Text = "Sim", Value = "1" });
            fav.Add(new SelectListItem() { Text = "Não", Value = "0" });
            ViewBag.Favorito = new SelectList(fav, "Value", "Text");

            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CRM item = Mapper.Map<CRMViewModel, CRM>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCRM"] = 3;
                        return RedirectToAction("MontarTelaCRM");
                    }

                    // Cria pasta
                    String caminho = "/Imagens/" + idAss.ToString() + "/CRM/" + item.CRM1_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Listas
                    listaMaster = new List<CRM>();
                    Session["ListaCRM"] = null;
                    Session["IncluirCRM"] = 1;
                    Session["CRMNovo"] = item.CRM1_CD_ID;
                    Session["IdCRM"] = item.CRM1_CD_ID;

                    // Processa Anexos
                    if (Session["FileQueueCRM"] != null)
                    {
                        List<FileQueue> fq = (List<FileQueue>)Session["FileQueueCRM"];

                        foreach (var file in fq)
                        {
                            if (file.Profile == null)
                            {
                                UploadFileQueueCRM(file);
                            }
                        }
                        Session["FileQueueCRM"] = null;
                    }

                    if ((Int32)Session["VoltaCRM"] == 3)
                    {
                        Session["VoltaCRM"] = 0;
                        return RedirectToAction("IncluirProcessoCRM", "CRM");
                    }
                    return RedirectToAction("MontarTelaCRm");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpPost]
        public void UploadFileToSession(IEnumerable<HttpPostedFileBase> files, String profile)
        {
            List<FileQueue> queue = new List<FileQueue>();
            foreach (var file in files)
            {
                FileQueue f = new FileQueue();
                f.Name = Path.GetFileName(file.FileName);
                f.ContentType = Path.GetExtension(file.FileName);

                MemoryStream ms = new MemoryStream();
                file.InputStream.CopyTo(ms);
                f.Contents = ms.ToArray();

                if (profile != null)
                {
                    if (file.FileName.Equals(profile))
                    {
                        f.Profile = 1;
                    }
                }
                queue.Add(f);
            }
            Session["FileQueueCRM"] = queue;
        }

        [HttpPost]
        public ActionResult UploadFileQueueCRM(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdCRM"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensCRM"] = 10;
                return RedirectToAction("VoltarAnexoCRM");
            }

            CRM item = baseApp.GetItemById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensCRM"] = 11;
                return RedirectToAction("VoltarAnexoCRM");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/CRM/" + item.CRM1_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            CRM_ANEXO foto = new CRM_ANEXO();
            foto.CRAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.CRAN_DT_ANEXO = DateTime.Today;
            foto.CRAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.CRAN_IN_TIPO = tipo;
            foto.CRAN_NM_TITULO = fileName;
            foto.CRM1_CD_ID = item.CRM1_CD_ID;

            item.CRM_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item);
            return RedirectToAction("VoltarAnexoCRM");
        }

        [HttpPost]
        public ActionResult UploadFileCRM(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdCRM"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensCRM"] = 10;
                return RedirectToAction("VoltarAnexoCRM");
            }

            CRM item = baseApp.GetItemById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensCRM"] = 11;
                return RedirectToAction("VoltarAnexoCRM");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/CRM/" + item.CRM1_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            CRM_ANEXO foto = new CRM_ANEXO();
            foto.CRAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.CRAN_DT_ANEXO = DateTime.Today;
            foto.CRAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.CRAN_IN_TIPO = tipo;
            foto.CRAN_NM_TITULO = fileName;
            foto.CRM1_CD_ID = item.CRM1_CD_ID;

            item.CRM_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item);
            return RedirectToAction("VoltarAnexoCRM");
        }

        public ActionResult VoltarAnexoCRM()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("EditarProcessoCRM", new { id = (Int32)Session["IdCRM"] });
        }





    }
}