using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Widget;
using DAP.Mobile.CustomControls;
using DAP.Mobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RoundedPicker), typeof(RoundedPickerRenderer))]
[assembly: ExportRenderer(typeof(RoundedTimePicker), typeof(RoundedTimePickerRenderer))]
[assembly: ExportRenderer(typeof(RoundedDatePicker), typeof(RoundedDatePickerRenderer))]

namespace DAP.Mobile.Droid.Renderers
{
    public class RoundedPickerRenderer : PickerRenderer
    {
        public RoundedPickerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            RendererHelper.SetPickerStyles(Control);
        }
    }

    public class RoundedTimePickerRenderer : TimePickerRenderer
    {
        public RoundedTimePickerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.TimePicker> e)
        {
            base.OnElementChanged(e);
            RendererHelper.SetPickerStyles(Control);
        }
    }

    public class RoundedDatePickerRenderer : DatePickerRenderer
    {
        public RoundedDatePickerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.DatePicker> e)
        {
            base.OnElementChanged(e);
            RendererHelper.SetPickerStyles(Control);
        }
    }

    public static class RendererHelper
    {
        public static void SetPickerStyles(EditText control)
        {
            if (control == null) return;

            GradientDrawable gd = new GradientDrawable();
            gd.SetColor(ColorStateList.ValueOf(Android.Graphics.Color.White));
            gd.SetCornerRadius(50);

            Drawable[] layers = { gd, GetDrawable(control) };
            LayerDrawable layerDrawable = new LayerDrawable(layers);
            layerDrawable.SetLayerInset(0, 0, 0, 0, 0);
            layerDrawable.SetPadding(0, 0, 10, 0);

            control.Background = layerDrawable;
        }

        private static BitmapDrawable GetDrawable(EditText control)
        {
            int resID = control.Resources.GetIdentifier("arrow", "drawable", control.Context.PackageName);
            var drawable = ContextCompat.GetDrawable(control.Context, resID);
            var bitmap = ((BitmapDrawable)drawable).Bitmap;

            var result = new BitmapDrawable(control.Resources, Bitmap.CreateScaledBitmap(bitmap, 70, 70, true));
            result.Gravity = Android.Views.GravityFlags.Right;

            return result;
        }
    }
}