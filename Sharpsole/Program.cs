using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;

namespace Sharpsole
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("Compile....");
			var csc = new CSharpCodeProvider();

			string[] references = new string[Sharpsole.Properties.Settings.Default.References.Count];
			for (int i = 0; i < references.Length; i++)
			{
				references[i] = Sharpsole.Properties.Settings.Default.References[i];
			}

			var options = new CompilerParameters(references);
			options.CompilerOptions = "/target:exe";
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;

			var result = csc.CompileAssemblyFromFile(options, args);
			if (result.Errors.HasErrors)
			{
				foreach (CompilerError error in result.Errors)
				{
					Console.WriteLine(error);
				}
				return -1;
			}

			var asm = result.CompiledAssembly;

			if (asm.EntryPoint != null)
			{
				try
				{
					if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("Run....");
					asm.EntryPoint.Invoke(null, new object[0]);
					if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("Done!");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
			else
			{
				if (Sharpsole.Properties.Settings.Default.IsVerbose) Console.WriteLine("No entry point found!");
			}

			return 0;
		}
	}
}