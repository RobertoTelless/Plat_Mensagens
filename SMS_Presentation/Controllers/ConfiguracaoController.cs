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
    public class ConfiguracaoController : Controller
    {
        private readonly IConfiguracaoAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private readonly ICategoriaClienteAppService catApp;
        private readonly IPosicaoAppService posApp;

        private String msg;
        private Exception exception;
        CONFIGURACAO objeto = new CONFIGURACAO();
        CONFIGURACAO objetoAntes = new CONFIGURACAO();
        List<CONFIGURACAO> listaMaster = new List<CONFIGURACAO>();
        CATEGORIA_CLIENTE objetoCat = new CATEGORIA_CLIENTE();
        CATEGORIA_CLIENTE objetoCatAntes = new CATEGORIA_CLIENTE();
        List<CATEGORIA_CLIENTE> listaMasterCat = new List<CATEGORIA_CLIENTE>();
        POSICAO objetoPos = new POSICAO();
        POSICAO objetoPosAntes = new POSICAO();
        List<POSICAO> listaMasterPos = new List<POSICAO>();
        String extensao;

        public ConfiguracaoController(IConfiguracaoAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps, ICategoriaClienteAppService catApps, IPosicaoAppService posApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
            catApp = catApps;
            posApp = posApps;
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
        public JsonResult GetConfiguracao()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            var config = baseApp.GetItemById(idAss);
            var serialConfig = new CONFIGURACAO
            {
                CONF_CD_ID = config.CONF_CD_ID,
                ASSI_CD_ID = config.ASSI_CD_ID,
                CONF_NR_FALHAS_DIA = config.CONF_NR_FALHAS_DIA,
                CONF_NM_HOST_SMTP = config.CONF_NM_HOST_SMTP,
                CONF_NM_PORTA_SMTP = config.CONF_NM_PORTA_SMTP,
                CONF_NM_EMAIL_EMISSOO = config.CONF_NM_EMAIL_EMISSOO,
                CONF_NM_SENHA_EMISSOR = config.CONF_NM_SENHA_EMISSOR,
                CONF_NR_REFRESH_DASH = config.CONF_NR_REFRESH_DASH,
                CONF_NM_ARQUIVO_ALARME = config.CONF_NM_ARQUIVO_ALARME,
                CONF_NR_REFRESH_NOTIFICACAO = config.CONF_NR_REFRESH_NOTIFICACAO,
                CONF_SG_LOGIN_SMS = config.CONF_SG_LOGIN_SMS,
                CONF_SG_SENHA_SMS = config.CONF_SG_SENHA_SMS,
            };

            return Json(serialConfig);
        }

        [HttpGet]
        public ActionResult MontarTelaConfiguracao()
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            objeto = baseApp.GetItemById(idAss);
            Session["Configuracao"] = objeto;

            ViewBag.Listas = (CONFIGURACAO)Session["Configuracao"];
            ViewBag.Title = "Configuracao";
            var listaGrid = new List<SelectListItem>();
            listaGrid.Add(new SelectListItem() { Text = "10", Value = "10" });
            listaGrid.Add(new SelectListItem() { Text = "25", Value = "25" });
            listaGrid.Add(new SelectListItem() { Text = "50", Value = "50" });
            listaGrid.Add(new SelectListItem() { Text = "100", Value = "100" });
            ViewBag.ListaGrid = new SelectList(listaGrid, "Value", "Text");

            // Indicadores

            // Mensagem

            // Abre view
            Session["MensConfiguracao"] = 0;
            objetoAntes = objeto;
            if (objeto.CONF_NR_FALHAS_DIA == null)
            {
                objeto.CONF_NR_FALHAS_DIA = 3;
            }
            Session["Configuracao"] = objeto;
            Session["IdConf"] = 1;
            ConfiguracaoViewModel vm = Mapper.Map<CONFIGURACAO, ConfiguracaoViewModel>(objeto);
            return View(vm);
        }

        [HttpPost]
        public ActionResult MontarTelaConfiguracao(ConfiguracaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            var listaGrid = new List<SelectListItem>();
            listaGrid.Add(new SelectListItem() { Text = "10", Value = "10" });
            listaGrid.Add(new SelectListItem() { Text = "25", Value = "25" });
            listaGrid.Add(new SelectListItem() { Text = "50", Value = "50" });
            listaGrid.Add(new SelectListItem() { Text = "100", Value = "100" });
            ViewBag.ListaGrid = new SelectList(listaGrid, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CONFIGURACAO item = Mapper.Map<ConfiguracaoViewModel, CONFIGURACAO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Sucesso
                    objeto = new CONFIGURACAO();
                    Session["ListaConfiguracao"] = null;
                    Session["Configuracao"] = null;
                    Session["MensConfiguracao"] = 0;
                    return RedirectToAction("MontarTelaConfiguracao");
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

        public ActionResult VoltarBaseConfiguracao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaConfiguracao");
        }

        [HttpGet]
        public ActionResult EditarConfiguracao(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CONFIGURACAO item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Configuracao"] = item;
            Session["IdVolta"] = id;
            ConfiguracaoViewModel vm = Mapper.Map<CONFIGURACAO, ConfiguracaoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarConfiguracao(ConfiguracaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CONFIGURACAO item = Mapper.Map<ConfiguracaoViewModel, CONFIGURACAO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Sucesso
                    objeto = new CONFIGURACAO();
                    Session["ListaConfiguracao"] = null;
                    Session["MensConfiguracao"] = 0;
                    return RedirectToAction("MontarTelaConfiguracao");
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
        public ActionResult MontarTelaCatCliente()
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<CATEGORIA_CLIENTE>)Session["ListaCat"] == null || ((List<CATEGORIA_CLIENTE>)Session["ListaCat"]).Count == 0)
            {
                listaMasterCat = catApp.GetAllItens();
                Session["ListaCat"] = listaMasterCat;
            }
            ViewBag.Listas = (List<CATEGORIA_CLIENTE>)Session["ListaCat"];
            ViewBag.Title = "CatCliente";
            Session["CatCliente"] = null;
            Session["IncluirCat"] = 0;

            // Indicadores
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            if (Session["MensConfiguracao"] != null)
            {
                if ((Int32)Session["MensConfiguracao"] == 2)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensConfiguracao"] == 10)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0028", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensConfiguracao"] == 11)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0029", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            Session["MensConfiguracao"] = null;
            Session["VoltaCat"] = 1;
            objetoCat = new CATEGORIA_CLIENTE();
            return View(objetoCat);
        }

        public ActionResult RetirarFiltroCat()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaCat"] = null;
            return RedirectToAction("MontarTelaCatCliente");
        }

        public ActionResult MostrarTudoCat()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterCat = catApp.GetAllItensAdm();
            Session["ListaCat"] = listaMasterCat;
            return RedirectToAction("MontarTelaCatCliente");
        }

        public ActionResult VoltarBaseCat()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("MontarTelaCatCliente");
        }

        [HttpGet]
        public ActionResult IncluirCat()
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaCatCliente", "Configuracao");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas

            // Prepara view
            CATEGORIA_CLIENTE item = new CATEGORIA_CLIENTE();
            CategoriaClienteViewModel vm = Mapper.Map<CATEGORIA_CLIENTE, CategoriaClienteViewModel>(item);
            vm.ASSI_CD_ID = idAss;
            vm.CACL_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirCat(CategoriaClienteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CATEGORIA_CLIENTE item = Mapper.Map<CategoriaClienteViewModel, CATEGORIA_CLIENTE>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = catApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensConfiguracao"] = 10;
                        return RedirectToAction("MontarTelaCatCliente");
                    }

                    // Sucesso
                    listaMasterCat = new List<CATEGORIA_CLIENTE>();
                    Session["ListaCat"] = null;
                    Session["IncluirCat"] = 1;
                    Session["IdCat"] = item.CACL_CD_ID;
                    return RedirectToAction("VoltarBaseCat");
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
        public ActionResult EditarCat(Int32 id)
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaCat", "Configuracao");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CATEGORIA_CLIENTE item = catApp.GetItemById(id);
            Session["Cat"] = item;

            // Indicadores
            ViewBag.Incluir = (Int32)Session["IncluirCat"];

            // Mensagens
            if (Session["MensConfiguracao"] != null)
            {


            }

            Session["VoltaCat"] = 1;
            objetoCatAntes = item;
            Session["IdCat"] = id;
            CategoriaClienteViewModel vm = Mapper.Map<CATEGORIA_CLIENTE, CategoriaClienteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarCat(CategoriaClienteViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    CATEGORIA_CLIENTE item = Mapper.Map<CategoriaClienteViewModel, CATEGORIA_CLIENTE>(vm);
                    Int32 volta = catApp.ValidateEdit(item, objetoCatAntes, usuario);

                    // Verifica retorno

                    // Sucesso
                    listaMasterCat = new List<CATEGORIA_CLIENTE>();
                    Session["ListaCat"] = null;
                    return RedirectToAction("MontarTelaCatCliente");
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
        public ActionResult ExcluirCat(Int32 id)
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaCatCliente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CATEGORIA_CLIENTE item = catApp.GetItemById(id);
            objetoCatAntes = (CATEGORIA_CLIENTE)Session["Cat"];
            item.CACL_IN_ATIVO = 0;
            Int32 volta = catApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensConfiguracao"] = 11;
                return RedirectToAction("MontarTelaCatCliente");
            }
            Session["ListaCat"] = null;
            return RedirectToAction("MontarTelaCatCliente");
        }

        [HttpGet]
        public ActionResult ReativarCat(Int32 id)
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaCatCliente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CATEGORIA_CLIENTE item = catApp.GetItemById(id);
            objetoCatAntes = (CATEGORIA_CLIENTE)Session["Cat"];
            item.CACL_IN_ATIVO = 1;
            Int32 volta = catApp.ValidateReativar(item, usuario);
            Session["ListaCat"] = null;
            return RedirectToAction("MontarTelaCatCliente");
        }

        [HttpGet]
        public ActionResult MontarTelaPosicao()
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<POSICAO>)Session["ListaPosicao"] == null || ((List<POSICAO>)Session["ListaPosicao"]).Count == 0)
            {
                listaMasterPos = posApp.GetAllItens();
                Session["ListaPosicao"] = listaMasterPos;
            }
            ViewBag.Listas = (List<POSICAO>)Session["ListaPosicao"];
            ViewBag.Title = "Posicao";
            Session["Posicao"] = null;
            Session["IncluirPos"] = 0;

            // Indicadores
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            if (Session["MensConfiguracao"] != null)
            {
                if ((Int32)Session["MensConfiguracao"] == 2)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensConfiguracao"] == 12)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0030", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensConfiguracao"] == 13)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0031", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            Session["MensConfiguracao"] = null;
            Session["VoltaPos"] = 1;
            objetoPos = new POSICAO();
            return View(objetoPos);
        }

        public ActionResult RetirarFiltroPosicao()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaPosicao"] = null;
            return RedirectToAction("MontarTelaPosicao");
        }

        public ActionResult MostrarTudoPosicao()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterPos = posApp.GetAllItensAdm();
            Session["ListaPosicao"] = listaMasterPos;
            return RedirectToAction("MontarTelaPosicao");
        }

        public ActionResult VoltarBasePosicao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("MontarTelaPosicao");
        }

        [HttpGet]
        public ActionResult IncluirPosicao()
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaPosicao", "Configuracao");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas

            // Prepara view
            POSICAO item = new POSICAO();
            PosicaoViewModel vm = Mapper.Map<POSICAO, PosicaoViewModel>(item);
            vm.ASSI_CD_ID = idAss;
            vm.POSI_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirPosicao(PosicaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    POSICAO item = Mapper.Map<PosicaoViewModel, POSICAO>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = posApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensConfiguracao"] = 10;
                        return RedirectToAction("MontarTelaPosicao");
                    }

                    // Sucesso
                    listaMasterPos = new List<POSICAO>();
                    Session["ListaPosicao"] = null;
                    Session["IncluirPosicao"] = 1;
                    Session["IdPosicao"] = item.POSI_CD_ID;
                    return RedirectToAction("VoltarBasePosicao");
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
        public ActionResult EditarPosicao(Int32 id)
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaPosicao", "Configuracao");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            POSICAO item = posApp.GetItemById(id);
            Session["Posicao"] = item;

            // Indicadores
            ViewBag.Incluir = (Int32)Session["IncluirPosicao"];

            // Mensagens
            if (Session["MensConfiguracao"] != null)
            {


            }

            Session["VoltaPosicao"] = 1;
            objetoPosAntes = item;
            Session["IdPosicao"] = id;
            PosicaoViewModel vm = Mapper.Map<POSICAO, PosicaoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarPosicao(PosicaoViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    POSICAO item = Mapper.Map<PosicaoViewModel, POSICAO>(vm);
                    Int32 volta = posApp.ValidateEdit(item, objetoPosAntes, usuario);

                    // Verifica retorno

                    // Sucesso
                    listaMasterPos = new List<POSICAO>();
                    Session["ListaPosicao"] = null;
                    return RedirectToAction("MontarTelaPosicao");
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
        public ActionResult ExcluirPosicao(Int32 id)
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaCatCliente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            POSICAO item = posApp.GetItemById(id);
            objetoPosAntes = (POSICAO)Session["Posicao"];
            item.POSI_IN_ATIVO = 0;
            Int32 volta = posApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensConfiguracao"] = 11;
                return RedirectToAction("MontarTelaPosicao");
            }
            Session["ListaPosicao"] = null;
            return RedirectToAction("MontarTelaPosicao");
        }

        [HttpGet]
        public ActionResult ReativarPosicao(Int32 id)
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
                    Session["MensConfiguracao"] = 2;
                    return RedirectToAction("MontarTelaPosicao");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            POSICAO item = posApp.GetItemById(id);
            objetoPosAntes = (POSICAO)Session["Posicao"];
            item.POSI_IN_ATIVO = 1;
            Int32 volta = posApp.ValidateReativar(item, usuario);
            Session["ListaPosicao"] = null;
            return RedirectToAction("MontarTelaPosicao");
        }
    }
}