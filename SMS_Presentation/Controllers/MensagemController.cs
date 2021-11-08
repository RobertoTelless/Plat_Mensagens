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
using CrossCutting;
using System.Net.Mail;
using System.Net.Http;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace SMS_Presentation.Controllers
{
    public class MensagemController : Controller
    {
        private readonly IMensagemAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IClienteAppService cliApp;
        private readonly IConfiguracaoAppService confApp;
        private readonly ITemplateAppService temApp;
        private readonly IGrupoAppService gruApp;
        private String msg;
        private Exception exception;
        MENSAGENS objeto = new MENSAGENS();
        MENSAGENS objetoAntes = new MENSAGENS();
        List<MENSAGENS> listaMaster = new List<MENSAGENS>();
        String extensao;

        public MensagemController(IMensagemAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps, IClienteAppService cliApps, IConfiguracaoAppService confApps, ITemplateAppService temApps, IGrupoAppService gruApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
            cliApp = cliApps;
            confApp = confApps;
            temApp = temApps;
            gruApp = gruApps;
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

        [HttpPost]
        public JsonResult BuscaNomeRazao(String nome)
        {
            Int32 isRazao = 0;
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<Hashtable> listResult = new List<Hashtable>();

            List<CLIENTE> clientes = cliApp.GetAllItens(idAss);

            if (nome != null)
            {
                List<CLIENTE> lstCliente = clientes.Where(x => x.CLIE_NM_NOME != null && x.CLIE_NM_NOME.ToLower().Contains(nome.ToLower())).ToList<CLIENTE>();

                if (lstCliente == null || lstCliente.Count == 0)
                {
                    isRazao = 1;
                    lstCliente = clientes.Where(x => x.CLIE_NM_RAZAO != null).ToList<CLIENTE>();
                    lstCliente = lstCliente.Where(x => x.CLIE_NM_RAZAO.ToLower().Contains(nome.ToLower())).ToList<CLIENTE>();
                }

                if (lstCliente != null)
                {
                    foreach (var item in lstCliente)
                    {
                        Hashtable result = new Hashtable();
                        result.Add("id", item.CLIE_CD_ID);
                        if (isRazao == 0)
                        {
                            result.Add("text", item.CLIE_NM_NOME);
                        }
                        else
                        {
                            result.Add("text", item.CLIE_NM_NOME + " (" + item.CLIE_NM_RAZAO + ")");
                        }
                        listResult.Add(result);
                    }
                }
            }
            return Json(listResult);
        }

        [HttpGet]
        public ActionResult MontarTelaMensagem()
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "OPR" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensMensagem"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<MENSAGENS>)Session["ListaMensagem"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaMensagem"] = listaMaster;
            }
            ViewBag.Listas = (List<MENSAGENS>)Session["ListaMensagem"];
            ViewBag.Title = "Mensagem";
            Session["Mensagem"] = null;
            List<SelectListItem> tipos = new List<SelectListItem>();
            tipos.Add(new SelectListItem() { Text = "E-Mail", Value = "1" });
            tipos.Add(new SelectListItem() { Text = "SMS", Value = "2" });
            tipos.Add(new SelectListItem() { Text = "WhatsApp", Value = "3" });
            ViewBag.Tipos = new SelectList(tipos, "Value", "Text");
            Session["IncluirMensagem"] = 0;

            // Indicadores
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            if (Session["MensMensagem"] != null)
            {
                if ((Int32)Session["MensMensagem"] == 2)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            Session["VoltaMensagem"] = 1;
            objeto = new MENSAGENS();
            if (Session["FiltroMensagem"] != null)
            {
                objeto = (MENSAGENS)Session["FiltroMensagem"];
            }
            objeto.MENS_DT_CRIACAO = DateTime.Today.Date;
            objeto.MENS_DT_ENVIO = DateTime.Today.Date;
            return View(objeto);
        }

        public ActionResult RetirarFiltroMensagem()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaMensagem"] = null;
            Session["FiltroMensagem"] = null;
            return RedirectToAction("MontarTelaMensagem");
        }

        public ActionResult MostrarTudoMensagem()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaMensagem"] = null;
            Session["FiltroMensagem"] = listaMaster;
            return RedirectToAction("MontarTelaMensagem");
        }

        [HttpPost]
        public ActionResult FiltrarMensagem(MENSAGENS item)
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                List<MENSAGENS> listaObj = new List<MENSAGENS>();
                Session["FiltroMensagem"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.MENS_DT_CRIACAO, item.MENS_DT_ENVIO, item.MENS_NM_CAMPANHA, item.MENS_TX_TEXTO, item.MENS_IN_TIPO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaMensagem"] = listaObj;
                return RedirectToAction("MontarTelaMensagem");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaMensagem");
            }
        }

        public ActionResult VoltarBaseMensagem()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("MontarTelaMensagem");
        }

        public ActionResult VoltarMensagemAnexo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 volta = (Int32)Session["VoltaMensagem"];
            if (volta == 1)
            {
                return RedirectToAction("MontarTelaMensagem");
            }
            else if (volta == 2)
            {
                return RedirectToAction("VoltarAnexoCliente", "Cliente");
            }
            return RedirectToAction("MontarTelaMensagem");
        }

        [HttpGet]
        [ValidateInput(false)]
        public ActionResult IncluirMensagem()
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
                    Session["MensMensagem"] = 2;
                    return RedirectToAction("MontarTelaMensagem", "Mensagem");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Clientes = new SelectList(cliApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss).OrderBy(p => p.GRUP_NM_NOME), "GRUP_CD_ID", "GRUP_NM_NOME");
            ViewBag.Cats = new SelectList(baseApp.GetAllTipos().OrderBy(p => p.CACL_NM_NOME), "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF().OrderBy(p => p.UF_SG_SIGLA), "UF_CD_ID", "UF_NM_NOME");
            Session["Mensagem"] = null;
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            sexo.Add(new SelectListItem() { Text = "Outros", Value = "3" });
            ViewBag.Sexo = new SelectList(sexo, "Value", "Text");
            List<SelectListItem> tipos = new List<SelectListItem>();
            tipos.Add(new SelectListItem() { Text = "E-Mail", Value = "1" });
            tipos.Add(new SelectListItem() { Text = "SMS", Value = "2" });
            tipos.Add(new SelectListItem() { Text = "WhatsApp", Value = "3" });
            ViewBag.Tipos = new SelectList(tipos, "Value", "Text");
            ViewBag.Status = new SelectList(baseApp.GetAllPosicao().OrderBy(p => p.POSI_NM_NOME), "POSI_CD_ID", "POSI_NM_NOME");
            ViewBag.Temp = new SelectList(temApp.GetAllItens(idAss).Where(p => p.TEMP_IN_TIPO != 0).ToList().OrderBy(p => p.TEMP_SG_SIGLA), "TEMP_CD_ID", "TEMP_SG_SIGLA");

            // Prepara view
            String header = temApp.GetByCode("TEMPBAS").TEMP_TX_CABECALHO;
            String body = temApp.GetByCode("TEMPBAS").TEMP_TX_CORPO;
            String footer = temApp.GetByCode("TEMPBAS").TEMP_TX_DADOS;

            if (Session["MensMensagem"] != null)
            {
                if ((Int32)Session["MensMensagem"] == 3)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0026", CultureInfo.CurrentCulture));
                }
            }

            Session["MensagemNovo"] = 0;
            MENSAGENS item = new MENSAGENS();
            MensagemViewModel vm = Mapper.Map<MENSAGENS, MensagemViewModel>(item);
            vm.ASSI_CD_ID = idAss;
            vm.MENS_DT_CRIACAO = DateTime.Now;
            vm.MENS_IN_ATIVO = 1;
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            vm.MENS_NM_CABECALHO = header;
            vm.MENS_NM_RODAPE = footer;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult IncluirMensagem(MensagemViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ViewBag.Clientes = new SelectList(cliApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss).OrderBy(p => p.GRUP_NM_NOME), "GRUP_CD_ID", "GRUP_NM_NOME");
            ViewBag.Cats = new SelectList(baseApp.GetAllTipos().OrderBy(p => p.CACL_NM_NOME), "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF().OrderBy(p => p.UF_SG_SIGLA), "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Temp = new SelectList(temApp.GetAllItens(idAss).Where(p => p.TEMP_IN_TIPO != 0).ToList().OrderBy(p => p.TEMP_SG_SIGLA), "TEMP_CD_ID", "TEMP_SG_SIGLA");
            Session["Mensagem"] = null;
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            sexo.Add(new SelectListItem() { Text = "Outros", Value = "3" });
            ViewBag.Sexo = new SelectList(sexo, "Value", "Text");
            List<SelectListItem> tipos = new List<SelectListItem>();
            tipos.Add(new SelectListItem() { Text = "E-Mail", Value = "1" });
            tipos.Add(new SelectListItem() { Text = "SMS", Value = "2" });
            tipos.Add(new SelectListItem() { Text = "WhatsApp", Value = "3" });
            ViewBag.Tipos = new SelectList(tipos, "Value", "Text");
            ViewBag.Status = new SelectList(baseApp.GetAllPosicao().OrderBy(p => p.POSI_NM_NOME), "POSI_CD_ID", "POSI_NM_NOME");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Checa mensagens
                    if (vm.MENS_IN_TIPO == 1)
                    {
                        if (String.IsNullOrEmpty(vm.MENS_TX_TEXTO))
                        {
                            Session["MensMensagem"] = 3;
                            return RedirectToAction("IncluirMensagem");
                        }
                    }
                    if (vm.MENS_IN_TIPO == 2)
                    {
                        if (String.IsNullOrEmpty(vm.MENS_TX_SMS))
                        {
                            Session["MensMensagem"] = 3;
                            return RedirectToAction("IncluirMensagem");
                        }
                    }

                    // Executa a operação
                    MENSAGENS item = Mapper.Map<MensagemViewModel, MENSAGENS>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                    }

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Mensagem/" + item.MENS_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    Session["IdMensagem"] = item.MENS_CD_ID;
                    if (Session["FileQueueMensagem"] != null)
                    {
                        List<FileQueue> fq = (List<FileQueue>)Session["FileQueueMensagem"];

                        foreach (var file in fq)
                        {
                            if (file.Profile == null)
                            {
                                UploadFileQueueMensagem(file);
                            }
                        }
                        Session["FileQueueMensagem"] = null;
                    }

                    // Processa
                    if (item.MENS_DT_AGENDAMENTO == null)
                    {
                        MENSAGENS mens = baseApp.GetItemById(item.MENS_CD_ID);
                        Session["IdMensagem"] = mens.MENS_CD_ID;
                        vm.MENS_CD_ID = mens.MENS_CD_ID;
                        vm.MENSAGEM_ANEXO = mens.MENSAGEM_ANEXO;
                        Int32 retGrava = ProcessarEnvioMensagem(vm, usuario);
                        if (retGrava > 0)
                        {

                        }
                    }

                    // Sucesso
                    listaMaster = new List<MENSAGENS>();
                    Session["ListaMensagem"] = null;
                    Session["MensagemNovo"] = item.MENS_CD_ID;
                    return RedirectToAction("MontarTelaMensagem");
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
            List<System.Net.Mail.Attachment> att = new List<System.Net.Mail.Attachment>();
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
                att.Add(new System.Net.Mail.Attachment(file.InputStream, f.Name));
                queue.Add(f);
            }
            Session["FileQueueMensagem"] = queue;
            Session["Attachments"] = att;
        }

        [HttpPost]
        public ActionResult UploadFileQueueMensagem(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdMensagem"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensMensagem"] = 10;
                return RedirectToAction("VoltarBaseMensagem");
            }

            MENSAGENS item = baseApp.GetItemById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensMensagem"] = 11;
                return RedirectToAction("VoltarBaseMensagem");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/Mensagem/" + item.MENS_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            MENSAGEM_ANEXO foto = new MENSAGEM_ANEXO();
            foto.MEAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.MEAN_DT_ANEXO = DateTime.Today;
            foto.MEAN_IN_ATIVO = 1;
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
            foto.MEAN_IN_TIPO = tipo;
            foto.MEAN_NM_TITULO = fileName.Length < 49 ? fileName : fileName.Substring(0,48);
            foto.MENS_CD_ID = item.MENS_CD_ID;

            item.MENSAGEM_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item);
            return RedirectToAction("VoltarBaseMensagem");
        }

        [ValidateInput(false)]
        public Int32 ProcessarEnvioMensagem(MensagemViewModel vm, USUARIO usuario)
        {
            // Recupera contatos
            Int32 idAss = (Int32)Session["IdAssinante"];
            CLIENTE cliente = null;
            GRUPO grupo = null;
            List<CLIENTE> listaCli = new List<CLIENTE>();
            Int32 escopo = 0;
            String erro = null;
            Int32 volta = 0;
            PlatMensagensEntities Db = new PlatMensagensEntities();
            MENSAGENS mens = baseApp.GetItemById(vm.MENS_CD_ID);

            // Nome
            if (vm.ID > 0)
            {                
                cliente = cliApp.GetItemById(vm.ID.Value);
                escopo = 1;
            }
            else if (vm.GRUPO > 0)
            {
                listaCli = new List<CLIENTE>();
                grupo = gruApp.GetItemById(vm.GRUPO.Value);
                foreach (GRUPO_CLIENTE item in grupo.GRUPO_CLIENTE)
                {
                    listaCli.Add(item.CLIENTE);
                }
                escopo = 2;
            }
            else
            {
                IQueryable<CLIENTE> query = Db.CLIENTE;
                escopo = 2;

                // Sexo
                if (vm.SEXO > 0)
                {
                    query = query.Where(p => p.CLIE_IN_SEXO == vm.SEXO);
                }

                // Cidade
                if (!String.IsNullOrEmpty(vm.CIDADE))
                {
                    query = query.Where(p => p.CLIE_NM_CIDADE.Contains(vm.CIDADE));
                }

                // UF
                if (vm.UF > 0)
                {
                    query = query.Where(p => p.UF_CD_ID == vm.UF);
                }

                // Categoria
                if (vm.CATEGORIA > 0)
                {
                    query = query.Where(p => p.CACL_CD_ID == vm.CATEGORIA);
                }

                // Status
                if (vm.STATUS > 0)
                {
                    query = query.Where(p => p.CLIE_IN_STATUS == vm.STATUS);
                }

                // Data Nascimento
                if (vm.DATA_NASC != null)
                {
                    query = query.Where(p => p.CLIE_DT_NASCIMENTO == vm.DATA_NASC);
                }

                // Processa filtro
                if (query != null)
                {
                    query = query.Where(p => p.ASSI_CD_ID == idAss);
                    listaCli = query.ToList<CLIENTE>();
                }
            }

            // Processa e-mail
            if (vm.MENS_IN_TIPO == 1)
            {
                CONFIGURACAO conf = confApp.GetItemById(usuario.ASSI_CD_ID);
                if (escopo == 1)
                {
                    // Prepara cabeçalho
                    String cab = vm.MENS_NM_CABECALHO.Replace("{Nome}", cliente.CLIE_NM_NOME);

                    // Prepara rodape
                    ASSINANTE assi = (ASSINANTE)Session["Assinante"];
                    String rod = vm.MENS_NM_RODAPE.Replace("{NomeRemetente}", assi.ASSI_NM_NOME);

                    // Prepara corpo do e-mail e trata link
                    StringBuilder str = new StringBuilder();
                    str.AppendLine(vm.MENS_TX_TEXTO);
                    if (!String.IsNullOrEmpty(vm.MENS_NM_LINK))
                    {
                        if (!vm.MENS_NM_LINK.Contains("www."))
                        {
                            vm.MENS_NM_LINK = "www." + vm.MENS_NM_LINK;
                        }
                        if (!vm.MENS_NM_LINK.Contains("http://"))
                        {
                            vm.MENS_NM_LINK = "http://" + vm.MENS_NM_LINK;
                        }
                        str.AppendLine("<a href='" + vm.MENS_NM_LINK + "'>Clique aqui para maiores informações</a>");
                    }
                    String body = str.ToString();                  
                    String emailBody = cab + body + rod;

                    // Checa e monta anexos
                    List<System.Net.Mail.Attachment> listaAnexo = new List<System.Net.Mail.Attachment>();
                    if (vm.MENSAGEM_ANEXO.Count > 0)
                    {
                        foreach (MENSAGEM_ANEXO item in vm.MENSAGEM_ANEXO)
                        {
                            String fn = Server.MapPath(item.MEAN_AQ_ARQUIVO);
                            System.Net.Mail.Attachment anexo = new System.Net.Mail.Attachment(fn);
                            listaAnexo.Add(anexo);
                        }
                    }

                    List<SendGrid.Helpers.Mail.Attachment> listaAnexo1 = new List<SendGrid.Helpers.Mail.Attachment>();
                    if (vm.MENSAGEM_ANEXO.Count > 0)
                    {
                        foreach (MENSAGEM_ANEXO item in vm.MENSAGEM_ANEXO)
                        {
                            SendGrid.Helpers.Mail.Attachment anexo = new SendGrid.Helpers.Mail.Attachment();
                            anexo.Filename = Server.MapPath(item.MEAN_AQ_ARQUIVO);
                            listaAnexo1.Add(anexo);
                        }
                    }

                    // Monta e-mail
                    NetworkCredential net = new NetworkCredential(conf.CONF_NM_EMAIL_EMISSOO, conf.CONF_NM_SENHA_EMISSOR);
                    Email mensagem = new Email();
                    mensagem.ASSUNTO = vm.MENS_NM_CAMPANHA != null ? vm.MENS_NM_CAMPANHA : "Assunto Diverso";
                    mensagem.CORPO = emailBody;
                    mensagem.DEFAULT_CREDENTIALS = false;
                    mensagem.EMAIL_DESTINO = cliente.CLIE_NM_EMAIL;
                    mensagem.EMAIL_EMISSOR = conf.CONF_NM_EMAIL_EMISSOO;
                    mensagem.ENABLE_SSL = true;
                    mensagem.NOME_EMISSOR = cliente.ASSINANTE.ASSI_NM_NOME;
                    mensagem.PORTA = conf.CONF_NM_PORTA_SMTP;
                    mensagem.PRIORIDADE = System.Net.Mail.MailPriority.High;
                    mensagem.SENHA_EMISSOR = conf.CONF_NM_SENHA_EMISSOR;
                    mensagem.SMTP = conf.CONF_NM_HOST_SMTP;
                    mensagem.NETWORK_CREDENTIAL = net;
                    mensagem.ATTACHMENT = listaAnexo;

                    // Envia mensagem
                    try
                    {
                        Int32 voltaMail = CommunicationPackage.SendEmail(mensagem);
                    }
                    catch (Exception ex)
                    {
                        erro = ex.Message;
                        if (ex.InnerException != null)
                        {
                            erro += ex.InnerException.Message;
                        }
                        if (ex.GetType() == typeof(SmtpFailedRecipientException))
                        {
                            var se = (SmtpFailedRecipientException)ex;
                            erro += se.FailedRecipient;
                        }
                    }

                    // Grava mensagem/destino e erros
                    if (erro == null)
                    {
                        MENSAGENS_DESTINOS dest = new MENSAGENS_DESTINOS();
                        dest.MEDE_IN_ATIVO = 1;
                        dest.MEDE_IN_POSICAO = 1;
                        dest.MEDE_IN_STATUS = 1;
                        dest.CLIE_CD_ID = cliente.CLIE_CD_ID;
                        dest.MEDE_DS_ERRO_ENVIO = erro;
                        dest.MENS_CD_ID = mens.MENS_CD_ID;
                        mens.MENSAGENS_DESTINOS.Add(dest);
                        mens.MENS_DT_ENVIO = DateTime.Now;
                        volta = baseApp.ValidateEdit(mens, mens);
                    }
                    else
                    {
                        mens.MENS_TX_RETORNO = erro;
                        volta = baseApp.ValidateEdit(mens, mens);
                    }

                    // Envia pelo sendgrid
                    String assunto = vm.MENS_NM_CAMPANHA != null ? vm.MENS_NM_CAMPANHA : "Assunto Diverso";
                    //EnviarSendGrid(conf.CONF_NM_EMAIL_EMISSOO, assunto, cliente.CLIE_NM_EMAIL, cliente.CLIE_NM_NOME, emailBody, listaAnexo1).Wait();

                    erro = null;
                    return volta;
                }
                else
                {
                    foreach (CLIENTE item in listaCli)
                    {
                        // Prepara cabeçalho
                        String cab = vm.MENS_NM_CABECALHO.Replace("{Nome}", item.CLIE_NM_NOME);

                        // Prepara rodape
                        ASSINANTE assi = (ASSINANTE)Session["Assinante"];
                        String rod = vm.MENS_NM_RODAPE;

                        // Prepara corpo do e-mail e trata link
                        StringBuilder str = new StringBuilder();
                        str.AppendLine(vm.MENS_TX_TEXTO);
                        if (!String.IsNullOrEmpty(vm.MENS_NM_LINK))
                        {
                            if (!vm.MENS_NM_LINK.Contains("www."))
                            {
                                vm.MENS_NM_LINK = "www." + vm.MENS_NM_LINK;
                            }
                            if (!vm.MENS_NM_LINK.Contains("http://"))
                            {
                                vm.MENS_NM_LINK = "http://" + vm.MENS_NM_LINK;
                            }
                            str.AppendLine("<a href='" + vm.MENS_NM_LINK + "'>Clique aqui para maiores informações</a>");
                        }
                        String body = str.ToString();
                        String emailBody = cab + body + rod;

                        // Checa e monta anexos
                        List<System.Net.Mail.Attachment> listaAnexo = new List<System.Net.Mail.Attachment>();
                        if (vm.MENSAGEM_ANEXO.Count > 0)
                        {
                            foreach (MENSAGEM_ANEXO ane in vm.MENSAGEM_ANEXO)
                            {
                                System.Net.Mail.Attachment anexo = new System.Net.Mail.Attachment(Server.MapPath(ane.MEAN_AQ_ARQUIVO));
                                listaAnexo.Add(anexo);
                            }
                        }

                        // Monta e-mail
                        NetworkCredential net = new NetworkCredential(conf.CONF_NM_EMAIL_EMISSOO, conf.CONF_NM_SENHA_EMISSOR);
                        Email mensagem = new Email();
                        mensagem.ASSUNTO = vm.MENS_NM_CAMPANHA != null ? vm.MENS_NM_CAMPANHA : "Assunto Diverso";
                        mensagem.CORPO = emailBody;
                        mensagem.DEFAULT_CREDENTIALS = false;
                        mensagem.EMAIL_DESTINO = item.CLIE_NM_EMAIL;
                        mensagem.EMAIL_EMISSOR = conf.CONF_NM_EMAIL_EMISSOO;
                        mensagem.ENABLE_SSL = true;
                        mensagem.NOME_EMISSOR = item.ASSINANTE.ASSI_NM_NOME;
                        mensagem.PORTA = conf.CONF_NM_PORTA_SMTP;
                        mensagem.PRIORIDADE = System.Net.Mail.MailPriority.High;
                        mensagem.SENHA_EMISSOR = conf.CONF_NM_SENHA_EMISSOR;
                        mensagem.SMTP = conf.CONF_NM_HOST_SMTP;
                        mensagem.NETWORK_CREDENTIAL = net;
                        mensagem.ATTACHMENT = listaAnexo;

                        // Envia mensagem
                        try
                        {
                            Int32 voltaMail = CommunicationPackage.SendEmail(mensagem);
                        }
                        catch (Exception ex)
                        {
                            erro = ex.Message;
                            if (ex.GetType() == typeof(SmtpFailedRecipientException))
                            {
                                var se = (SmtpFailedRecipientException)ex;
                                erro += se.FailedRecipient;
                            }
                        }

                        // Grava mensagem/destino e erros
                        if (erro == null)
                        {
                            MENSAGENS_DESTINOS dest = new MENSAGENS_DESTINOS();
                            dest.MEDE_IN_ATIVO = 1;
                            dest.MEDE_IN_POSICAO = 1;
                            dest.MEDE_IN_STATUS = 1;
                            dest.CLIE_CD_ID = item.CLIE_CD_ID;
                            dest.MEDE_DS_ERRO_ENVIO = erro;
                            dest.MENS_CD_ID = mens.MENS_CD_ID;
                            mens.MENSAGENS_DESTINOS.Add(dest);
                            mens.MENS_DT_ENVIO = DateTime.Now;
                            volta = baseApp.ValidateEdit(mens, mens);
                        }
                        else
                        {
                            mens.MENS_TX_RETORNO = erro;
                            volta = baseApp.ValidateEdit(mens, mens);
                        }
                        erro = null;
                    }
                    return volta;
                }
            }

            // Processa SMS
            if (vm.MENS_IN_TIPO == 2)
            {
                CONFIGURACAO conf = confApp.GetItemById(usuario.ASSI_CD_ID);

                // Monta token
                String text = conf.CONF_SG_LOGIN_SMS + ":" + conf.CONF_SG_SENHA_SMS;
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                String token = Convert.ToBase64String(textBytes);
                String auth = "Basic " + token;

                // Prepara texto
                String texto = vm.MENS_TX_SMS;

                // inicia processo
                String resposta = String.Empty;

                // Monta destinatarios
                if (escopo == 1)
                {
                    try
                    {
                        String listaDest = "55" + Regex.Replace(cliente.CLIE_NR_CELULAR, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled).ToString();
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api-v2.smsfire.com.br/sms/send/bulk");
                        httpWebRequest.Headers["Authorization"] = auth;
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        String customId = Cryptography.GenerateRandomPassword(8);

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            string json = String.Concat("{\"destinations\": [{\"to\": \"", listaDest, "\", \"text\": \"", texto, "\", \"" + customId + "\": \"yyyy\", \"from\": \"SystemBR\"}]}");
                            streamWriter.Write(json);
                        }

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            resposta = result;
                        }
                    }
                    catch (Exception ex)
                    {
                        erro = ex.Message;
                    }

                    // Grava mensagem/destino e erros
                    if (erro == null)
                    {
                        MENSAGENS_DESTINOS dest = new MENSAGENS_DESTINOS();
                        dest.MEDE_IN_ATIVO = 1;
                        dest.MEDE_IN_POSICAO = 1;
                        dest.MEDE_IN_STATUS = 1;
                        dest.CLIE_CD_ID = cliente.CLIE_CD_ID;
                        dest.MEDE_DS_ERRO_ENVIO = resposta;
                        dest.MENS_CD_ID = mens.MENS_CD_ID;
                        mens.MENSAGENS_DESTINOS.Add(dest);
                        mens.MENS_DT_ENVIO = DateTime.Now;
                        volta = baseApp.ValidateEdit(mens, mens);
                    }
                    else
                    {
                        mens.MENS_TX_RETORNO = erro;
                        volta = baseApp.ValidateEdit(mens, mens);
                    }
                    erro = null;
                    return volta;
                }
                else
                {
                    try
                    {
                        String listaDest = "55" + Regex.Replace(cliente.CLIE_NR_CELULAR, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled).ToString();
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api-v2.smsfire.com.br/sms/send/bulk");
                        httpWebRequest.Headers["Authorization"] = auth;
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            string json = String.Concat("{\"destinations\": [{\"to\": \"", listaDest, "\", \"text\": \"", texto, "\", \"customId\": \"zzzz\", \"from\": \"PlatMensagens\"}]}");

                            streamWriter.Write(json);
                        }

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            resposta = result;
                        }
                    }
                    catch (Exception ex)
                    {
                        erro = ex.Message;
                    }

                    // Grava mensagem/destino e erros
                    if (erro == null)
                    {
                        MENSAGENS_DESTINOS dest = new MENSAGENS_DESTINOS();
                        dest.MEDE_IN_ATIVO = 1;
                        dest.MEDE_IN_POSICAO = 1;
                        dest.MEDE_IN_STATUS = 1;
                        dest.CLIE_CD_ID = cliente.CLIE_CD_ID;
                        dest.MEDE_DS_ERRO_ENVIO = resposta;
                        dest.MENS_CD_ID = mens.MENS_CD_ID;
                        mens.MENSAGENS_DESTINOS.Add(dest);
                        mens.MENS_DT_ENVIO = DateTime.Now;
                        volta = baseApp.ValidateEdit(mens, mens);
                    }
                    else
                    {
                        mens.MENS_TX_RETORNO = erro;
                        volta = baseApp.ValidateEdit(mens, mens);
                    }
                    erro = null;
                    return volta;
                }
            }
            return 0;
        }

        public static async Task EnviarSendGrid(String mailFrom, String assunto, String mailTo, String nome, String texto, List<SendGrid.Helpers.Mail.Attachment> anexo)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(mailFrom, "RTI");
            var subject = assunto;
            var to = new EmailAddress(mailTo, nome);
            var plainTextContent = "-";
            var htmlContent = texto;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.AddAttachments(anexo);
            var response = await client.SendEmailAsync(msg);
        }

        [HttpGet]
        public ActionResult VerMensagem(Int32 id)
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
                    Session["MensMensagem"] = 2;
                    return RedirectToAction("MontarTelaMensagem", "Mensagem");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            Session["IdMensagem"] = id;
            MENSAGENS item = baseApp.GetItemById(id);
            MensagemViewModel vm = Mapper.Map<MENSAGENS, MensagemViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult ExcluirMensagem(Int32 id)
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
                    Session["MensMensagem"] = 2;
                    return RedirectToAction("VoltarBaseMensagem");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            MENSAGENS item = baseApp.GetItemById(id);
            item.MENS_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            Session["ListaMensagem"] = null;
            return RedirectToAction("VoltarBaseMensagem");
        }

        [HttpGet]
        public ActionResult ReativarMensagem(Int32 id)
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
                    Session["MensMensagem"] = 2;
                    return RedirectToAction("VoltarBaseMensagem");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            MENSAGENS item = baseApp.GetItemById(id);
            item.MENS_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            Session["ListaMensagem"] = null;
            return RedirectToAction("VoltarBaseMensagem");
        }

        public ActionResult GerarRelatorioLista()
        {            
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "MensagemLista" + "_" + data + ".pdf";
            List<MENSAGENS> lista = (List<MENSAGENS>)Session["ListaMensagem"];
            MENSAGENS filtro = (MENSAGENS)Session["FiltroMensagem"];
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

            cell = new PdfPCell(new Paragraph("Mensagens - Listagem", meuFont2))
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
            table = new PdfPTable(new float[] { 70f, 70f, 160f, 70f, 70f});
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Mensagens selecionadas pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 5;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Data Criação", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Data Envio", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Campanha", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Tipo", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Num.Envios", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (MENSAGENS item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.MENS_DT_CRIACAO.Value.ToShortDateString() + " " + item.MENS_DT_CRIACAO.Value.ToShortTimeString(), meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.MENS_DT_ENVIO.Value.ToShortDateString() + " " + item.MENS_DT_ENVIO.Value.ToShortTimeString(), meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.MENS_NM_CAMPANHA, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.MENS_IN_TIPO == 1)
                {
                    cell = new PdfPCell(new Paragraph("E-Mail", meuFontO))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.MENS_IN_TIPO == 2)
                {
                    cell = new PdfPCell(new Paragraph("SMS", meuFontO))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                else if (item.MENS_IN_TIPO == 3)
                {
                    cell = new PdfPCell(new Paragraph("WhatsApp", meuFontP))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                }
                cell = new PdfPCell(new Paragraph(item.MENSAGENS_DESTINOS.Count.ToString(), meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
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
                if (filtro.MENS_IN_TIPO > 0)
                {
                    parametros += "Tipo: " + filtro.MENS_IN_TIPO;
                    ja = 1;
                }
                if (filtro.MENS_NM_CAMPANHA != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Campanha: " + filtro.MENS_NM_CAMPANHA;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Campanha: " + filtro.MENS_NM_CAMPANHA;
                    }
                }
                if (filtro.MENS_TX_TEXTO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Texto: " + filtro.MENS_TX_TEXTO;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Texto: " + filtro.MENS_TX_TEXTO;
                    }
                }
                if (filtro.MENS_DT_CRIACAO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Criação: " + filtro.MENS_DT_CRIACAO.Value.ToShortDateString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Criação: " + filtro.MENS_DT_CRIACAO.Value.ToShortDateString();
                    }
                }
                if (filtro.MENS_DT_ENVIO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Envio: " + filtro.MENS_DT_ENVIO.Value.ToShortDateString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Envio: " + filtro.MENS_DT_ENVIO.Value.ToShortDateString();
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

            return RedirectToAction("MontarTelaMensagem");
        }

        [HttpPost]
        public JsonResult PesquisaTemplate(String temp)
        {
            // Recupera Template
            TEMPLATE tmp = temApp.GetItemById(Convert.ToInt32(temp));

            // Atualiza
            var hash = new Hashtable();
            hash.Add("TEMP_IN_TIPO", tmp.TEMP_IN_TIPO);
            hash.Add("TEMP_TX_CABECALHO", tmp.TEMP_TX_CABECALHO);
            hash.Add("TEMP_TX_CORPO", tmp.TEMP_TX_CORPO);
            hash.Add("TEMP_TX_DADOS", tmp.TEMP_TX_DADOS);

            // Retorna
            Session["VoltaCEP"] = 2;
            return Json(hash);
        }

    }
}