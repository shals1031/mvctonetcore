using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeliconLatest
{
    public class TeliconActionFilter : IAuthorizationFilter
    {
        private readonly TeliconDbContext db;
        public TeliconActionFilter(TeliconDbContext db)
        {
            this.db = db;
        }
        public int TaskId { get; set; }
        public ActionMode Mode { get; set; }

        //
        // Summary:
        //     Called when a process requests authorization.
        //
        // Parameters:
        //   filterContext:
        //     The filter context, which encapsulates information for using System.Web.Mvc.AuthorizeAttribute.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The filterContext parameter is null.
        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            //using TelicondbContext db = new TelicondbContext();
            string id = db.Users.FirstOrDefault(m => m.UserName == filterContext.HttpContext.User.Identity.Name).UserId;
            string[] roles = db.UsersInRoles.Where(x => x.UserId == id).Select(p => p.Roles.RoleName).ToArray();
            var roleIds = db.Roles.Where(x => roles.Contains(x.RoleName)).Select(x => x.RoleId).ToList();
            var roleTask = db.TasksInRoles.Where(x => roleIds.Contains(x.RoleId) && x.TaskId == TaskId).FirstOrDefault();
            if (roleTask == null)
            {
                if (roles.Contains("AppAdmin") && TaskId == 1)
                {

                }
                else
                {
                    RouteValueDictionary redirectTargetDictionary = new RouteValueDictionary
                    {
                        { "action", "Index" },
                        { "controller", "Home" }
                    };

                    filterContext.Result = new RedirectToRouteResult(redirectTargetDictionary);
                    filterContext.RouteData.Values.Add("message", "Access Denied, this action requires more privileges.");
                }
            }
            else
            {
                if (Mode == ActionMode.Write && !roleTask.CanWrite)
                {
                    JsonReturnParams Data = new JsonReturnParams
                    {
                        Additional = 1,
                        Code = "1436",
                        Msg = ""
                    };
                    filterContext.Result = new JsonResult(Data);
                }
            }
        }

        //
        // Summary:
        //     When overridden, provides an entry point for custom authorization checks.
        //
        // Parameters:
        //   httpContext:
        //     The HTTP context, which encapsulates all HTTP-specific information about an individual
        //     HTTP request.
        //
        // Returns:
        //     true if the user is authorized; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The httpContext parameter is null.
        //protected virtual bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    return false;
        //}

        //
        // Summary:
        //     Processes HTTP requests that fail authorization.
        //
        // Parameters:
        //   filterContext:
        //     Encapsulates the information for using System.Web.Mvc.AuthorizeAttribute. The
        //     filterContext object contains the controller, HTTP context, request context,
        //     action result, and route data.
        protected virtual void HandleUnauthorizedRequest(AuthorizationHandlerContext filterContext)
        {

        }
    }
    public class TeliconAuthorizeAttribute : TypeFilterAttribute
    {
        public TeliconAuthorizeAttribute(int TaskId) : base(typeof(TeliconActionFilter))
        {
            Arguments = new object[] { TaskId };
        }
    }

    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
                return;

            context.Result = new RedirectResult("/Auth/Login");
        }
    }

    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute() : base(typeof(AuthorizeActionFilter))
        {
        }
    }

    public enum ActionMode
    {
        Read,
        Write
    }
}