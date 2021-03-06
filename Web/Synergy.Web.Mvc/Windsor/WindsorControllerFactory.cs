﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using JetBrains.Annotations;
using Synergy.Contracts;
using Synergy.Core;

namespace Synergy.Web.Mvc.Windsor
{
    public class WindsorControllerFactory : IControllerFactory
    {
        private readonly IComponentLocator componentLocator;

        public WindsorControllerFactory(IComponentLocator componentLocator)
        {
            this.componentLocator = componentLocator;
        }

        /// <inheritdoc />
        [NotNull]
        public IController CreateController([NotNull] RequestContext requestContext, [NotNull]string controllerPrefix)
        {
            Fail.IfArgumentNull(requestContext, nameof(requestContext));
            Fail.IfArgumentNull(controllerPrefix, nameof(controllerPrefix));

            var controllerName = String.Concat(controllerPrefix, "Controller");
            if (this.componentLocator.HasComponent(controllerName))
            {
                var controller = this.componentLocator.GetComponent<IController>(controllerName);

                //var requestArea = requestContext.RouteData.GetAreaName() ?? "";
                //var controllerArea = ExtensionHelper.GetAreaName(controller) ?? "";

                //if (requestArea == controllerArea)
                    return controller;
            }

            throw new HttpException(404, $"Controller '{controllerPrefix}' was not found. Requested URL '{requestContext.HttpContext.Request.Url}'");
        }

        /// <inheritdoc />
        public SessionStateBehavior GetControllerSessionBehavior([NotNull] RequestContext requestContext, [NotNull] string controllerName)
        {
            return new SessionStateBehavior();
        }

        /// <inheritdoc />
        public void ReleaseController([NotNull] IController controller)
        {
            Fail.IfArgumentNull(controller, "controller");

            this.componentLocator.ReleaseComponent(controller);
        }
    }
}