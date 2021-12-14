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
    public class AssinanteController : Controller
    {
        private readonly IAssinanteAppService baseApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IConfiguracaoAppService confApp;
        private readonly IAssinanteCnpjAppService ccnpjApp;

        private String msg;
        private Exception exception;
        ASSINANTE objeto = new ASSINANTE();
        ASSINANTE objetoAntes = new ASSINANTE();
        List<ASSINANTE> listaMaster = new List<ASSINANTE>();
        String extensao;

        public AssinanteController(IAssinanteAppService baseApps, IUsuarioAppService usuApps, IConfiguracaoAppService confApps, IAssinanteCnpjAppService ccnpjApps)
        {
            baseApp = baseApps;
            usuApp = usuApps;
            confApp = confApps;
            ccnpjApp = ccnpjApps;
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

        public ActionResult EnviarSmsAssinante(Int32 id, String mensagem)
        {
            try
            {
                ASSINANTE clie = baseApp.GetById(id);

                // Verifica existencia prévia
                if (clie == null)
                {
                    Session["MensSMSAssi"] = 1;
                    return RedirectToAction("MontarTelaAssinante");
                }

                // Criticas
                if (clie.ASSIN_NR_CELULAR == null)
                {
                    Session["MensSMSAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante");
                }

                // Monta token
                Int32 idAss = (Int32)Session["IdAssinante"];
                CONFIGURACAO conf = confApp.GetItemById(idAss);
                String text = conf.CONF_SG_LOGIN_SMS + ":" + conf.CONF_SG_SENHA_SMS;
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                String token = Convert.ToBase64String(textBytes);
                String auth = "Basic " + token;
                String erro = String.Empty;

                // Prepara texto
                String texto = String.Empty;

                // inicia processo
                String resposta = String.Empty;

                try
                {
                    String dest = "55" + Regex.Replace(clie.ASSIN_NR_CELULAR, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled).ToString();
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api-v2.smsfire.com.br/sms/send/bulk");
                    httpWebRequest.Headers["Authorization"] = auth;
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = String.Concat("{\"destinations\": [{\"to\": \"", dest, "\", \"text\": \"", texto, "\", \"customId\": \"sysbr\", \"from\": \"SystemBR\"}]}");

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

                return RedirectToAction("MontarTelaCliente");
            }
            catch (Exception ex)
            {
                Session["MensSMSAssi"] = 3;
                Session["MensSMSAssiErro"] = ex.Message;
                return RedirectToAction("MontarTelaAssinante");
            }
        }

        [HttpPost]
        public JsonResult BuscaNomeRazao(String nome)
        {
            Int32 isRazao = 0;
            Int32 idAss = (Int32)Session["IdAssinante"];
            List<Hashtable> listResult = new List<Hashtable>();

            List<ASSINANTE> clientes = baseApp.GetAllItens();

            if (nome != null)
            {
                List<ASSINANTE> lstCliente = clientes.Where(x => x.ASSI_NM_NOME != null && x.ASSI_NM_NOME.ToLower().Contains(nome.ToLower())).ToList<ASSINANTE>();

                if (lstCliente == null || lstCliente.Count == 0)
                {
                    isRazao = 1;
                    lstCliente = clientes.Where(x => x.ASSI_NM_RAZAO_SOCIAL != null).ToList<ASSINANTE>();
                    lstCliente = lstCliente.Where(x => x.ASSI_NM_RAZAO_SOCIAL.ToLower().Contains(nome.ToLower())).ToList<ASSINANTE>();
                }

                if (lstCliente != null)
                {
                    foreach (var item in lstCliente)
                    {
                        Hashtable result = new Hashtable();
                        result.Add("id", item.ASSI_CD_ID);
                        if (isRazao == 0)
                        {
                            result.Add("text", item.ASSI_NM_NOME);
                        }
                        else
                        {
                            result.Add("text", item.ASSI_NM_NOME + " (" + item.ASSI_NM_RAZAO_SOCIAL + ")");
                        }
                        listResult.Add(result);
                    }
                }
            }
            return Json(listResult);
        }

        [HttpPost]
        public JsonResult PesquisaCNPJ(string cnpj)
        {
            List<ASSINANTE_QUADRO_SOCIETARIO> lstQs = new List<ASSINANTE_QUADRO_SOCIETARIO>();

            var url = "https://api.cnpja.com.br/companies/" + Regex.Replace(cnpj, "[^0-9]", "");
            String json = String.Empty;
            WebRequest request = WebRequest.Create(url);
            request.Headers["Authorization"] = "df3c411d-bb44-41eb-9304-871c45d72978-cd751b62-ff3d-4421-a9d2-b97e01ca6d2b";

            try
            {
                WebResponse response = request.GetResponse();
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }

                var jObject = JObject.Parse(json);
                if (jObject["membership"].Count() == 0)
                {
                    ASSINANTE_QUADRO_SOCIETARIO qs = new ASSINANTE_QUADRO_SOCIETARIO();

                    qs.ASSINANTE = new ASSINANTE();
                    qs.ASSINANTE.ASSI_NM_RAZAO_SOCIAL = jObject["name"] == null ? String.Empty : jObject["name"].ToString();
                    qs.ASSINANTE.ASSI_NM_NOME = jObject["alias"] == null ? jObject["name"].ToString() : jObject["alias"].ToString();
                    qs.ASSINANTE.ASSI_NR_CEP = jObject["address"]["zip"].ToString();
                    qs.ASSINANTE.ASSI_NM_ENDERECO = jObject["address"]["street"].ToString();
                    qs.ASSINANTE.ASSI_NR_NUMERO = jObject["address"]["number"].ToString();
                    qs.ASSINANTE.ASSI_NM_BAIRRO = jObject["address"]["neighborhood"].ToString();
                    qs.ASSINANTE.ASSI_NM_CIDADE = jObject["address"]["city"].ToString();
                    qs.ASSINANTE.UF_CD_ID = ((List<UF>)Session["UFs"]).Where(x => x.UF_SG_SIGLA == jObject["address"]["state"].ToString()).Select(x => x.UF_CD_ID).FirstOrDefault();
                    
                    qs.ASSINANTE.ASSI_NR_TELEFONE = jObject["phone"].ToString();
                    qs.ASSINANTE.ASSI_NM_EMAIL = jObject["email"].ToString();
                    qs.ASQS_IN_ATIVO = 0;
                    lstQs.Add(qs);
                }
                else
                {
                    foreach (var s in jObject["membership"])
                    {
                        ASSINANTE_QUADRO_SOCIETARIO qs = new ASSINANTE_QUADRO_SOCIETARIO();

                        qs.ASSINANTE = new ASSINANTE();
                        qs.ASSINANTE.ASSI_NM_RAZAO_SOCIAL = jObject["name"].ToString() == "" ? String.Empty : jObject["name"].ToString();
                        qs.ASSINANTE.ASSI_NM_NOME = jObject["alias"].ToString() == "" ? jObject["name"].ToString() : jObject["alias"].ToString();
                        qs.ASSINANTE.ASSI_NR_CEP = jObject["address"]["zip"].ToString();
                        qs.ASSINANTE.ASSI_NM_ENDERECO = jObject["address"]["street"].ToString();
                        qs.ASSINANTE.ASSI_NR_NUMERO = jObject["address"]["number"].ToString();
                        qs.ASSINANTE.ASSI_NM_BAIRRO = jObject["address"]["neighborhood"].ToString();
                        qs.ASSINANTE.ASSI_NM_CIDADE = jObject["address"]["city"].ToString();
                        qs.ASSINANTE.UF_CD_ID = ((List<UF>)Session["UFs"]).Where(x => x.UF_SG_SIGLA == jObject["address"]["state"].ToString()).Select(x => x.UF_CD_ID).FirstOrDefault();
                        qs.ASSINANTE.ASSI_NR_TELEFONE = jObject["phone"].ToString();
                        qs.ASSINANTE.ASSI_NM_EMAIL = jObject["email"].ToString();
                        qs.ASQS_NM_QUALIFICACAO = s["role"]["description"].ToString();
                        qs.ASQS_NM_NOME = s["name"].ToString();

                        // CNPJá não retorna esses valores
                        qs.ASQS_NM_PAIS_ORIGEM = String.Empty;
                        qs.ASQS_NM_REPRESENTANTE_LEGAL = String.Empty;
                        qs.ASQS_NM_QUALIFICACAO_REP_LEGAL = String.Empty;
                        lstQs.Add(qs);
                    }
                }
                return Json(lstQs);
            }
            catch (WebException ex)
            {
                var hash = new Hashtable();
                hash.Add("status", "ERROR");

                if ((ex.Response as HttpWebResponse)?.StatusCode.ToString() == "BadRequest")
                {
                    hash.Add("public", 1);
                    hash.Add("message", "CNPJ inválido");
                    return Json(hash);
                }
                if ((ex.Response as HttpWebResponse)?.StatusCode.ToString() == "NotFound")
                {
                    hash.Add("public", 1);
                    hash.Add("message", "O CNPJ consultado não está registrado na Receita Federal");
                    return Json(hash);
                }
                else
                {
                    hash.Add("public", 1);
                    hash.Add("message", ex.Message);
                    return Json(hash);
                }
            }
        }

        private List<ASSINANTE_QUADRO_SOCIETARIO> PesquisaCNPJ(ASSINANTE cliente)
        {
            List<ASSINANTE_QUADRO_SOCIETARIO> lstQs = new List<ASSINANTE_QUADRO_SOCIETARIO>();

            var url = "https://api.cnpja.com.br/companies/" + Regex.Replace(cliente.ASSI_NR_CNPJ, "[^0-9]", "");
            String json = String.Empty;

            WebRequest request = WebRequest.Create(url);
            request.Headers["Authorization"] = "df3c411d-bb44-41eb-9304-871c45d72978-cd751b62-ff3d-4421-a9d2-b97e01ca6d2b";

            WebResponse response = request.GetResponse();

            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.UTF8))
            {
                json = reader.ReadToEnd();
            }

            var jObject = JObject.Parse(json);

            foreach (var s in jObject["membership"])
            {
                ASSINANTE_QUADRO_SOCIETARIO qs = new ASSINANTE_QUADRO_SOCIETARIO();

                qs.ASQS_NM_QUALIFICACAO = s["role"]["description"].ToString();
                qs.ASQS_NM_NOME = s["name"].ToString();
                qs.ASSI_CD_ID = cliente.ASSI_CD_ID;

                // CNPJá não retorna esses valores
                qs.ASQS_NM_PAIS_ORIGEM = String.Empty;
                qs.ASQS_NM_REPRESENTANTE_LEGAL = String.Empty;
                qs.ASQS_NM_QUALIFICACAO_REP_LEGAL = String.Empty;
                lstQs.Add(qs);
            }
            return lstQs;
        }

        [HttpGet]
        public ActionResult MontarTelaAssinante()
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<ASSINANTE>)Session["ListaAssi"] == null)
            {
                listaMaster = baseApp.GetAllItens();
                Session["ListaAssi"] = listaMaster;
            }
            ViewBag.Listas = (List<ASSINANTE>)Session["ListaAssi"];
            ViewBag.Title = "Assinantes";
            Session["Assinante"] = null;

            // Indicadores
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            if (Session["MensAssi"] != null)
            {
                if ((Int32)Session["MensAssi"] == 3)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0057", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensAssi"] == 50)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0052", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            Session["VoltaAssi"] = 1;
            objeto = new ASSINANTE();
            if (Session["FiltroAssi"] != null)
            {
                objeto = (ASSINANTE)Session["FiltroAssi"];
            }
            return View(objeto);
        }

        public ActionResult RetirarFiltroAssinante()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaAssi"] = null;
            Session["FiltroAssi"] = null;
            if ((Int32)Session["VoltaAssi"] == 2)
            {
                return RedirectToAction("VerCardsAssinante");
            }
            return RedirectToAction("MontarTelaAssinante");
        }

        public ActionResult MostrarTudoAssinante()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm();
            Session["ListaAssi"] = null;
            Session["FiltroAssi"] = listaMaster;
            if ((Int32)Session["VoltaAssi"] == 2)
            {
                return RedirectToAction("VerCardsAssinante");
            }
            return RedirectToAction("MontarTelaAssinante");
        }

        [HttpPost]
        public ActionResult FiltrarAssinante(ASSINANTE item)
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                List<ASSINANTE> listaObj = new List<ASSINANTE>();
                Session["FiltroAssi"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.TIPE_CD_ID.Value, item.ASSI_NM_NOME, item.ASSI_NR_CPF, item.ASSI_NR_CNPJ, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensAssi"] = 1;
                    return RedirectToAction("MontarTelaAssinante");
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaAssi"] = listaObj;
                if ((Int32)Session["VoltaAssi"] == 2)
                {
                    return RedirectToAction("VerCardsAssinante");
                }
                return RedirectToAction("MontarTelaAssinante");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaAssinante");
            }
        }

        public ActionResult VoltarBaseAssinante()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((Int32)Session["VoltaAssi"] == 2)
            {
                return RedirectToAction("VerCardsAssinante");
            }
            return RedirectToAction("MontarTelaAssinante");
        }

        [HttpGet]
        public ActionResult IncluirAssinante()
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Planos = new SelectList(baseApp.GetAllPlanos().OrderBy(p => p.PLAN_NM_NOME), "PLAN_CD_ID", "PLAN_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF().OrderBy(p => p.UF_SG_SIGLA), "UF_CD_ID", "UF_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(baseApp.GetAllTiposPessoa().OrderBy(p => p.TIPE_NM_NOME), "TIPE_CD_ID", "TIPE_NM_NOME");

            // Prepara view
            ASSINANTE item = new ASSINANTE();
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            vm.ASSI_DT_INICIO = DateTime.Today.Date;
            vm.ASSI_IN_ATIVO = 1;
            vm.ASSI_IN_STATUS = 1;
            vm.ASSI_IN_TIPO = 1;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirAssinante(AssinanteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ViewBag.Planos = new SelectList(baseApp.GetAllPlanos().OrderBy(p => p.PLAN_NM_NOME), "PLAN_CD_ID", "PLAN_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF().OrderBy(p => p.UF_SG_SIGLA), "UF_CD_ID", "UF_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(baseApp.GetAllTiposPessoa().OrderBy(p => p.TIPE_NM_NOME), "TIPE_CD_ID", "TIPE_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensAssi"] = 3;
                        return RedirectToAction("MontarTelaAssinante");
                    }

                    // Cria pastas
                    String caminho = "/Imagens/Assinante/" + item.ASSI_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<ASSINANTE>();
                    Session["ListaAssi"] = null;
                    Session["IncluirAssi"] = 1;
                    Session["AssiNovo"] = item.ASSI_CD_ID;

                    if (item.TIPE_CD_ID == 2)
                    {
                        var lstQs = PesquisaCNPJ(item);

                        foreach (var qs in lstQs)
                        {
                            Int32 voltaQs = ccnpjApp.ValidateCreate(qs, usuario);
                        }
                    }

                    Session["IdAssi"] = item.ASSI_CD_ID;
                    if (Session["FileQueueAssinante"] != null)
                    {
                        List<FileQueue> fq = (List<FileQueue>)Session["FileQueueAssinante"];

                        foreach (var file in fq)
                        {
                            if (file.Profile == null)
                            {
                                UploadFileQueueAssinante(file);
                            }
                        }
                        Session["FileQueueAssinante"] = null;
                    }

                    return RedirectToAction("MontarTelaAssinante");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                vm.TIPE_CD_ID = 0;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarAssinante(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS" || usuario.PERFIL.PERF_SG_SIGLA == "OPR")
                {
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ViewBag.Planos = new SelectList(baseApp.GetAllPlanos().OrderBy(p => p.PLAN_NM_NOME), "PLAN_CD_ID", "PLAN_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF().OrderBy(p => p.UF_SG_SIGLA), "UF_CD_ID", "UF_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(baseApp.GetAllTiposPessoa().OrderBy(p => p.TIPE_NM_NOME), "TIPE_CD_ID", "TIPE_NM_NOME");

            ASSINANTE item = baseApp.GetItemById(id);
            Session["Assinante"] = item;
            ViewBag.QuadroSoci = ccnpjApp.GetByCliente(item);

            // Indicadores
            ViewBag.Incluir = (Int32)Session["IncluirAssi"];

            // Mensagens
            if (Session["MensAssi"] != null)
            {
                if ((Int32)Session["MensAssi"] == 10)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensAssi"] == 11)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensAssi"] == 40)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0034", CultureInfo.CurrentCulture));
                }
            }

            Session["VoltaAssi"] = 1;
            objetoAntes = item;
            Session["IdAssi"] = id;
            Session["VoltaCep"] = 1;
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarAssinante(AssinanteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ViewBag.Planos = new SelectList(baseApp.GetAllPlanos().OrderBy(p => p.PLAN_NM_NOME), "PLAN_CD_ID", "PLAN_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF().OrderBy(p => p.UF_SG_SIGLA), "UF_CD_ID", "UF_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(baseApp.GetAllTiposPessoa().OrderBy(p => p.TIPE_NM_NOME), "TIPE_CD_ID", "TIPE_NM_NOME");
            ASSINANTE clie = baseApp.GetItemById(vm.ASSI_CD_ID);

            // Indicadores
            ViewBag.Incluir = (Int32)Session["IncluirAssi"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuario);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<ASSINANTE>();
                    Session["ListaAssi"] = null;
                    Session["IncluirAssi"] = 0;

                    if (Session["FiltroAssi"] != null)
                    {
                        FiltrarAssinante((ASSINANTE)Session["FiltroAssi"]);
                    }

                    if ((Int32)Session["VoltaAssi"] == 2)
                    {
                        return RedirectToAction("VerCardsAssinante");
                    }
                    return RedirectToAction("MontarTelaAssinante");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(clie);
                    return View(vm);
                }
            }
            else
            {
                vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(clie);
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult VerAssinante(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            Session["IdAssi"] = id;
            ASSINANTE item = baseApp.GetItemById(id);
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult ExcluirAssinante(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ASSINANTE item = baseApp.GetItemById(id);
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirAssinante(AssinanteViewModel vm)
        {
            try
            {
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }

                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<ASSINANTE>();
                Session["ListaAssi"] = null;
                return RedirectToAction("MontarTelaAssinante");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult ReativarAssinante(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ASSINANTE item = baseApp.GetItemById(id);
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarAssinante(AssinanteViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<ASSINANTE>();
                Session["ListaAssi"] = null;
                return RedirectToAction("MontarTelaAssinante");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        public ActionResult VerCardsAssinante()
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if (Session["ListaAssi"] == null)
            {
                listaMaster = baseApp.GetAllItens();
                Session["ListaAssi"] = listaMaster;
            }
            ViewBag.Listas = (List<ASSINANTE>)Session["ListaAssi"];
            ViewBag.Title = "Assinantes";

            // Abre view
            Session["VoltaAssi"] = 2;
            objeto = new ASSINANTE();
            return View(objeto);
        }

        [HttpGet]
        public ActionResult VerAnexoAssinante(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("MontarTelaAssinante", "Assinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ASSINANTE_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoAssinante()
        {

            return RedirectToAction("EditarAssinante", new { id = (Int32)Session["IdAssi"] });
        }

        public FileResult DownloadAssinante(Int32 id)
        {
            ASSINANTE_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.ASAN_AQ_ARQUIVO;
            Int32 pos = arquivo.LastIndexOf("/") + 1;
            String nomeDownload = arquivo.Substring(pos);
            String contentType = string.Empty;
            if (arquivo.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }
            else if (arquivo.Contains(".jpg"))
            {
                contentType = "image/jpg";
            }
            else if (arquivo.Contains(".png"))
            {
                contentType = "image/png";
            }
            return File(arquivo, contentType, nomeDownload);
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
            Session["FileQueueAssinante"] = queue;
        }

        [HttpPost]
        public ActionResult UploadFileQueueAssinante(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdAssi"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensAssi"] = 10;
                return RedirectToAction("VoltarAnexoAssinante");
            }

            ASSINANTE item = baseApp.GetItemById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensAssi"] = 11;
                return RedirectToAction("VoltarAnexoAssinante");
            }
            String caminho = "/Imagens/Assinante/" + item.ASSI_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            ASSINANTE_ANEXO foto = new ASSINANTE_ANEXO();
            foto.ASAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.ASAN_DT_ANEXO = DateTime.Today;
            foto.ASAN_IN_ATIVO = 1;
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
            foto.ASAN_IN_TIPO = tipo;
            foto.ASAN_NM_TITULO = fileName;
            foto.ASSI_CD_ID = item.ASSI_CD_ID;

            item.ASSINANTE_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item, usu);
            return RedirectToAction("VoltarAnexoAssinante");
        }

        [HttpPost]
        public ActionResult UploadFileAssinante(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdAssi"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                Session["MensAssi"] = 10;
                return RedirectToAction("VoltarAnexoAssinante");
            }

            ASSINANTE item = baseApp.GetItemById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                Session["MensAssi"] = 11;
                return RedirectToAction("VoltarAnexoAssinante");
            }
            String caminho = "/Imagens/Assinante/" + item.ASSI_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.Directory.CreateDirectory(Server.MapPath(caminho));
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            ASSINANTE_ANEXO foto = new ASSINANTE_ANEXO();
            foto.ASAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.ASAN_DT_ANEXO = DateTime.Today;
            foto.ASAN_IN_ATIVO = 1;
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
            foto.ASAN_IN_TIPO = tipo;
            foto.ASAN_NM_TITULO = fileName;
            foto.ASSI_CD_ID = item.ASSI_CD_ID;

            item.ASSINANTE_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, item, usu);
            return RedirectToAction("VoltarAnexoAssinante");
        }

        [HttpPost]
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            ASSINANTE cli = (ASSINANTE)Session["Assinante"];

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(cep);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza
            var hash = new Hashtable();

            if (tipoEnd == 1)
            {
                hash.Add("ASSI_NM_ENDERECO", end.Address);
                hash.Add("ASSI_NR_NUMERO", end.Complement);
                hash.Add("ASSI_NM_BAIRRO", end.District);
                hash.Add("ASSI_NM_CIDADE", end.City);
                hash.Add("UF_CD_ID", baseApp.GetUFBySigla(end.Uf).UF_CD_ID);
                hash.Add("ASSI_NR_CEP", cep);
            }

            // Retorna
            Session["VoltaCEP"] = 2;
            return Json(hash);
        }

        [HttpGet]
        public ActionResult EditarPagamento(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("VoltarAnexoAssinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ASSINANTE_PAGAMENTO item = baseApp.GetPagtoById(id);
            objetoAntes = (ASSINANTE)Session["Assinante"];
            AssinantePagamentoViewModel vm = Mapper.Map<ASSINANTE_PAGAMENTO, AssinantePagamentoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPagamento(AssinantePagamentoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    ASSINANTE_PAGAMENTO item = Mapper.Map<AssinantePagamentoViewModel, ASSINANTE_PAGAMENTO>(vm);
                    Int32 volta = baseApp.ValidateEditPagto(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoAssinante");
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

        [HttpGet]
        public ActionResult ExcluirPagamento(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("VoltarAnexoAssinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ASSINANTE_PAGAMENTO item = baseApp.GetPagtoById(id);
            objetoAntes = (ASSINANTE)Session["Assinante"];
            item.ASPA_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateEditPagto(item);
            return RedirectToAction("VoltarAnexoAssinante");
        }

        [HttpGet]
        public ActionResult ReativarPagamento(Int32 id)
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("VoltarAnexoAssinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            ASSINANTE_PAGAMENTO item = baseApp.GetPagtoById(id);
            objetoAntes = (ASSINANTE)Session["Assinante"];
            item.ASPA_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditPagto(item);
            return RedirectToAction("VoltarAnexoAssinante");
        }

        [HttpGet]
        public ActionResult IncluirPagamento()
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
                    Session["MensAssi"] = 2;
                    return RedirectToAction("VoltarAnexoAssinante");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ASSINANTE_PAGAMENTO item = new ASSINANTE_PAGAMENTO();
            AssinantePagamentoViewModel vm = Mapper.Map<ASSINANTE_PAGAMENTO, AssinantePagamentoViewModel>(item);
            vm.ASSI_CD_ID = (Int32)Session["IdAssi"];
            vm.ASPA_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirPagamento(AssinantePagamentoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ASSINANTE_PAGAMENTO item = Mapper.Map<AssinantePagamentoViewModel, ASSINANTE_PAGAMENTO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreatePagto(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoAssinante");
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




    }
}