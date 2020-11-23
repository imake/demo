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
import android.widget.ImageView;

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
import org.opencv.imgproc.Imgproc;
import org.pytorch.Module;

import java.io.File;
import java.nio.Buffer;
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

    private int pytorchUseSzie=200;

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
                Log.d("landmarkInfos", "processImage: count="+landmarkInfos.get(0).landmarks.size());

                FaceLandmarkInfo faceLandmarkInfo = landmarkInfos.get(0);
                List<TenginekitPoint> landmarks=faceLandmarkInfo.landmarks;
                TenginekitPoint leftEye=landmarks.get(101);
                TenginekitPoint rightEye=landmarks.get(117);
                TenginekitPoint middlePoint= new TenginekitPoint(((leftEye.X+rightEye.X)/2),((leftEye.Y+rightEye.Y)/2));
               // double eyeDis=Math.abs(Math.sqrt((leftEye.X-rightEye.X)*(leftEye.X-rightEye.X)+(leftEye.Y-rightEye.Y)*(leftEye.Y-rightEye.Y)));

                //int dis=face_rect[0].right-face_rect[0].left;
                //dis=AverRectDis(dis);

                float dis = CheckDis(landmarks);
                dis = AverRectDis(dis);

                float bitmapWidth=dis*1.75f;
                float yUp=bitmapWidth*0.475f;
                float yDown=bitmapWidth*0.5f;

                int x=(int)(middlePoint.X-bitmapWidth/2);
                int y=(int)(middlePoint.Y-yUp);

                //Log.d("TenginekitPoint", "processImage: eyeDis="+dis+" bitmapWidth="+bitmapWidth+" yUp="+yUp+" yDown="+yDown+"  x="+ x+" y="+y+" middlePoint.x="+middlePoint.X+" middlePoint.y"+middlePoint.Y);

                //OpenCV方法
                //mNV21Bytes转Mat
                final Bitmap bitmap = BytetoBitmap(mNV21Bytes);

                //边界处理
                if (bitmapWidth>bitmap.getWidth())
                    bitmapWidth=bitmap.getWidth();
                int width = (int) bitmapWidth;
                int height = (int) bitmapWidth;
                if (x+width>bitmap.getWidth())
                    x=bitmap.getWidth()-width;
                if (y+height>bitmap.getHeight())
                    y=bitmap.getHeight()-height;
                if (x<0)
                    x=0;
                if (y<0)
                    y=0;

                Mat bitmapMat =new Mat();
                org.opencv.android.Utils.bitmapToMat(bitmap,bitmapMat);

                //旋转bitmapMat
                Mat rotateMat=RotateMat(bitmapMat,new Point(middlePoint.X,middlePoint.Y),faceLandmarkInfo.roll);

                //裁剪rotateMat
                org.opencv.core.Rect rect=new org.opencv.core.Rect(x,y,width,height);
                Mat rectMat=new Mat(rotateMat,rect);

                //缩小rectMat
                org.opencv.core.Size dsize = new org.opencv.core.Size(pytorchUseSzie, pytorchUseSzie); // 设置新图片的大小
                Mat scaleSmallMat = new Mat(dsize, CvType.CV_16S);// 创建一个新的Mat（opencv的矩阵数据类型）
                Imgproc.resize(rectMat, scaleSmallMat,dsize);//调用Imgproc的Resize方法，进行图片缩放

                //Log.d("scaleMat", "processImage:scaleMat width="+scaleSmallMat.rows()+" height="+scaleSmallMat.cols());

                //final Bitmap map=Bitmap.createBitmap(scaleSmallMat.cols(),scaleSmallMat.rows(), Bitmap.Config.ARGB_8888);
               // org.opencv.android.Utils.matToBitmap(scaleSmallMat,map);

                //图片模型转换
                List<Mat> mv = new ArrayList<Mat>();// 分离出来的彩色通道数据
                Core.split(scaleSmallMat, mv);// 分离色彩通道
                //List<Mat> mv3=new ArrayList<Mat>();
