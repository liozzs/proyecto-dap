using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using DAP.Mobile.CustomControls;
using DAP.Mobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RoundedEntry), typeof(RoundedEntryRenderer))]

namespace DAP.Mobile.Droid.Renderers
{
    public class RoundedEntryRenderer : EntryRenderer
    {
        public RoundedEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                GradientDrawable gd = new GradientDrawable();
                gd.SetColor(ColorStateList.ValueOf(Android.Graphics.Color.White));
                gd.SetCornerRadius(50);
                Control.SetBackgroundDrawable(gd);
            }
        }
    }
}