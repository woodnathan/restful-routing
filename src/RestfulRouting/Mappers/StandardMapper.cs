﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestfulRouting.Mappers
{
    public class StandardMapper : Mapper
    {
        public Route Route;

        public StandardMapper Path(string url)
        {
            Route = new Route(url,
                new RouteValueDictionary(),
                new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("GET") }),
                new RouteValueDictionary(),
                RouteHandler);

            return this;
        }

        private static string GetActionName<TController>(Expression<Func<TController, object>> actionExpression)
        {
            var body = ((MethodCallExpression)actionExpression.Body);
            var actionName = body.Method.Name;
            return RouteSet.LowercaseDefaults ? actionName.ToLowerInvariant() : actionName;
        }

        public StandardMapper To<T>(Expression<Func<T, object>> func)
        {
            Route.Defaults["controller"] = GetControllerName<T>();

            Route.Defaults["action"] = GetActionName(func);

            return this;
        }

        public StandardMapper Constrain(string name, object constraint)
        {
            Route.Constraints[name] = constraint;

            return this;
        }

        public StandardMapper Default(string name, object constraint)
        {
            Route.Defaults[name] = constraint;

            return this;
        }

        public StandardMapper GetOnly()
        {
            Route.Constraints["httpMethod"] = new HttpMethodConstraint("GET");

            return this;
        }

        public StandardMapper Allow(params HttpVerbs[] methods)
        {
            return this.Allow(methods.Select(x => x.ToString()));
        }
        
        public StandardMapper Allow(params string[] methods)
        {
            Route.Constraints["httpMethod"] = new RestfulHttpMethodConstraint(methods.Select(x => x.ToUpperInvariant()).ToArray());

            return this;
        }

        public StandardMapper Named(string name)
        {
            Route = new NamedRoute(name, Route.Url, Route.Defaults, Route.Constraints, Route.RouteHandler);

            return this;
        }

        public StandardMapper WithNamespace<T>()
        {
            this.Namespaces = new[] { typeof(T).Namespace };
            return this;
        }

        public override void RegisterRoutes(RouteCollection routeCollection)
        {
            Route.Url = Join(BasePath, Route.Url);
            Route.RouteHandler = RouteHandler;
            
            if (Route.DataTokens == null)
                Route.DataTokens = new RouteValueDictionary();
            Route.DataTokens["Namespaces"] = this.Namespaces;
            
            routeCollection.Add(Route);
        }
    }
}
