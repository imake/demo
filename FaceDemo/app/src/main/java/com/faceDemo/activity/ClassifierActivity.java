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
import org.pytorch.Module;

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

    private String mPngAssetName;
    public static final String INTENT_PNG_ASSET_NAME = "INTENT_PNG_ASSET_NAME";

    private int pytorchUseSzie=200;

    public static String FILE_NAME = "douyin-4.pt";

    private TextView timeTextView;

    private Bitmap performBitmap=null;

    private int width;
    private int height;

    private String time_1;
    private String time_2;
    private String time_3;
    private String time_4;
    private String time_5;
    private String time_6;

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
        if (sensorEventUtil!= null) {

            if (mModule == null) {
                final String moduleFileAbsoluteFilePath = new File(
                        Utils.assetFilePath(this, getModuleAssetName())).getAbsolutePath();
                mModule = Module.load(moduleFileAbsoluteFilePath);
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
            List<FaceLandmarkInfo> landmarkInfos = new ArrayList<>();
            if (faceDetect.getFaceCount() > 0) {
                faceDetectInfos = faceDetect.getDetectInfos();
                landmarkInfos = faceDetect.landmark2d();
                landmarkInfos = fit(landmarkInfos);
            }
            Log.d("#####", "processImage: " + faceDetectInfos.size());
            if (faceDetectInfos != null && faceDetectInfos.size() > 0) {
                Rect[] face_rect = new Rect[faceDetectInfos.size()];

                List<List<TenginekitPoint>> face_landmarks = new ArrayList<>();
                for (int i = 0; i < faceDetectInfos.size(); i++) {
                    Rect rect = new Rect();
                    rect = faceDetectInfos.get(i).asRect();
                    face_rect[i] = rect;
                    face_landmarks.add(landmarkInfos.get(i).landmarks);
                }
                //Log.d("landmarkInfos", "processImage: count="+landmarkInfos.get(0).landmarks.size());

                FaceLandmarkInfo faceLandmarkInfo = landmarkInfos.get(0);
                List<TenginekitPoint> landmarks=faceLandmarkInfo.landmarks;
                TenginekitPoint leftEye=landmarks.get(101);
                TenginekitPoint rightEye=landmarks.get(117);
                TenginekitPoint middlePoint= new TenginekitPoint(((leftEye.X+rightEye.X)/2),((leftEye.Y+rightEye.Y)/2));
               // double eyeDis=Math.abs(Math.sqrt((leftEye.X-rightEye.X)*(leftEye.X-rightEye.X)+(leftEye.Y-rightEye.Y)*(leftEye.Y-rightEye.Y)));


                float dis = CheckDis(landmarks);
                dis = AverRectDis(dis);

                float bitmapWidth=dis*1.85f;
                float yUp=bitmapWidth*0.475f;
                float yDown=bitmapWidth*0.5f;

                int x=(int)(middlePoint.X-bitmapWidth/2);
                int y=(int)(middlePoint.Y-yUp);

                //Log.d("TenginekitPoint", "processImage: eyeDis="+dis+" bitmapWidth="+bitmapWidth+" yUp="+yUp+" yDown="+yDown+"  x="+ x+" y="+y+" middlePoint.x="+middlePoint.X+" middlePoint.y"+middlePoint.Y);

                //边界处理
                if (bitmapWidth>previewHeight)
                    bitmapWidth=previewHeight;
                width = (int) bitmapWidth;
                height = (int) bitmapWidth;
                if (x+width>previewHeight)
                    x=previewHeight-width;
                if (y+height>previewWidth)
                    y=previewWidth-height;
                if (x<0)
                    x=0;
                if (y<0)
                    y=0;

                //OpenCV方法
                //YUV 转 Mat
                long start_time_1 = System.nanoTime();
                Mat mat = new Mat(480*3/2,640,CvType.CV_8UC1);//,byteBuffer 640,480
                int re =  mat.put(0,0,mNV21Bytes);
                Mat bitmapMat  = new Mat();
                Imgproc.cvtColor(mat, bitmapMat , Imgproc.COLOR_YUV2RGB_NV21,3);//COLOR_YUV2BGR_I420

                Core.transpose(bitmapMat,bitmapMat);
                Core.flip(bitmapMat,bitmapMat,0);
                Core.flip(bitmapMat,bitmapMat,1);
                long end_time_1 = System.nanoTime();
                time_1=String.valueOf((end_time_1-start_time_1)/1000000.0);

                //旋转bitmapMat
                if (Math.abs(faceLandmarkInfo.roll)>=5) {
                    long start_time_2 = System.nanoTime();
                    Mat rotateMat = RotateMat(bitmapMat, new Point(middlePoint.X, middlePoint.Y), faceLandmarkInfo.roll);

                    //裁剪rotateMat
                    org.opencv.core.Rect rect=new org.opencv.core.Rect(x,y,width,height);
                    Mat scaleSmallMat=new Mat(rotateMat,rect);

                    //缩小rectMat
                    org.opencv.core.Size dsize = new org.opencv.core.Size(pytorchUseSzie, pytorchUseSzie); // 设置新图片的大小
                    Imgproc.resize(scaleSmallMat, scaleSmallMat,dsize);//调用Imgproc的Resize方法，进行图片缩放

                    scaleSmallMat.convertTo(scaleSmallMat,CvType.CV_32FC3,2.0/255,-1);
                    int size=(int)(scaleSmallMat.total()*3);
                    float[] data=new float[size];
                    scaleSmallMat.get(0,0,data);
                    long end_time_2 = System.nanoTime();
                    time_2=String.valueOf((end_time_2-start_time_2)/1000000.0);

                    //Pytorch模型处理
                    long start_time_3 = System.nanoTime();
                    final float[] pyTorchData = BitmapUtils.PytorchFunction(mModule, this, data,pytorchUseSzie, pytorchUseSzie);

                    Mat pyTorchScaleOriginalMat=new Mat(pytorchUseSzie,pytorchUseSzie,CvType.CV_32FC3);
                    pyTorchScaleOriginalMat.put(0,0,pyTorchData);

                    pyTorchScaleOriginalMat.convertTo(pyTorchScaleOriginalMat,CvType.CV_8UC3,255.0/2,255.0/2);
                    long end_time_3 = System.nanoTime();
                    time_3=String.valueOf((end_time_3-start_time_3)/1000000.0);

                    //读取透明模板图
                    long start_time_4 = System.nanoTime();
                    //读取透明模板图
                    Mat srclMat = LoadMaskMat();
                    long end_time_4 = System.nanoTime();
                    time_4=String.valueOf((end_time_4-start_time_4)/1000000.0);
                    //Log.d("Imgcodecs", "Imgcodecs: srclMat="+srclMat.size()+" channels="+srclMat.channels());

                    //转换后的图片放回原大小
                    long start_time_5 = System.nanoTime();
                    org.opencv.core.Size pyTorchOriginalSize = new org.opencv.core.Size(width, height); // 设置新图片的大小
                    Imgproc.resize(pyTorchScaleOriginalMat, pyTorchScaleOriginalMat,pyTorchOriginalSize);//调用Imgproc的Resize方法，进行图片缩放

                    // Log.d("scaleOriginalMat", "processImage: scaleOriginalMat.width="+scaleOriginalMat.width()+" height="+scaleOriginalMat.height());

                    //将模型转换后的头部图片合并到rotateMat上
                    org.opencv.core.Rect rec = new org.opencv.core.Rect(x,y, pyTorchScaleOriginalMat.cols(), pyTorchScaleOriginalMat.rows());
                    Mat mat1Sub=rotateMat.submat(rec);
                    pyTorchScaleOriginalMat.copyTo(mat1Sub);
                    long end_time_5 = System.nanoTime();
                    time_5=String.valueOf((end_time_5-start_time_5)/1000000.0);

                    //最后一步
                    //在rotateMat上创建mask掩码
                    long start_time_6 = System.nanoTime();
                    Mat mask = Mat.zeros(rotateMat.rows(), rotateMat.cols(), CvType.CV_8UC3);
                    int cx = x;
                    int cy = y;
                    org.opencv.core.Rect maskRect=new org.opencv.core.Rect(cx,cy,width,height);
                    Mat mat1SubA=mask.submat(maskRect);
                    srclMat.copyTo(mat1SubA);
                    //Log.d("mask", "mask: mask="+mask.size()+" channels="+mask.channels());

                    //对mask掩码旋转
                    Mat rotateMask=RotateMat(mask,new Point(middlePoint.X,middlePoint.Y),-faceLandmarkInfo.roll);

                    //在把rotateMat旋转回正
                    Mat ratoteOriginalMat=RotateMat(rotateMat,new Point(middlePoint.X,middlePoint.Y),-faceLandmarkInfo.roll);

                    //融合
                    Mat blendMat = BlendMat(ratoteOriginalMat,bitmapMat,rotateMask);

                    long end_time_6 = System.nanoTime();
                    time_6=String.valueOf((end_time_6-start_time_6)/1000000.0);
                    //测试
                    //final Bitmap TTT=Bitmap.createBitmap(allMat.cols(),allMat.rows(), Bitmap.Config.ARGB_8888);
                    //org.opencv.android.Utils.matToBitmap(allMat,TTT);

                    performBitmap=Bitmap.createBitmap(blendMat.cols(),blendMat.rows(), Bitmap.Config.ARGB_8888);
                    org.opencv.android.Utils.matToBitmap(blendMat,performBitmap);
                }
                else
                {
                    //裁剪rotateMat
                    org.opencv.core.Rect rect=new org.opencv.core.Rect(x,y,width,height);
                    Mat rectMat=new Mat(bitmapMat,rect);

                    //缩小rectMat
                    Mat scaleSmallMat=new Mat();
                    org.opencv.core.Size dsize = new org.opencv.core.Size(pytorchUseSzie, pytorchUseSzie); // 设置新图片的大小
                    Imgproc.resize(rectMat, scaleSmallMat,dsize);//调用Imgproc的Resize方法，进行图片缩放

                    scaleSmallMat.convertTo(scaleSmallMat,CvType.CV_32FC3,2.0/255,-1);
                    int size=(int)(scaleSmallMat.total()*3);
                    float[] data=new float[size];
                    scaleSmallMat.get(0,0,data);

                    //Pytorch模型处理
                    final float[] pyTorchData = BitmapUtils.PytorchFunction(mModule, this, data,pytorchUseSzie, pytorchUseSzie);

                    Mat pyTorchScaleOriginalMat=new Mat(pytorchUseSzie,pytorchUseSzie,CvType.CV_32FC3);
                    pyTorchScaleOriginalMat.put(0,0,pyTorchData);

                    pyTorchScaleOriginalMat.convertTo(pyTorchScaleOriginalMat,CvType.CV_8UC3,255.0/2,255.0/2);

                    //读取透明模板图
                    Mat srclMat = LoadMaskMat();

                    //转换后的图片放回原大小
                    org.opencv.core.Size pyTorchOriginalSize = new org.opencv.core.Size(width, height); // 设置新图片的大小
                    Imgproc.resize(pyTorchScaleOriginalMat, pyTorchScaleOriginalMat,pyTorchOriginalSize);//调用Imgproc的Resize方法，进行图片缩放

                    //融合
                    Mat blendMat = BlendMat(pyTorchScaleOriginalMat,rectMat,srclMat);

                    //将模型转换后的头部图片合并到bitmapMat上
                    org.opencv.core.Rect rec = new org.opencv.core.Rect(x,y, blendMat.cols(), blendMat.rows());
                    Mat mat1Sub=bitmapMat.submat(rec);
                    blendMat.copyTo(mat1Sub);

                    performBitmap=Bitmap.createBitmap(bitmapMat.cols(),bitmapMat.rows(), Bitmap.Config.ARGB_8888);
                    org.opencv.android.Utils.matToBitmap(bitmapMat,performBitmap);
                }

                long end_time_7 = System.nanoTime();
                String time_7=String.valueOf((end_time_7-start_time_1)/1000000.0);

                if (timeTextView==null)
                {
                    timeTextView=findViewById(R.id.timeText);
                }
                timeTextView.setText("NV21转mat："+time_1+"\n裁剪mat提取dada："+time_2+"\n模型处理："+time_3+"\n读取透明模板图:"+time_4+"\n生成图贴回去："+time_5+"\n两张图片融合："+time_6+"\n总时长："+time_7);

                if (bigBitmapView==null)
                {
                    bigBitmapView = findViewById(R.id.bigBitmapview);
                }

                if (bigBgView==null)
                {
                    bigBgView = findViewById(R.id.bigBgview);
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

    private Mat LoadMaskMat()
    {
        //读取透明模板图
        final String pngFileAbsoluteFilePath = new File(
                Utils.assetFilePath(this, getPngAssetName())).getAbsolutePath();
        Mat srclMat = Imgcodecs.imread(pngFileAbsoluteFilePath,Imgcodecs.IMREAD_UNCHANGED);
        org.opencv.core.Size srcSize = new org.opencv.core.Size(width, width); // 设置新图片的大小
        Imgproc.resize(srclMat, srclMat,srcSize);//调用Imgproc的Resize方法，进行图片缩放
        //Log.d("Imgcodecs", "Imgcodecs: srclMat="+srclMat.size()+" channels="+srclMat.channels());
        return srclMat;
    }

    private Mat BlendMat(Mat mat_1,Mat mat_2,Mat mask)
    {
        mask.convertTo(mask,CvType.CV_32FC3,1.0/255,0);
        mat_1.convertTo(mat_1,CvType.CV_32FC3,1.0/255,0);
        Mat mat9999=new Mat(mat_1.size(),CvType.CV_32FC3);
        Core.multiply(mask,mat_1,mat9999);

        Mat matInvMask=new Mat(mat_2.size(), CvType.CV_32FC3,new Scalar(1.0,1.0,1.0));
        Mat matInvMask_0 = new Mat(mat_2.size(), CvType.CV_32FC3);
        Core.subtract(matInvMask,mask,matInvMask_0);
        mat_2.convertTo(mat_2,CvType.CV_32FC3,1.0/255,0);
        Mat mat8888=new Mat(mat_2.size(),CvType.CV_32FC3);
        Core.multiply(matInvMask_0,mat_2,mat8888);

        Mat allMat=new Mat(mat_2.size(),CvType.CV_32FC3);
        Core.add(mat9999,mat8888,allMat);
        allMat.convertTo(allMat,CvType.CV_8UC3,255.0,0);

        return allMat;
    }

    protected Bitmap BytetoBitmap(byte[] mNv21Bytes)
    {
        Bitmap bitmap=null;

        bitmap=com.tenginekit.Image.convertCameraYUVData(mNv21Bytes,previewWidth,previewHeight,previewHeight,previewWidth,-90,true);
        Log.d("bitmap", "BytetoBitmap: bitmap.getWidth()="+bitmap.getWidth()+" bitmap.getHeight()="+bitmap.getHeight());

        return  bitmap;
    }

    protected String getModuleAssetName() {
        if (!TextUtils.isEmpty(mModuleAssetName)) {
            return mModuleAssetName;
        }
        final String moduleAssetNameFromIntent = FILE_NAME;
        mModuleAssetName = !TextUtils.isEmpty(moduleAssetNameFromIntent)
                ? moduleAssetNameFromIntent
                : "douyin-4.pt";

        return mModuleAssetName;
    }

    protected String getPngAssetName() {
        if (!TextUtils.isEmpty(mPngAssetName)) {
            return mPngAssetName;
        }
        final String pngAssetNameFromIntent = "standardMASK.jpg";
        mPngAssetName = !TextUtils.isEmpty(pngAssetNameFromIntent)
                ? pngAssetNameFromIntent
                : "standardMASK.jpg";

        return mPngAssetName;
    }

    private List<FaceLandmarkInfo> lastList;
    private static final float DISTANCE = 3F;

    //比较基准
    private List<LinkedList> averagePositionList;
    //用于计算比较基准的特征点：脸的上下左右，共取4个点，下嘴唇的中间取上下2个点，眉毛的中间各取1个点，
    //眼睛的上部中间各取3个点（眨眼时特征点变动范围小，故多取几个点），共14个点
    private final int[] markPoint = new int[]{18, 37, 208, 200, 104, 105, 106, 122, 121, 120, 36, 0, 73, 89};
    //计算过去10个frame的均值作为比较基准
    private static final int averageFrameNum = 10;

    private List<FaceLandmarkInfo> fit(List<FaceLandmarkInfo> runningList){

        if (Utils.isEmpty(runningList)){
            lastList = null;
            averagePositionList = null;
            return runningList;
        }

        if (Utils.isEmpty(lastList)){
            lastList = runningList;

            //初始化比较基准：过去10个frame的特征点
            averagePositionList = new ArrayList<LinkedList>();
            for (int i=0;i<markPoint.length;i++){
                LinkedList<TenginekitPoint> averageList = new LinkedList<TenginekitPoint>();
                for (int j=0;j<averageFrameNum;j++){
                    averageList.addLast(runningList.get(0).landmarks.get(markPoint[i]));
                }
                averagePositionList.add(averageList);
            }

            return runningList;
        }

        List<TenginekitPoint> runningLandmarks= runningList.get(0).landmarks;

        for (int markIndex=0; markIndex<markPoint.length; markIndex++) {

            //计算每个特征点过去10个frame的均值
            LinkedList<TenginekitPoint> averageList = averagePositionList.get(markIndex);
            float averageX = 0.0f;
            float averageY = 0.0f;
            for (int k=0;k<averageFrameNum;k++){
                averageX += averageList.get(k).X;
                averageY += averageList.get(k).Y;
            }
            averageX = averageX / averageFrameNum;
            averageY = averageY / averageFrameNum;

            //如果某一个特征点与比较基准距离较大，则更新last FaceLandmarkInfo
            TenginekitPoint runningMark = runningLandmarks.get(markPoint[markIndex]);
            float diffX = Math.abs(averageX - runningMark.X);
            float diffY = Math.abs(averageY - runningMark.Y);

            if (diffX > DISTANCE || diffY > DISTANCE) {
                lastList = runningList; //更新last FaceLandmarkInfo

                //更新比较基准
                for (int k=0; k<markPoint.length; k++) {
                    averagePositionList.get(k).addLast(runningLandmarks.get(markPoint[k]));
                    averagePositionList.get(k).removeFirst();
                }
                return runningList;
            }
        }

        //更新比较基准
        for (int k=0; k<markPoint.length; k++) {
            averagePositionList.get(k).addLast(runningLandmarks.get(markPoint[k]));
            averagePositionList.get(k).removeFirst();
        }
        return lastList;
    }

    private LinkedList<Float> averRectDis;
    float lastDis = 0.0f;
    public float AverRectDis(float rectDis)
    {
        if (lastDis == 0.0f)
        {
            lastDis=rectDis;
            averRectDis = new LinkedList<>();
            for (int i=0;i<averageFrameNum;i++)
            {
                averRectDis.addLast(rectDis);
            }
            return rectDis;
        }

        float sum=0;
        for (int j=0;j<averageFrameNum;j++)
        {
            sum += averRectDis.get(j);
        }
        float aver=sum/averageFrameNum;
        if (Math.abs(rectDis-aver)>4)
        {
            lastDis=rectDis;
        }
        averRectDis.addLast(rectDis);
        averRectDis.removeFirst();
        return lastDis;
    }

    public Bitmap BitmapMatricRotate(Bitmap bitmap,float angle,float x,float y)
    {
        Bitmap dest = Bitmap.createBitmap(bitmap.getWidth(),bitmap.getHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(dest);
        int width = bitmap.getWidth();
        int height = bitmap.getHeight(); // 创建新的图片
        Matrix matrix = new Matrix(); //旋转图片 动作
        matrix.postScale(1f,1f);
        matrix.postRotate(-angle,x,y);        //旋转角度

        Bitmap resizedBitmap = Bitmap.createBitmap(bitmap);
        canvas.setMatrix(matrix);
        canvas.drawBitmap(resizedBitmap,0,0,null);

        Log.d("resizedBitmap", "BitmapMatricRotate: resizedBitmap.height="+dest.getHeight()+" resizedBitmap.width="+dest.getWidth()+" width="+width+" height="+height);

        return dest;
    }

    public Bitmap GetResizedBitmap(Bitmap bitmap)
    {
        Bitmap dest = Bitmap.createBitmap(960,1280, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(dest);
        Bitmap resizedBitmap = Bitmap.createBitmap(bitmap);
        Paint paint=new Paint();
        paint.setColor(Color.YELLOW);
        canvas.drawBitmap(resizedBitmap,240,320,paint);
        return dest;
    }

    private float CheckDis(List<TenginekitPoint> points)
    {
        float min_X = points.get(0).X;
        float max_X = min_X;

        for (int index=1;index<points.size();index++){
            float current_X = points.get(index).X;
            if(current_X > max_X){
                max_X = current_X;
            }
            if(current_X < min_X){
                min_X = current_X;
            }
        }
        return max_X - min_X;
    }

    public Mat RotateMat(Mat _mat,Point _point,float _angle)
    {
        Mat rotateMat=new Mat();
        Mat matric = Imgproc.getRotationMatrix2D(_point,_angle,1);
        org.opencv.core.Size size=new org.opencv.core.Size(_mat.cols(),_mat.rows());
        Imgproc.warpAffine(_mat,rotateMat,matric,size);
        return rotateMat;
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