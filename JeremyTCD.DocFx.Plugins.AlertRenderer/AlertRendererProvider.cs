using System.Collections.Generic;
using System.Composition;
using Microsoft.DocAsCode.Dfm;

namespace JeremyTCD.DocFx.Plugins.BlockquoteRenderer
{
    [Export(typeof(IDfmCustomizedRendererPartProvider))]
    public class AlertRendererProvider : IDfmCustomizedRendererPartProvider
    {
        public IEnumerable<IDfmCustomizedRendererPart> CreateParts(IReadOnlyDictionary<string, object> parameters)
        {
            yield return new AlertRenderer();
        }
    }
}
