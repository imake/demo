package org.pytorch.demo.vision;

import android.Manifest;
import android.annotation.SuppressLint;
import android.content.Context;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.ImageFormat;
import android.graphics.Rect;
import android.graphics.SurfaceTexture;
import android.graphics.YuvImage;
import android.hardware.Camera;
import android.media.Image;
import android.media.ImageReader;
import android.os.Bundle;
import android.os.SystemClock;
import android.text.TextUtils;
import android.util.Log;
import android.util.Size;
import android.view.Surface;
import android.view.TextureView;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.Toast;

import org.pytorch.Module;
import org.pytorch.demo.BaseModuleActivity;
import org.pytorch.demo.BitmapUtils;
import org.pytorch.demo.R;
import org.pytorch.demo.StatusBarUtils;
import org.pytorch.demo.Utils;

import androidx.annotation.Nullable;
import androidx.annotation.UiThread;
import androidx.annotation.WorkerThread;
import androidx.camera.core.CameraInfoUnavailableException;
import androidx.camera.core.CameraX;
import androidx.camera.core.ImageAnalysis;
import androidx.camera.core.ImageAnalysisConfig;
import androidx.camera.core.ImageProxy;
import androidx.camera.core.Preview;
import androidx.camera.core.PreviewConfig;
import androidx.core.app.ActivityCompat;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.nio.ByteBuffer;
import java.util.Locale;

public abstract class AbstractCameraXActivity<R> extends BaseModuleActivity {

  private static final String TAG = "AbstractCameraXActivity";
  public static final String INTENT_MODULE_ASSET_NAME = "INTENT_MODULE_ASSET_NAME";
  private Module mModule;
  private String mModuleAssetName;
    
  private static final int REQUEST_CODE_CAMERA_PERMISSION = 200;
  private static final String[] PERMISSIONS = {Manifest.permission.CAMERA};

  private long mLastAnalysisResultTime;

  protected abstract int getContentViewLayoutId();

  protected abstract TextureView getCameraPreviewTextureView();

  protected Context _context;

  protected abstract ImageView getPyTorchImageView();
  ImageView pyTorchImageView;

  RelativeLayout rlContainer;
  private boolean enableDraw = true;
  TextureView smallTextureView;

  @Override
  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    StatusBarUtils.setStatusBarOverlay(getWindow(), true);
    setContentView(getContentViewLayoutId());

    startBackgroundThread();

    pyTorchImageView = getPyTorchImageView();

    _context = this;

    rlContainer = findViewById(org.pytorch.demo.R.id.rlContainer);

    smallTextureView = new TextureView(this);
    rlContainer.addView(smallTextureView);

