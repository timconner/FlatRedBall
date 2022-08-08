using FlatRedBall.Glue.CodeGeneration;
using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.SaveClasses;

namespace GameCommunicationPlugin.CodeGeneration
{
    internal class HybridEntityCodeGenerator : ElementComponentCodeGenerator
    {
        public override ICodeBlock GenerateInitializeLate(ICodeBlock codeBlock, IElement element)
        {
            codeBlock.Line("GlueDynamicManager.GlueDynamicManager.Self.AttachEntity(this);");

            return base.GenerateInitializeLate(codeBlock, element);
        }
    }
}
