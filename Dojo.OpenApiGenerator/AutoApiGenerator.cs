using System;
using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Stubble.Core;
using Stubble.Core.Builders;

namespace Dojo.OpenApiGenerator
{
    [Generator]
    public class AutoApiGenerator : ISourceGenerator
    {
        private readonly StubbleVisitorRenderer _stubbleBuilder;
        private string _projectDir;
        private AutoApiGeneratorSettings _autoApiGeneratorSettings;
        private readonly IGenerateApiConfigurationService _generateApiConfiguration;
        private readonly IGenerateInheritedApiVersionAttribute _generateInheritedApiVersionAttributervice;
        private readonly IGenerateApisSourceCode _generateApisSourceCode;
    
        public AutoApiGenerator()
        {
            _stubbleBuilder = new StubbleBuilder().Build();
            _generateApiConfiguration = new GenerateApiConfigurationService();
            _generateInheritedApiVersionAttributervice = new GenerateInheritedApiVersionAttribute();
            _generateApisSourceCode = new GenerateApisSourceCode();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif

//            Debug.WriteLine("Initialize code generator");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                _projectDir = context.GetProjectDir();
                _autoApiGeneratorSettings = AutoApiGeneratorSettings.GetAutoApiGeneratorSettings(_projectDir);
                var projectNamespace = context.GetProjectDefaultNamespace();
                var apisToOverride = new List<string>();

                _generateApiConfiguration
                    .GenerateApiConfiguratorSourceCode(context, projectNamespace, _stubbleBuilder)
                    // .Bind(() => _generateInheritedApiVersionAttributervice.GenerateInheritedApiVersionAttributeSourceCode(context,  projectNamespace, _stubbleBuilder))
                    .Bind(()=> _generateApisSourceCode.GenerateApiSourceCode(context, projectNamespace, _stubbleBuilder, apisToOverride, _projectDir, _autoApiGeneratorSettings)); 
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine($"Ignore auto api generator for directory {_projectDir} because of exception: {exception.Message}");
            }
        }
    }
}