//                mv3.add(mv.get(2));
//                mv3.add(mv.get(1));
//                mv3.add(mv.get(0));
//                Mat dest=new Mat();
//                Core.merge(mv3, dest);// 合并split()方法分离出来的彩色通道数据
//
//                dest.convertTo(dest,CvType.CV_32F,2.0/255,-1);
//                int size=(int)(dest.total()*3);
//                float[] data=new float[size];
//                dest.get(0,0,data);

                Mat a1=mv.get(0);
                Mat a2=mv.get(1);
                Mat a3=mv.get(2);
                a1.convertTo(a1,CvType.CV_32F,2.0/255,-1);
                a2.convertTo(a2,CvType.CV_32F,2.0/255,-1);
                a3.convertTo(a3,CvType.CV_32F,2.0/255,-1);
                int size1=(int)(a1.total());
                float[] data1=new float[size1];
                a1.get(0,0,data1);
                int size2=(int)(a1.total());
                float[] data2=new float[size2];
                a2.get(0,0,data2);
                int size3=(int)(a2.total());
                float[] data3=new float[size3];
                a3.get(0,0,data3);

                float[] data4=new float[size1+size2+size3];

                for (int i=0;i<data1.length;i++)
                {
                    data4[i]=data1[i];
                }
                for (int i=0;i<data2.length;i++)
                {
                    data4[i+data1.length]=data2[i];
                }
                for (int i=0;i<data3.length;i++)
                {
                    data4[i+data1.length+data2.length]=data3[i];
                }


                //Log.d("data", "processImage: data="+data[2]);
                //final Bitmap pyTorchBitmap = BitmapUtils.PytorchFunction(mModule, this, map, data,200, 200);
                final float[] pyTorchData = BitmapUtils.PytorchFunction(mModule, this, data4,pytorchUseSzie, pytorchUseSzie);
                Log.d("pyTorchData", "processImage: pyTorchData="+pyTorchData.length);

                int dataSize=pytorchUseSzie*pytorchUseSzie;
                float[] data11=new float[dataSize];
                float[] data22=new float[dataSize];
                float[] data33=new float[dataSize];

                float[] data44=new float[dataSize*3];

                for (int i=0;i<dataSize;i++)
                {
                    int j=i*3;
                    data44[j]=pyTorchData[i];
                    data44[j+1]=pyTorchData[i+dataSize];
                    data44[j+2]=pyTorchData[i+dataSize*2];
                }

                Mat pyTorchMat=new Mat(pytorchUseSzie,pytorchUseSzie,CvType.CV_32FC3);
                pyTorchMat.put(0,0,data44);

                pyTorchMat.convertTo(pyTorchMat,CvType.CV_8U,255.0/2,255.0/2);

                List<Mat> mv1 = new ArrayList<Mat>();// 分离出来的彩色通道数据
                Core.split(pyTorchMat, mv1);// 分离色彩通道
                mv1.add(mv.get(3));
                Mat dest1=new Mat();
                //Log.d("dest1", "processImage: mv="+mv1.size()+" pyTorchMat="+dest1.channels());
                Core.merge(mv1, dest1);// 合并split()方法分离出来的彩色通道数据
                Log.d("dest1", "processImage: mv="+mv1.size()+" pyTorchMat="+dest1.channels());


                //转换后的图片放回原大小
