using Android.Content;
using Android.Webkit;
using Java.Interop;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using XHApp.CustomViews;
using XHApp.Droid.Renderers;

[assembly: ExportRenderer(typeof(EnglishAssistantView), typeof(EnglishAssistantViewRenderer))]
namespace XHApp.Droid.Renderers
{
    public class EnglishAssistantViewRenderer : WebViewRenderer
    {
        readonly string JavascriptFunction;
        readonly Context _context;

        public EnglishAssistantViewRenderer(Context context) : base(context)
        {
            _context = context;

            using (StreamReader sr = new StreamReader(context.Assets.Open("WebViewJavascriptBridge.js")))
            {
                JavascriptFunction = sr.ReadToEnd();
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
            {
                Control.RemoveJavascriptInterface("android");
            }
            if (e.NewElement != null)
            {
                Control.SetWebViewClient(new EnglishAssistantJavascriptWebViewClient($"javascript: {JavascriptFunction}"));
                Control.AddJavascriptInterface(new EnglishAssistantJSBridge(this), "android");
            }
        }
    }

    public class EnglishAssistantJavascriptWebViewClient : WebViewClient
    {
        string _javascript;

        public EnglishAssistantJavascriptWebViewClient(string javascript)
        {
            _javascript = javascript;
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            base.OnPageFinished(view, url);
            view.EvaluateJavascript(_javascript, null);
        }
    }

    public class EnglishAssistantJSBridge : Java.Lang.Object
    {
        readonly WeakReference<EnglishAssistantViewRenderer> renderer;

        public EnglishAssistantJSBridge(EnglishAssistantViewRenderer englishAssistantViewsRenderer)
        {
            this.renderer = new WeakReference<EnglishAssistantViewRenderer>(englishAssistantViewsRenderer);
        }

        [JavascriptInterface]
        [Export("voiceRequestFromWeb")]
        public string InvokeAction(string data, string callbackId)
        {
            string result = "error";
            EnglishAssistantViewRenderer englishAssistantViewsRenderer;

            if (renderer != null && renderer.TryGetTarget(out englishAssistantViewsRenderer))
            {
                EnglishAssistantView view = (EnglishAssistantView)englishAssistantViewsRenderer.Element;

                switch (data.Trim('"').ToUpper())
                {
                    case "LOG_IN":
                        result = view.LogIn().GetAwaiter().GetResult();
                        break;
                    case "VOICE_START":
                        result = view.StartRecord().GetAwaiter().GetResult();
                        break;
                    case "VOICE_STOP":
                        result = view.StopRecord().GetAwaiter().GetResult();
                        break;
                }
            }

            return result;
        }
    }
}