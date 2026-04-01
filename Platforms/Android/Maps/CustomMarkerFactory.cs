using AndroidContext = global::Android.Content.Context;
using AndroidBitmap = global::Android.Graphics.Bitmap;
using AndroidCanvas = global::Android.Graphics.Canvas;
using AndroidPaint = global::Android.Graphics.Paint;
using AndroidPaintFlags = global::Android.Graphics.PaintFlags;
using AndroidColor = global::Android.Graphics.Color;
using AndroidRectF = global::Android.Graphics.RectF;

namespace MauiApp1.Platforms.Android.Maps;

internal static class CustomMarkerFactory
{
    public static AndroidBitmap Create(AndroidContext context, double rating)
    {
        var density = context.Resources?.DisplayMetrics?.Density ?? 1f;

        int width = (int)(86 * density);
        int height = (int)(112 * density);

        var bitmap = AndroidBitmap.CreateBitmap(width, height, AndroidBitmap.Config.Argb8888)!;
        var canvas = new AndroidCanvas(bitmap);

        using var anti = new AndroidPaint(AndroidPaintFlags.AntiAlias);

        float centerX = width / 2f;
        float imageRadius = 28f * density;
        float imageCenterY = 36f * density;

        anti.Color = AndroidColor.White;
        canvas.DrawCircle(centerX, imageCenterY, imageRadius + (3f * density), anti);

        anti.Color = AndroidColor.Rgb(43, 43, 43);
        canvas.DrawCircle(centerX, imageCenterY, imageRadius, anti);

        anti.Color = AndroidColor.Rgb(245, 102, 31);
        float ratingTop = imageCenterY + imageRadius - (7f * density);
        float ratingBottom = ratingTop + (20f * density);
        var ratingRect = new AndroidRectF(centerX - (28f * density), ratingTop, centerX + (28f * density), ratingBottom);
        canvas.DrawRoundRect(ratingRect, 10f * density, 10f * density, anti);

        using var textPaint = new AndroidPaint(AndroidPaintFlags.AntiAlias)
        {
            Color = AndroidColor.White,
            TextSize = 12f * density,
            TextAlign = AndroidPaint.Align.Center,
            FakeBoldText = true
        };

        canvas.DrawText($"{rating:0.0} ★", centerX, ratingTop + (14f * density), textPaint);

        anti.Color = AndroidColor.Rgb(255, 213, 192);
        canvas.DrawCircle(centerX, height - (18f * density), 16f * density, anti);

        anti.Color = AndroidColor.Rgb(245, 140, 92);
        canvas.DrawCircle(centerX, height - (18f * density), 10f * density, anti);

        return bitmap;
    }
}
