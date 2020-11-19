package com.faceDemo.activity;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
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

import org.pytorch.Module;

import java.io.File;
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

                Log.d("TenginekitPoint", "processImage: eyeDis="+dis+" bitmapWidth="+bitmapWidth+" yUp="+yUp+" yDown="+yDown+"  x="+ x+" y="+y+" middlePoint.x="+middlePoint.X+" middlePoint.y"+middlePoint.Y);

                Log.d("faceLandmarkInfo", "processImage:faceLandmarkInfo.roll="+faceLandmarkInfo.roll);

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
                            //bigBitmapView.setImageBitmap(scaleBitmap);
                        }
                    });

                bigBgView.post(new Runnable() {
                    @Override
                    public void run() {
                        //bigBgView.setImageBitmap((mergeBitmap));
                    }
                });

                mergeView.post(new Runnable() {
                    @Override
                    public void run() {
                        mergeView.setImageBitmap((newBitmap));
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
        final String moduleAssetNameFromIntent = "traced-model-1118-2.pt";
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

}