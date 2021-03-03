using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShovelRouteMgmt
{
    public enum RouteMgmtMode
    {
        Add,
        Delete
    }

    struct RouteInfo
    {
        public string TenantOrServiceName;
        public string AppId;
    }

    class RouteEntry
    {
        private RouteMgmtMode Mode = RouteMgmtMode.Add;
        private string choiceString = "Please choose 1 for add, or 2 for delete mode:";

        public void Run()
        {
            ChooseMode();
        }

        private void ChooseMode()
        {
            // could loop without chosen to enable multiple route entry
            bool chosen = false;
            while (!chosen)
            {
                Console.WriteLine(choiceString);
                var choice = Console.ReadKey();
                switch (choice.Key)
                {
                    case ConsoleKey.D1:
                        AddMode();
                        break;
                    case ConsoleKey.D2:
                        DeleteMode();
                        break;
                    case ConsoleKey.X:
                        chosen = true;
                        break;
                    default:
                        break;
                }
            }

        }

        private void AddMode()
        {
            var route = GetRouteInfo();
            if (RedisRouteMgmt.AddRoute(route.TenantOrServiceName + route.AppId))
            {
                Console.WriteLine("Route Added");
            }
            else
            {
                Console.WriteLine("Route Add Failed");
            }
        }

        private void DeleteMode()
        {
            var route = GetRouteInfo();
            if(RedisRouteMgmt.DeleteRoute(route.TenantOrServiceName + route.AppId))
            {
                Console.WriteLine("Route Deleted");
            }
            else
            {
                Console.WriteLine("Route delete Failed");
            }
        }

        private RouteInfo GetRouteInfo()
        {
            bool correct = false;
            var route = new RouteInfo();

            while (!correct)
            {

                Console.WriteLine("\nPlease enter tenant or service name:");
                route.TenantOrServiceName = Console.ReadLine();

                Console.WriteLine("\nPlease enter appId:");
                route.AppId = Console.ReadLine();

                Console.WriteLine("\nIs this correct? Y or start again");
                Console.WriteLine("Tenant or service name:" +
                route.TenantOrServiceName);

                Console.WriteLine("AppId:" +
                route.AppId);



                var choice = Console.ReadKey();
                {
                    if (choice.Key == ConsoleKey.Y)
                    {
                        correct = true;
                    }
                }
            }
            return route;

        }
    }
}
