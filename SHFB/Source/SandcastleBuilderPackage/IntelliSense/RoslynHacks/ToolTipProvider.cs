namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Utilities;

    [ContentType("Roslyn Languages")]
    [Name("OverrideRoslynToolTipProvider")]
    [Order(Before = "RoslynToolTipProvider")]
    [Export(typeof(IUIElementProvider<Completion, ICompletionSession>))]
    internal class ToolTipProvider : IUIElementProvider<Completion, ICompletionSession>
    {
        private readonly SVsServiceProvider _serviceProvider;

        [ImportingConstructor]
        public ToolTipProvider(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public UIElement GetUIElement(Completion itemToRender, ICompletionSession context, UIElementType elementType)
        {
            if (RoslynUtilities.IsFinalRoslyn)
            {
                // The bug which requires this provider has been fixed
                return null;
            }

            // only hook when necessary
            if (!RoslynUtilities.IsRoslynInstalled(_serviceProvider) ?? true)
                return null;

            if (itemToRender != null && itemToRender.GetType().FullName != "Microsoft.CodeAnalysis.Editor.Implementation.Completion.Presentation.CustomCommitCompletion")
            {
                // The Roslyn-provided tool tip provider will throw an exception in this case, so
                // we override it with the default Visual Studio behavior
                TextBlock formattableTextBlock = new TextBlock();
                formattableTextBlock.Text = itemToRender.Description;
                formattableTextBlock.MaxWidth = GetScreenRect(context).Width * 0.4;
                formattableTextBlock.TextWrapping = TextWrapping.Wrap;
                //formattableTextBlock.TextRunProperties = this._completionTabView.SessionView.PresenterStyle.TooltipTextRunProperties;
                return formattableTextBlock;
            }

            // allow Roslyn to handle this case
            return null;
        }

        internal static Rect GetScreenRect(IIntellisenseSession session)
        {
            if (session != null && session.TextView != null)
            {
                Visual visualElement = ((IWpfTextView)session.TextView).VisualElement;
                if (PresentationSource.FromVisual(visualElement) != null)
                {
                    Rect screenRect = WpfHelper.GetScreenRect(visualElement.PointToScreen(new Point(0.0, 0.0)));
                    return new Rect(0.0, 0.0, screenRect.Width * WpfHelper.DeviceScaleX, screenRect.Height * WpfHelper.DeviceScaleY);
                }
            }

            return new Rect(0.0, 0.0, SystemParameters.PrimaryScreenWidth * WpfHelper.DeviceScaleX, SystemParameters.PrimaryScreenHeight * WpfHelper.DeviceScaleY);
        }
    }
}
