using Makale_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MakaleWebProject.Filter
{
    public class AutAdmin : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            Kullanici user = filterContext.HttpContext.Session["login"] as Kullanici;

            if (user!= null && user.Admin==false)
            {
                filterContext.Result = new RedirectResult("/Home/YetkisizErişim");

            }
        }
    }
}