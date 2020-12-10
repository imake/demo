package com.faceDemo.activity;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.YuvImage;
import android.os.Build;
import android.os.Bundle;
import android.text.TextUtils;
import android.util.Log;
import android.util.Size;
import android.view.TextureView;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.RequiresApi;

import com.faceDemo.BitmapUtils;
import com.faceDemo.R;
import com.faceDemo.camera.CameraEngine;
import com.faceDemo.currencyview.OverlayView;
import com.faceDemo.encoder.BitmapEncoder;
import com.faceDemo.encoder.CircleEncoder;
import com.faceDemo.encoder.EncoderBus;
import com.faceDemo.encoder.RectEncoder;
import com.faceDemo.utils.Utils;
import com.tenginekit.KitCore;
import com.tenginekit.face.Face;
import com.tenginekit.face.FaceDetectInfo;
import com.tenginekit.face.FaceLandmarkInfo;
import com.tenginekit.model.TenginekitPoint;

import org.opencv.android.BaseLoaderCallback;
import org.opencv.android.LoaderCallbackInterface;
import org.opencv.android.OpenCVLoader;
import org.opencv.core.Core;
import org.opencv.core.CvType;
import org.opencv.core.Mat;
import org.opencv.core.Point;
import org.opencv.core.Scalar;
import org.opencv.imgcodecs.Imgcodecs;
import org.opencv.imgproc.Imgproc;
import org.pytorch.IValue;
import org.pytorch.Module;
import org.pytorch.Tensor;

import java.io.File;
import java.nio.Buffer;
import java.nio.ByteBuffer;
import java.nio.FloatBuffer;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;


public class ClassifierActivity extends CameraActivity {

    private static final String TAG = "ClassifierActivity";

    private OverlayView trackingOverlay;
    public ImageView bigBitmapView;

    public ImageView bigBgView;

    public ImageView mergeView;

    private Module mModule;
    private String mModuleAssetName;
    public static final String INTENT_MODULE_ASSET_NAME = "INTENT_MODULE_ASSET_NAME";

    private String mPngAssetName = "background.jpg";
    public static final String INTENT_PNG_ASSET_NAME = "INTENT_PNG_ASSET_NAME";

    public static String FILE_NAME = "humanMatting-4.pt";

    public static int threshold = 128;

    private TextView timeTextView;

    private Bitmap performBitmap=null;

    private String time_1;
    private String time_2;
    private String time_3;
    private String time_4;


    private Mat mImageMat = null;
    private Mat bitmapMat = null;
    private Mat smallBitmapMat = null;
    private Mat smallBitmapMatFP = null;
    private int size = 57600;
    private long[] shape = new long[]{1,160,120,3};
    private Tensor predictionTensor = null;
    private Tensor inputTensor = null;
    org.opencv.core.Size dsize = new org.opencv.core.Size(120, 160);
    org.opencv.core.Size osize = new org.opencv.core.Size(480, 640);
    private Mat originalSizeMask = null;
    private Mat backgroundMat = null;
    private Mat runningBackgroundMat = null;
    private Mat originalSizeMask8U = null;
    private float[] data = null;
    private float[] pytorchData = null;
    private Mat pytorchScaleOriginalMat = null;



