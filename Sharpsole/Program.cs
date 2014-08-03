using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Sharpsole
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			string root = Path.GetDirectoryName(typeof(Program).Assembly.Location);

			if (Sharpsole.Properties.Settings.Default.IsVerbose)
			{
				// AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
				Console.WriteLine("Compile....");
			}
			var csc = new CSharpCodeProvider();

			string[] references = new string[Sharpsole.Properties.Settings.Default.References.Count];
			for (int i = 0; i < references.Length; i++)
			{
				references[i] = Sharpsole.Properties.Settings.Default.References[i];
			}

			var options = new CompilerParameters(references);
			options.CompilerOptions = "/target:exe";
			options.GenerateExecutable = true;
			options.GenerateInMemory = false;

			List<string> additionalFiles = new List<string>();

			string[] sources = new string[args.Length];
			for (int i = 0; i < sources.Length; i++)
			{
				string[] lines = File.ReadAllLines(args[i]);

				for (int j = 0; j < lines.Length; j++)
				{
					if (lines[j].StartsWith("#library"))
					{
						string libFile = lines[j].Substring("#library".Length).Trim();
						if (string.IsNullOrWhiteSpace(libFile))
							continue;
						if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("Adding external library: {0}", libFile);

						string target = root + "/" + Path.GetFileName(libFile);
                        File.Copy(libFile, target, true);
						additionalFiles.Add(target);

						options.ReferencedAssemblies.Add(libFile);
						lines[j] = "";
					}
				}
				sources[i] = string.Join("\n", lines);
			}

			var result = csc.CompileAssemblyFromSource(options, sources);
			if (result.Errors.HasErrors)
			{
				foreach (CompilerError error in result.Errors)
				{
					Console.WriteLine(error);
				}
				return -1;
			}
			var domain = AppDomain.CreateDomain("result");
			try
			{
				if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("Run....");
				domain.ExecuteAssembly(result.PathToAssembly);
				if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("Done!");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				AppDomain.Unload(domain);

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}

			foreach(var file in additionalFiles)
			{
				File.Delete(file);
			}

			return 0;
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Console.WriteLine("Load assembly: {0}", args.LoadedAssembly.Location);
		}
	}
}