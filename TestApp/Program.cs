using System;
using System.Reflection;
using Dojo.Generators;
using Microsoft.Extensions.Configuration;


namespace ClassLibrary1
{
    [AutoException]
    public partial class MyException
    {
    }

    public partial class GcpSettings: IGcpSettings
    {
        public void Foo()
        {
            Console.WriteLine(this.ProjectId);
        }
    }

    public interface IGcpSettings
    {
        string ProjectId { get; }
    }

    public static class Program
    {
        public static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("/Users/alexmalafeev/paymentsense/source-generators/TestApp/appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            AppSettings apps = new AppSettings(configuration);
            
            apps.Gcp.Foo();
            throw new MyException();
        }
    }
}