    @Override
    protected int getLayoutId() {
        return R.layout.camera_connection_fragment;
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    @Override
    protected Size getDesiredPreviewFrameSize() {
        return new Size(640, 480);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

    }

    public void Registe() {
        /**
         * canvas 绘制人脸框，人脸关键点
         * */
        EncoderBus.GetInstance().Registe(new BitmapEncoder(this));

        //绘制人脸识别点
        EncoderBus.GetInstance().Registe(new CircleEncoder(this));

        //绘制人脸框
        EncoderBus.GetInstance().Registe(new RectEncoder(this));
    }


    @Override
    public void onPreviewSizeChosen(final Size size) {
        Registe();
        EncoderBus.GetInstance().onSetFrameConfiguration(previewHeight, previewWidth);


        trackingOverlay = (OverlayView) findViewById(R.id.facing_overlay);
        trackingOverlay.addCallback(new OverlayView.DrawCallback() {
            @Override
            public void drawCallback(final Canvas canvas) {
                EncoderBus.GetInstance().onDraw(canvas);
            }
        });
    }

    @Override
    protected void processImage() {
        long start_time_3 = System.nanoTime();
        if (sensorEventUtil!= null) {

            if (mModule == null) {
                final String moduleFileAbsoluteFilePath = new File(
                        Utils.assetFilePath(this, getModuleAssetName())).getAbsolutePath();
                mModule = Module.load(moduleFileAbsoluteFilePath);
            }

            if(backgroundMat == null){
                final String pngFileAbsoluteFilePath = new File(Utils.assetFilePath(this, mPngAssetName)).getAbsolutePath();
                backgroundMat = Imgcodecs.imread(pngFileAbsoluteFilePath,Imgcodecs.IMREAD_UNCHANGED);
            }

            getCameraBytes();
            int degree = CameraEngine.getInstance().getCameraOrientation(sensorEventUtil.orientation);
            /**
             * 设置旋转角
             */
            KitCore.Camera.setRotation(degree - 90, false, (int) CameraActivity.ScreenWidth, (int) CameraActivity.ScreenHeight);

            /**
             * 获取人脸信息
             */
            Face.FaceDetect faceDetect = Face.detect(mNV21Bytes);
            List<FaceDetectInfo> faceDetectInfos = new ArrayList<>();
            //List<FaceLandmarkInfo> landmarkInfos = new ArrayList<>();
            if (faceDetect.getFaceCount() > 0) {
                faceDetectInfos = faceDetect.getDetectInfos();
                //landmarkInfos = faceDetect.landmark2d();
                //landmarkInfos = fit(landmarkInfos);
            }
            Log.d("#####", "processImage: " + faceDetectInfos.size());
            if (faceDetectInfos != null && faceDetectInfos.size() > 0) {
                long start_time_2 = System.nanoTime();
                if(mImageMat == null){
                    mImageMat = new Mat(480*3/2,640,CvType.CV_8UC1);//,byteBuffer 640,480
                }
                int re =  mImageMat.put(0,0,mNV21Bytes);
                if(bitmapMat == null){
                    bitmapMat  = new Mat();
                }
                Imgproc.cvtColor(mImageMat, bitmapMat , Imgproc.COLOR_YUV2RGB_NV21,3);//COLOR_YUV2BGR_I420


                Core.transpose(bitmapMat,bitmapMat);
                Core.flip(bitmapMat,bitmapMat,0);
                Core.flip(bitmapMat,bitmapMat,1);


                if(smallBitmapMat == null){
                    smallBitmapMat = new Mat();
                }
                 //缩小到160*120
                Imgproc.resize(bitmapMat, smallBitmapMat,dsize);

                if(smallBitmapMatFP == null){
                    smallBitmapMatFP = new Mat();
                }
                smallBitmapMat.convertTo(smallBitmapMatFP,CvType.CV_32FC3,1.0/255);

                if(data==null){
                    data=new float[size];
                }
                smallBitmapMatFP.get(0,0,data);

                //Pytorch模型处理
                inputTensor =Tensor.fromBlob(data, shape);
                if(predictionTensor == null){
                    float[] zero_data = new float[19200];
                    for(int i=0;i<zero_data.length;i++){
                        zero_data[i] = 0.0f;
                    }
                    predictionTensor = Tensor.fromBlob(zero_data, new long[]{1,160,120,1});
                }
                long start_time_1 = System.nanoTime();
                predictionTensor = mModule.forward(IValue.from(inputTensor), IValue.from(predictionTensor)).toTensor();
                long end_time_1 = System.nanoTime();
                time_1=String.valueOf((end_time_1-start_time_1)/1000000);

                pytorchData = predictionTensor.getDataAsFloatArray();

                if(pytorchScaleOriginalMat == null) {
                    pytorchScaleOriginalMat = new Mat(160,120,CvType.CV_32FC1);
                }
                pytorchScaleOriginalMat.put(0,0,pytorchData);
                if(originalSizeMask == null){
                    originalSizeMask = new Mat();
                }
                Imgproc.resize(pytorchScaleOriginalMat, originalSizeMask,osize);
                if(originalSizeMask8U==null){
                    originalSizeMask8U = new Mat();
                }
                originalSizeMask.convertTo(originalSizeMask8U,CvType.CV_8UC1,255,-threshold);

                runningBackgroundMat = backgroundMat.clone();
                bitmapMat.copyTo(runningBackgroundMat, originalSizeMask8U);
                performBitmap=Bitmap.createBitmap(480,640, Bitmap.Config.ARGB_8888);
                org.opencv.android.Utils.matToBitmap(runningBackgroundMat,performBitmap);
                long end_time_2 = System.nanoTime();
                time_2=String.valueOf((end_time_2-start_time_2-(end_time_1-start_time_1))/1000000);
                time_3=String.valueOf((start_time_2-start_time_3)/1000000);
                time_4=String.valueOf((end_time_2-start_time_3)/1000000);

                if (timeTextView==null)
                {
                    timeTextView=findViewById(R.id.timeText);
                }
                timeTextView.setText("模型耗时:\n"+time_1+"ms"+"\n预处理耗时:\n"+time_2+"ms"+"\n人脸检测耗时:\n"+time_3+"ms"+"\n每帧总耗时:\n"+time_4+"ms");


                if (bigBgView==null)
                {
                    bigBgView = findViewById(R.id.bigBgview);
                }
                if (bigBitmapView==null)
                {
                    bigBitmapView = findViewById(R.id.bigBitmapview);
                }
                if (mergeView==null)
                {
                    mergeView = findViewById(R.id.mergeview);
                }

                    bigBitmapView.post(new Runnable() {
                        @Override
                        public void run() {
                            //bigBitmapView.setImageBitmap(BBB);
                        }
                    });

                bigBgView.post(new Runnable() {
                    @Override
                    public void run() { bigBgView.setImageBitmap((performBitmap));
                    }
                });

                mergeView.post(new Runnable() {
                    @Override
                    public void run() {
                        //mergeView.setImageBitmap((newBitmap));
                    }
                });


                //EncoderBus.GetInstance().onProcessResults(face_rect);
                //EncoderBus.GetInstance().onProcessResults(face_landmarks);
            }else{
                performBitmap=Bitmap.createBitmap(480,640, Bitmap.Config.ARGB_8888);
                org.opencv.android.Utils.matToBitmap(backgroundMat,performBitmap);
                predictionTensor = null;

                if (timeTextView==null)
                {
                    timeTextView=findViewById(R.id.timeText);
                }
                timeTextView.setText("未识别到人脸！");


                if (bigBgView==null)
                {
                    bigBgView = findViewById(R.id.bigBgview);
                }
                if (bigBitmapView==null)
                {
                    bigBitmapView = findViewById(R.id.bigBitmapview);
                }
                if (mergeView==null)
                {
                    mergeView = findViewById(R.id.mergeview);
                }

                bigBitmapView.post(new Runnable() {
                    @Override
                    public void run() {
                        //bigBitmapView.setImageBitmap(BBB);
                    }
                });

                bigBgView.post(new Runnable() {
                    @Override
                    public void run() { bigBgView.setImageBitmap((performBitmap));
                    }
                });

                mergeView.post(new Runnable() {
                    @Override
                    public void run() {
                        //mergeView.setImageBitmap((newBitmap));
                    }
                });
            }
        }

        runInBackground(new Runnable() {
            @Override
            public void run() {
                readyForNextImage();
                if (trackingOverlay!=null) {
                    //trackingOverlay.postInvalidate();
                }
            }
        });
    }

    protected String getModuleAssetName() {
        if (!TextUtils.isEmpty(mModuleAssetName)) {
            return mModuleAssetName;
        }
        final String moduleAssetNameFromIntent = FILE_NAME;
        mModuleAssetName = !TextUtils.isEmpty(moduleAssetNameFromIntent)
                ? moduleAssetNameFromIntent
                : "humanMatting-4.pt";

        return mModuleAssetName;
    }

    @Override
    public void onResume() {
        super.onResume();
        if (!OpenCVLoader.initDebug()) {
            Log.d(TAG, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
            OpenCVLoader.initAsync(OpenCVLoader.OPENCV_VERSION_3_0_0, this, mLoaderCallback);
        } else {
            Log.d(TAG, "OpenCV library found inside package. Using it!");
            mLoaderCallback.onManagerConnected(LoaderCallbackInterface.SUCCESS);
        }
    }

    //openCV4Android 需要加载用到
    private BaseLoaderCallback mLoaderCallback = new BaseLoaderCallback(this) {
        @Override
        public void onManagerConnected(int status) {
            switch (status) {
                case LoaderCallbackInterface.SUCCESS: {
                    Log.i(TAG, "OpenCV loaded successfully");
//                    mOpenCvCameraView.enableView();
//                    mOpenCvCameraView.setOnTouchListener(ColorBlobDetectionActivity.this);
                }
                break;
                default: {
                    super.onManagerConnected(status);
                }
                break;
            }
        }
    };

}