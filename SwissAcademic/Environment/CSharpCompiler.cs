using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using SwissAcademic.ApplicationInsights;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
#if Web
using System.Runtime.Loader;
#endif

namespace SwissAcademic
{
    public static class CSharpCompiler
    {
        static IEnumerable<MetadataReference> _assemblyReferences;

        public static IEnumerable<MetadataReference> AssemblyReferences
        {
            //https://github.com/dotnet/roslyn/issues/34111
            get
            {
                return LazyInitializer.EnsureInitialized(ref _assemblyReferences, () =>
                {

#if Web
                    var references = new List<MetadataReference>();

                    var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
                    foreach (var path in trustedAssembliesPaths)
                    {
                        references.Add(MetadataReference.CreateFromFile(path));
                    }

                    return references;
#else
                    var dotnetframmeworkroot = RuntimeEnvironment.GetRuntimeDirectory();
                    var applicationroot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    return new[]
                            {
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Drawing.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Net.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Net.Http.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Web.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Core.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Data.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Xml.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Xml.Linq.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(dotnetframmeworkroot, "System.Windows.Forms.dll")),

                                MetadataReference.CreateFromFile(typeof(MarshalByRefObject).GetTypeInfo().Assembly.Location), //mscorelib
                                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),

                                MetadataReference.CreateFromFile(Path.Combine(applicationroot, "Newtonsoft.Json.dll")),

                                MetadataReference.CreateFromFile(Path.Combine(applicationroot, "SwissAcademic.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(applicationroot, "SwissAcademic.AI.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(applicationroot, "SwissAcademic.Citavi.dll")),
                            };
#endif
                });
            }
        }

        public static CSharpCompilerResult Compile(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var assemblyName = Guid.NewGuid().ToString();
            var references = AssemblyReferences;
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    foreach (var dia in result.Diagnostics)
                    {
                        if (dia.Severity == DiagnosticSeverity.Error)
                        {
                            Telemetry.TrackTrace("CompilerError", SeverityLevel.Error, ("Error", dia));
                        }
                    }
                    return new CSharpCompilerResult(null, result.Diagnostics);
                }

                ms.Seek(0, SeekOrigin.Begin);
#if Web
                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
#else
                var assembly = Assembly.Load(ms.ToArray());
#endif

                return new CSharpCompilerResult(assembly, result.Diagnostics);
            }
        }

        public static Task Initalize()
        {
            return Task.Run(() =>
            {
                try
                {
                    var compilation = CSharpCompilation.Create(
                        Guid.NewGuid().ToString(),
                        new[] { CSharpSyntaxTree.ParseText("using SwissAcademic;using SwissAcademic.Citavi;public class CSharpCompilationInit{}") },
                        AssemblyReferences,
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                    using (var ms = new MemoryStream())
                    {
                        _ = compilation.Emit(ms);
                    }
                }
                catch { }
            });
        }
    }

    public class CSharpCompilerResult
    {
        internal CSharpCompilerResult(Assembly assembly, IEnumerable<Diagnostic> diagnostics)
        {
            CompiledAssembly = assembly;
            Diagnostics = diagnostics;
        }

        public Assembly CompiledAssembly { get; }
        public IEnumerable<Diagnostic> Diagnostics { get; }
        public IEnumerable<Diagnostic> Errors => Diagnostics.Where(dia => dia.Severity == DiagnosticSeverity.Error);
        public bool Success => CompiledAssembly != null;
    }
}
