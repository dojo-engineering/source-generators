using System.Text;
using Dojo.Generators.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dojo.OpenApiGenerator
{
    [Generator]
    public class AutoControllerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassWithAttributeSyntaxReceiver("AutoController"));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = context.SyntaxReceiver as ClassWithAttributeSyntaxReceiver;

            foreach (var candidateClass in syntaxReceiver.CandidateClasses)
            {
                
            }

            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(@"
            using System;
            using System.CodeDom.Compiler;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Http;

            namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
            {   
                [GeneratedCode(""Dojo.OpenApiGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
                public class GenerateHelloWorldController : ControllerBase
                {
                    [HttpGet]
                    [ProducesResponseType(StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
                    public virtual IActionResult Hello()
                    {
                        return Ok(""Hello From Generator 4"");
                    }
                }
            }");

            // inject the created source into the users compilation
            context.AddSource("HelloWorldController.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