    smallTextureView.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
      @Override
      public void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
        enableDraw = true;
      }

      @Override
      public void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) {

      }

      @Override
      public boolean onSurfaceTextureDestroyed(SurfaceTexture surface) {
        enableDraw = false;
        return false;
      }

      @Override
      public void onSurfaceTextureUpdated(SurfaceTexture surface) {

      }
    });

    if (mModule == null) {
      final String moduleFileAbsoluteFilePath = new File(
              Utils.assetFilePath(this, getModuleAssetName())).getAbsolutePath();
      mModule = Module.load(moduleFileAbsoluteFilePath);
    }

    if (ActivityCompat.checkSelfPermission(this, Manifest.permission.CAMERA)
            != PackageManager.PERMISSION_GRANTED) {
      ActivityCompat.requestPermissions(
              this,
              PERMISSIONS,
              REQUEST_CODE_CAMERA_PERMISSION);
    } else {
      try {
        setupCameraX();
      } catch (CameraInfoUnavailableException e) {
        e.printStackTrace();
      }
    }
  }

  @Override
  public void onRequestPermissionsResult(
          int requestCode, String[] permissions, int[] grantResults) {
    if (requestCode == REQUEST_CODE_CAMERA_PERMISSION) {
      if (grantResults[0] == PackageManager.PERMISSION_DENIED) {
        Toast.makeText(
                this,
                "You can't use image classification example without granting CAMERA permission",
                Toast.LENGTH_LONG)
                .show();
        finish();
      } else {
        try {
          setupCameraX();
        } catch (CameraInfoUnavailableException e) {
          e.printStackTrace();
        }
      }
    }
  }


  Thread renderThread;
  Bitmap renderBitmap;
  private void renderThread(){

    if(renderThread == null){
      renderThread = new Thread(new Runnable() {
        @Override
        public void run() {
          while(true){

            if(renderBitmap == null){
              return;
            }

            pyTorchImageView.post(new Runnable() {
              @Override
              public void run() {
                pyTorchImageView.setImageBitmap(renderBitmap);

              }
            });

/*
            if(enableDraw){
              Canvas canvas = smallTextureView.lockCanvas();
              if(canvas != null){
                canvas.drawBitmap(renderBitmap,0,0,null);

                //canvas.drawColor(Color.YELLOW);
                smallTextureView.unlockCanvasAndPost(canvas);
              }
            }


 */

            try {
              Thread.sleep(50);
            } catch (InterruptedException e) {
              e.printStackTrace();
            }
          }
        }
      });
      renderThread.start();
    }

  }

  @SuppressLint("RestrictedApi")
  private void setupCameraX() throws CameraInfoUnavailableException {
    final TextureView textureView = getCameraPreviewTextureView();
    textureView.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
      @Override
      public void onSurfaceTextureAvailable(SurfaceTexture surfaceTexture, int i, int i1) {

      }

      @Override
      public void onSurfaceTextureSizeChanged(SurfaceTexture surfaceTexture, int i, int i1) {

      }

      @Override
      public boolean onSurfaceTextureDestroyed(SurfaceTexture surfaceTexture) {
        return false;
      }

      @Override
      public void onSurfaceTextureUpdated(SurfaceTexture surfaceTexture) {


        Bitmap bitmap = textureView.getBitmap();

        //Log.d("Cam", "b width" + bitmap.getWidth() + " height=" + bitmap.getHeight());


        int width = 300;
        int height = 300;
        int x = 90;
        int y = 170;
        int[] pixels = new int[width * height];

        //截图
        bitmap.getPixels(pixels, 0, width, x, y, width, height);
        Bitmap dest = Bitmap.createBitmap(pixels, width, height, bitmap.getConfig());

        final long pyTorchStartTime = SystemClock.elapsedRealtime();
        //图片模型转换
        Bitmap pyTorchBitmap = BitmapUtils.PytorchFunction(mModule, _context, dest, 200, 200);
        final long pyTorchDurationTime = SystemClock.elapsedRealtime()-pyTorchStartTime;

        final long mergeStartTime = SystemClock.elapsedRealtime();
        //转换后的图片缩放回原大小
        Bitmap scaleBitmap = BitmapUtils.scaleBitmap(pyTorchBitmap, width, height);

        //图片合并
        Bitmap mergeBitmap = BitmapUtils.mergeBitmap(bitmap, scaleBitmap, x, y);
        final long mergeDurationTime = SystemClock.elapsedRealtime()-mergeStartTime;

        Toast.makeText(AbstractCameraXActivity.this,"pyTorchDurationTime:"+pyTorchDurationTime+" mergeDurationTime:"+mergeDurationTime,Toast.LENGTH_LONG).show();

        renderBitmap = mergeBitmap;

        renderThread();
      }
    });


    final PreviewConfig previewConfig = new PreviewConfig.Builder()
            .setTargetResolution(new Size(480,640))
            .setLensFacing(CameraX.LensFacing.FRONT).build();
    final Preview preview = new Preview(previewConfig);

    preview.setOnPreviewOutputUpdateListener(output -> textureView.setSurfaceTexture(output.getSurfaceTexture())

    );


    final ImageAnalysisConfig imageAnalysisConfig =
            new ImageAnalysisConfig.Builder()
                    .setLensFacing(CameraX.LensFacing.FRONT)
                    //.setTargetResolution(new Size(340,640))
                    //.setCallbackHandler(mBackgroundHandler)
                    //.setTargetRotation(Surface.ROTATION_270)
                    .setImageReaderMode(ImageAnalysis.ImageReaderMode.ACQUIRE_LATEST_IMAGE)
                    .build();


    final ImageAnalysis imageAnalysis = new ImageAnalysis(imageAnalysisConfig);
    imageAnalysis.setAnalyzer(
        (image, rotationDegrees) -> {
          Image imageTemp = image.getImage();
          Log.d("imageTemp", "imageTemp width" + imageTemp.getWidth() + " height=" + imageTemp.getHeight());
          //Bitmap _bitmap = toBitmap(imageTemp);
        });

    CameraX.bindToLifecycle(this, imageAnalysis,preview);
  }


  private Bitmap toBitmap(Image image) {
    Image.Plane[] planes = image.getPlanes();
    ByteBuffer yBuffer = planes[0].getBuffer();
    ByteBuffer uBuffer = planes[1].getBuffer();
    ByteBuffer vBuffer = planes[2].getBuffer();

    int ySize = yBuffer.remaining();
    int uSize = uBuffer.remaining();
    int vSize = vBuffer.remaining();

    byte[] nv21 = new byte[ySize + uSize + vSize];
    //U and V are swapped
    yBuffer.get(nv21, 0, ySize);
    vBuffer.get(nv21, ySize, vSize);
    uBuffer.get(nv21, ySize + vSize, uSize);

    YuvImage yuvImage = new YuvImage(nv21, ImageFormat.NV21, image.getWidth(), image.getHeight(), null);
    ByteArrayOutputStream out = new ByteArrayOutputStream();
    yuvImage.compressToJpeg(new Rect(0, 0, yuvImage.getWidth(), yuvImage.getHeight()), 75, out);

    byte[] imageBytes = out.toByteArray();
    return BitmapFactory.decodeByteArray(imageBytes, 0, imageBytes.length);
  }

  protected String getModuleAssetName() {
    if (!TextUtils.isEmpty(mModuleAssetName)) {
      return mModuleAssetName;
    }
    final String moduleAssetNameFromIntent = getIntent().getStringExtra(INTENT_MODULE_ASSET_NAME);
    mModuleAssetName = !TextUtils.isEmpty(moduleAssetNameFromIntent)
            ? moduleAssetNameFromIntent
            : "traced-model.pt";

    return mModuleAssetName;
  }

  @Override
  protected String getInfoViewAdditionalText() {
    return getModuleAssetName();
  }

  @Override
  protected void onDestroy() {
    super.onDestroy();
    if (mModule != null) {
      mModule.destroy();
    }
  }

}
