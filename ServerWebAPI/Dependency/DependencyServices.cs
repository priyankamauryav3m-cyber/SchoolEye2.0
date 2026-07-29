using System.Reflection;

public static class DependencyServices
{
    public static void RegisterServices(this IServiceCollection services)
    {
        var assembly = Assembly.Load("ServerWebAPI");
        var applicationAssembly = Assembly.Load("ApplicationInterface");
        var infrastructureAssembly = Assembly.Load("Infrastructure"); 

        var assemblies = new[]
        {
            assembly,
            applicationAssembly,
            infrastructureAssembly
        };

        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service")||t.Name.EndsWith("Repository"));

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, type);
            }
        }
      //  AppDomain.CurrentDomain.GetAssemblies()
      //.Where(a => a.FullName != null &&
      //            a.FullName.StartsWith("MyERP"))
      //.SelectMany(a => a.GetTypes())
      //.Where(t => t.IsClass
      //            && !t.IsAbstract
      //            && (t.Name.EndsWith("Service") ||
      //                t.Name.EndsWith("Repository")))
      //.Select(type => new
      //{
      //    Service = type,
      //    Interface = type.GetInterface("I" + type.Name)
      //})
      //.Where(x => x.Interface != null)
      //.ToList()
      //.ForEach(x =>
      //    services.AddScoped(x.Interface!, x.Service));
    }
}
