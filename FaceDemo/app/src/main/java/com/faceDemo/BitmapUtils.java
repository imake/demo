package com.faceDemo;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.util.Log;

import org.pytorch.IValue;
import org.pytorch.Module;
import org.pytorch.Tensor;
import org.pytorch.torchvision.TensorImageUtils;


public class BitmapUtils {


    /**
     * @param backBitmap  背景图
     * @param frontBitmap 上层图
     * @return
     */
    public static Bitmap mergeBitmap(Bitmap backBitmap, Bitmap frontBitmap, int x, int y) {
        //backBitmap =scaleBitmap(backBitmap,IMG_DEFAULT_WIDTH,IMG_DEFAULT_HEIGHT);
        //合并bitmap
        Bitmap bitmap = Bitmap.createBitmap(backBitmap.getWidth(), backBitmap.getHeight(), Bitmap.Config.RGB_565);
        Canvas canvas = new Canvas(bitmap);
        canvas.drawBitmap(backBitmap, 0, 0, null);
        if (frontBitmap != null) {
            Paint paint = new Paint();
            //paint.setColorFilter(new PorterDuffColorFilter(android.graphics.Color.RED, PorterDuff.Mode.SRC_IN));
            canvas.drawBitmap(frontBitmap, x, y, null);
        }
        return bitmap;
    }

    public static Bitmap scaleBitmap(Bitmap bitmap, int defWith, int defHeght) {
        //缩放
        float scaleX = 1.0f;
        float scaleY = 1.0f;
        if (bitmap.getWidth() != defWith) {
            scaleX = (defWith * 1.0f / bitmap.getWidth());
        }
        if (bitmap.getHeight() != defHeght) {
            scaleY = (defHeght * 1.0f / bitmap.getHeight());
        }
        Matrix matrix = new Matrix();
        matrix.postScale(scaleX, scaleY);
        return Bitmap.createBitmap(bitmap, 0, 0, bitmap.getWidth(), bitmap.getHeight(), matrix, false);
    }

    public static float[] PytorchFunction(Module module, Context context, float[] data, int newWidth,int newHeight) {

        //Bitmap bitmap = Bitmap.createScaledBitmap(_bitmap, newWidth, newHeight, true);
        final int fWidth = newWidth;
        final int fHeight = newHeight;

        float[] floatMean = new float[]{(float) 0.5, (float) 0.5, (float) 0.5};
        float[] floatStd = new float[]{(float) 0.5, (float) 0.5, (float) 0.5};
        // Tensor inputTensor = TensorImageUtils.bitmapToFloat32Tensor(_bitmap, floatMean, floatStd);
        final Tensor inputTensor =Tensor.fromBlob(data, new long[]{1,3,200,200});
        Log.d("inputTensor", "PytorchFunction: inputTensor="+inputTensor);
        final Module finalModule = module;

        long start_time = System.nanoTime();
        Tensor transfered = finalModule.forward(IValue.from(inputTensor)).toTensor();
        long end_time = System.nanoTime();
        float[] floatImage = transfered.getDataAsFloatArray();
        return floatImage;
        /*
        int[] intImage = new int[floatImage.length];
        Log.d("intImage", "intImage: "+intImage.length);
        for (int i = 0; i <= floatImage.length - 1; i++) {
            intImage[i] = (int) ((float) 255.0 * (floatImage[i] * 0.5 + 0.5));
            if (intImage[i] > 255) {
                intImage[i] = 255;
            } else if (intImage[i] < 0) {
                intImage[i] = 0;
            }
        }
        int[] colors = new int[fHeight * fWidth];
        Log.d("colors", "colors: "+colors.length);
        for (int i = 0; i <= colors.length - 1; i++) {
            colors[i] = Color.argb(255, intImage[i], intImage[i + fHeight * fWidth], intImage[i + fHeight * fWidth * 2]);
        }
        Bitmap transferedBitmap = Bitmap.createBitmap(colors, fWidth, fHeight, Bitmap.Config.RGB_565);
        //Toast.makeText(context, "time used: " + String.valueOf((end_time - start_time) / 1000000000.0), Toast.LENGTH_LONG).show();
        return transferedBitmap;

         */
    }
}
