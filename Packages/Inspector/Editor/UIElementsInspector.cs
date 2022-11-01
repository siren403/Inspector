using MetaRun.Editor.Extensions;
using UnityEngine.UIElements;

namespace MetaRun.Editor
{
    public abstract class UIElementsInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            this.AddProperties(root);
            this.AddButtons(root);
            return root;
        }
    }
  
}