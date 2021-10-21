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
    public class TemplateController : Controller
    {
        private readonly ITemplateAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IConfiguracaoAppService confApp;

        private String msg;
        private Exception exception;
        TEMPLATE objeto = new TEMPLATE();
        TEMPLATE objetoAntes = new TEMPLATE();
        List<TEMPLATE> listaMaster = new List<TEMPLATE>();
        String extensao;

        public TemplateController(ITemplateAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps, IConfiguracaoAppService confApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
            confApp = confApps;
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
        public ActionResult MontarTelaTemplate()
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<TEMPLATE>)Session["ListaTemplate"] == null || ((List<TEMPLATE>)Session["ListaTemplate"]).Count == 0)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaTemplate"] = listaMaster;
            }
            ViewBag.Listas = (List<TEMPLATE>)Session["ListaTemplate"];
            ViewBag.Title = "Template";
            Session["Template"] = null;
            Session["IncluirTemplate"] = 0;

            // Indicadores
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            if (Session["MensTemplate"] != null)
            {
                if ((Int32)Session["MensTemplate"] == 2)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensTemplate"] == 3)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0032", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensTemplate"] == 4)
                {
                    ModelState.AddModelError("", PlatMensagens_Resources.ResourceManager.GetString("M0033", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            Session["VoltaTemplate"] = 1;
            objeto = new TEMPLATE();
            return View(objeto);
        }

        public ActionResult RetirarFiltroTemplate()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaTemplate"] = null;
            return RedirectToAction("MontarTelaTemplate");
        }

        public ActionResult MostrarTudoTemplate()
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaTemplate"] = null;
            return RedirectToAction("MontarTelaTemplate");
        }

        public ActionResult VoltarBaseTemplate()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("MontarTelaTemplate");
        }

        [HttpPost]
        public ActionResult FiltrarTemplate(TEMPLATE item)
        {

            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                List<TEMPLATE> listaObj = new List<TEMPLATE>();
                Session["FiltroTemplate"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.TEMP_SG_SIGLA, item.TEMP_NM_NOME, item.TEMP_TX_CORPO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensTemplate"] = 1;
                    return RedirectToAction("MontarTelaTemplate");
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaTemplate"] = listaObj;
                return RedirectToAction("MontarTelaTemplate");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaTemplate");
            }
        }

        [HttpGet]
        public ActionResult IncluirTemplate()
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("MontarTelaTemplate", "Template");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            List<SelectListItem> tipos = new List<SelectListItem>();
            tipos.Add(new SelectListItem() { Text = "E-Mail", Value = "1" });
            tipos.Add(new SelectListItem() { Text = "SMS", Value = "2" });
            tipos.Add(new SelectListItem() { Text = "WhatsApp", Value = "3" });
            ViewBag.Tipos = new SelectList(tipos, "Value", "Text");
            
            // Prepara view
            TEMPLATE item = new TEMPLATE();
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            vm.ASSI_CD_ID = idAss;
            vm.TEMP_DT_CRIACAO = DateTime.Today.Date;
            vm.TEMP_IN_ATIVO = 1;
            vm.TEMP_IN_TIPO = 1;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirTemplate(TemplateViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            List<SelectListItem> tipos = new List<SelectListItem>();
            tipos.Add(new SelectListItem() { Text = "E-Mail", Value = "1" });
            tipos.Add(new SelectListItem() { Text = "SMS", Value = "2" });
            tipos.Add(new SelectListItem() { Text = "WhatsApp", Value = "3" });
            ViewBag.Tipos = new SelectList(tipos, "Value", "Text");
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    TEMPLATE item = Mapper.Map<TemplateViewModel, TEMPLATE>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensTemplate"] = 3;
                        return RedirectToAction("MontarTelaTemplate");
                    }

                    // Sucesso
                    listaMaster = new List<TEMPLATE>();
                    Session["ListaTemplate"] = null;
                    Session["IdTemplate"] = item.TEMP_CD_ID;
                    return RedirectToAction("MontarTelaTemplate");
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
        public ActionResult EditarTemplate(Int32 id)
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("MontarTelaTemplate", "Template");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            TEMPLATE item = baseApp.GetItemById(id);
            Session["Template"] = item;

            // Indicadores

            // Mensagens
            if (Session["MensTemplate"] != null)
            {


            }

            Session["VoltaTemplate"] = 1;
            objetoAntes = item;
            Session["IdTemplate"] = id;
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarTemplate(TemplateViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    TEMPLATE item = Mapper.Map<TemplateViewModel, TEMPLATE>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuario);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<TEMPLATE>();
                    Session["ListaTemplate"] = null;
                    return RedirectToAction("MontarTelaTemplate");
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

        public ActionResult VoltarAnexoTemplate()
        {

            return RedirectToAction("EditarTemplate", new { id = (Int32)Session["IdTemplate"] });
        }

        [HttpGet]
        public ActionResult ExcluirTemplate(Int32 id)
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("MontarTelaTemplate");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            TEMPLATE item = baseApp.GetItemById(id);
            objetoAntes = (TEMPLATE)Session["Template"];
            item.TEMP_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensTemplate"] = 4;
                return RedirectToAction("MontarTelaTemplate");
            }
            Session["ListaTemplate"] = null;
            return RedirectToAction("MontarTelaTemplate");
        }

        [HttpGet]
        public ActionResult ReativarTemplate(Int32 id)
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("MontarTelaTemplate");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            TEMPLATE item = baseApp.GetItemById(id);
            objetoAntes = (TEMPLATE)Session["Template"];
            item.TEMP_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            Session["ListaTemplate"] = null;
            return RedirectToAction("MontarTelaTemplate");
        }

        [HttpGet]
        public ActionResult VerTemplate(Int32 id)
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("MontarTelaTemplate", "Template");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            Session["IdTemplate"] = id;
            TEMPLATE item = baseApp.GetItemById(id);
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            return View(vm);
        }

    }
}