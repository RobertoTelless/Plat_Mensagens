using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EntitiesServices.Model;
using PlatMensagem_Solution.ViewModels;

namespace MvcMapping.Mappers
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<USUARIO, UsuarioViewModel>();
            CreateMap<USUARIO, UsuarioLoginViewModel>();
            CreateMap<LOG, LogViewModel>();
            //CreateMap<CONFIGURACAO, ConfiguracaoViewModel>();
            CreateMap<NOTIFICACAO, NotificacaoViewModel>();
        }
    }
}