//                Mat pyTorchMat = new Mat();
//                org.opencv.android.Utils.bitmapToMat(pyTorchBitmap,pyTorchMat);
                org.opencv.core.Size pyTorchOriginalSize = new org.opencv.core.Size(width, height); // 设置新图片的大小
                Mat pyTorchScaleOriginalMat = new Mat(pyTorchOriginalSize,CvType.CV_16S);
                Imgproc.resize(dest1, pyTorchScaleOriginalMat,pyTorchOriginalSize);//调用Imgproc的Resize方法，进行图片缩放

               // Log.d("scaleOriginalMat", "processImage: scaleOriginalMat.width="+scaleOriginalMat.width()+" height="+scaleOriginalMat.height());

                //将模型转换后的头部图片合并到rotateMat上
                org.opencv.core.Rect rec = new org.opencv.core.Rect(x,y, pyTorchScaleOriginalMat.cols(), pyTorchScaleOriginalMat.rows());
                Mat mat1Sub=rotateMat.submat(rec);
                pyTorchScaleOriginalMat.copyTo(mat1Sub);

                //最后一步
                //在rotateMat上创建mask掩码
                Mat mask = Mat.zeros(rotateMat.rows(), rotateMat.cols(), CvType.CV_8UC1);
                int cx = x;
                int cy = y;
                org.opencv.core.Rect maskRect=new org.opencv.core.Rect(cx,cy,width,height);
                Imgproc.rectangle(mask, maskRect, new Scalar(90,95,234), -1, 8);

                //对mask掩码旋转
                Mat rotateMask=RotateMat(mask,new Point(middlePoint.X,middlePoint.Y),-faceLandmarkInfo.roll);

                //在把rotateMat旋转回正
                Mat ratoteOriginalMat=RotateMat(rotateMat,new Point(middlePoint.X,middlePoint.Y),-faceLandmarkInfo.roll);


                //测试
                //final Bitmap TTT=Bitmap.createBitmap(dest1.cols(),dest1.rows(), Bitmap.Config.ARGB_8888);
                //org.opencv.android.Utils.matToBitmap(dest1,TTT);

                //把矩阵复制到另一个矩阵中（mask为操作掩码。它的非零元素表示矩阵中某个要被复制）
                ratoteOriginalMat.copyTo( bitmapMat, rotateMask );

                final Bitmap AAA=Bitmap.createBitmap(bitmapMat.cols(),bitmapMat.rows(), Bitmap.Config.ARGB_8888);
                org.opencv.android.Utils.matToBitmap(bitmapMat,AAA);


                /*
                //Bitmap方法
                //btye[]转Bitmap
                final Bitmap bitmap = BytetoBitmap(mNV21Bytes);

                if (bitmapWidth>bitmap.getWidth())
                    bitmapWidth=bitmap.getWidth();

                int width = (int) bitmapWidth;
                int height = (int) bitmapWidth;
                int[] pixels = new int[width * height];


                if (x+width>bitmap.getWidth())
                    x=bitmap.getWidth()-width;
                if (y+height>bitmap.getHeight())
                    y=bitmap.getHeight()-height;
                if (x<0)
                    x=0;
                if (y<0)
                    y=0;

                final Bitmap bigBitmap = GetResizedBitmap(bitmap);
                Log.d("bigBitmap", "processImage: bigBitmap="+bigBitmap.getWidth()+" "+bigBitmap.getHeight());
                //整个图片旋转faceLandmarkInfo.roll
                final Bitmap bitmapTemp = BitmapMatricRotate(bigBitmap,faceLandmarkInfo.roll, middlePoint.X+240,middlePoint.Y+320);
                    //截图
                bitmapTemp.getPixels(pixels, 0, width, x+240, y+320, width, height);
                final Bitmap dest = Bitmap.createBitmap(pixels, width, height, bitmapTemp.getConfig());

                //图片模型转换
                Bitmap pyTorchBitmap = BitmapUtils.PytorchFunction(mModule, this, dest, 200, 200);

                //转换后的图片缩放回原大小
                final Bitmap scaleBitmap = BitmapUtils.scaleBitmap(pyTorchBitmap, width, height);

                //图片合并
                final Bitmap mergeBitmap = BitmapUtils.mergeBitmap(bitmapTemp, scaleBitmap, x+240, y+320);

                //整个图片旋转复原
                final Bitmap rotateMergeBitmap=BitmapMatricRotate(mergeBitmap,-faceLandmarkInfo.roll, middlePoint.X+240,middlePoint.Y+320);

                //截图
                int[] newPixels = new int[480 * 640];
                rotateMergeBitmap.getPixels(newPixels, 0, 480, 240, 320, 480, 640);
                final Bitmap newBitmap = Bitmap.createBitmap(newPixels, 480, 640, rotateMergeBitmap.getConfig());

                 */

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
                            bigBitmapView.setImageBitmap(AAA);
                        }
                    });

                bigBgView.post(new Runnable() {
                    @Override
                    public void run() {
                        //bigBgView.setImageBitmap((TTT));
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
        final String moduleAssetNameFromIntent = "traced-model-200-2.pt";
        mModuleAssetName = !TextUtils.isEmpty(moduleAssetNameFromIntent)
                ? moduleAssetNameFromIntent
                : "traced-model-only.pt";

        return mModuleAssetName;
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