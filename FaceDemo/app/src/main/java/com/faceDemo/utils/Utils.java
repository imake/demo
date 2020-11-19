package com.faceDemo.utils;

import android.content.Context;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.text.TextUtils;
import android.util.Log;

import com.faceDemo.Constants;

import org.json.JSONObject;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetAddress;
import java.util.Arrays;
import java.util.Collection;
import java.util.Formatter;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;
import java.util.Random;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class Utils {	
	private static final String TAG = "Utils";
	private static final boolean DEBUG = false;
	
	public static boolean isEmpty(final String str) {
		return str == null || str.length() <= 0;
	}
	
	public static boolean isEmpty(final Collection<? extends Object> collection){
		return collection == null || collection.size() <= 0;
	}
	
	public static boolean isEmpty(final Map<? extends Object,? extends Object> list){
		return list == null || list.size() <= 0;
	}

	public static boolean isEmpty(final byte[] bytes) {
		return bytes == null || bytes.length <= 0;
	}
	
	public static boolean isEmpty(final String[] strArr){
		return strArr == null || strArr.length <= 0;
	}
	
	public static int nullAs(final Integer obj, final int def) {
		return obj == null ? def : obj;
	}

	public static long nullAs(final Long obj, final long def) {
		return obj == null ? def : obj;
	}

	public static boolean nullAs(final Boolean obj, final boolean def) {
		return obj == null ? def : obj;
	}

	public static String nullAs(final String obj, final String def) {
		return obj == null ? def : obj;
	}
	
	public static String emptyAs(final String obj, final String def) {
		return isEmpty(obj) ? def : obj;
	}
	
	public static int nullAsNil(final Integer obj) {
		return obj == null ? 0 : obj;
	}

	public static long nullAsNil(final Long obj) {
		return obj == null ? 0L : obj;
	}

	public static String nullAsNil(final String obj) {
		return obj == null ? "" : obj;
	}
	
	public static int optInt(final String string){
		return optInt(string,0);
	}
	
	public static int optInt(final String string, final int def) {
		try {
			if(!isEmpty(string)){
				return Integer.parseInt(string);
			}
		} catch (NumberFormatException e) {
			//ZLog.e(TAG, e);
		}
		return def;
	}

	public static long optLong(final String string, final long def) {
		try {
			if(!isEmpty(string)){
				return Long.parseLong(string);
			}
		} catch (NumberFormatException e) {
			//ZLog.e(TAG, e);
		}
		return def;
	}
	
	// 时间转字符串函数
	public static String time2String(int timeMs) {
		if(timeMs==0){
			return "";
		}
		StringBuffer mStringBuffer = new StringBuffer();
		int seconds = timeMs % 60;
		int minutes = (timeMs / 60) % 60;
		int hours = timeMs / 3600;

		if (hours > 0) {
			return extracted(mStringBuffer).format("%d:%02d:%02d", hours,
					minutes, seconds).toString();
		} else {
			return extracted(mStringBuffer).format("%02d:%02d", minutes,
					seconds).toString();
		}
	}

	public static Formatter extracted(StringBuffer mStringBuffer) {
		return new Formatter(mStringBuffer, Locale.getDefault());
	}
	
	/*
	 *把int型转换为带分隔符的string，如：2456转化为2,456
	 */
	public static String intToSpecialString(int num){
		String ret=num+"";
		int length=ret.length();
		if(length>0){
			int delimiterNum=(length-1)/3;
			for(int i=0;i<delimiterNum;i++){
				int midPos=length-(i+1)*3;
				ret=ret.substring(0, midPos)+","+ret.substring(midPos, ret.length());
			}
		}
		return ret;
	}

	public static String getDomainName(String requestUrl) {
		String hostName = null;
		if (!TextUtils.isEmpty(requestUrl)) {
			try {
				Pattern p = Pattern.compile(
						"[^//]*?\\.(com|cn|net|org|biz|info|cc|tv)",
						Pattern.CASE_INSENSITIVE);
				Matcher matcher = p.matcher(requestUrl);
				if (matcher.find()) {
					hostName = matcher.group();
				}
			} catch (Exception e) {

			}
		}
		return hostName;
	}
	
	// 域名到ip的静态映射
	public static final HashMap<String, String> domainIpMap = new HashMap<String, String>();

	public static String getIp(String domainName){
		String ip = null;
		if(!TextUtils.isEmpty(domainName)){
			ip = domainIpMap.get(domainName);
			if(TextUtils.isEmpty(ip)){
				try{
					InetAddress inetIp = InetAddress.getByName(domainName);
					ip = inetIp.getHostAddress();
					
				}catch(Exception e){
					Log.d("IPAdress",
							"failed to get ip,error=" + e.toString());
				}
				if(TextUtils.isEmpty(ip)){
					ip = "ip address not found!";
				}
				domainIpMap.put(domainName, ip);
			}
		}
		return ip;
	}
	
	/** 
     * 使用java正则表达式去掉多余的.与0 
     * @param s 
     * @return  
     */  
    public static String subZeroAndDot(String s){
        if(s.indexOf(".") > 0){  
            s = s.replaceAll("0+?$", "");//去掉多余的0  
            s = s.replaceAll("[.]$", "");//如最后一位是.则去掉  
        }  
        return s;  
    }  
    
    /**
	 * To tell the decoder to subsample the image, loading a smaller version into memory, 
	 * set inSampleSize to true in your BitmapFactory.Options object. For example, 
	 * an image with resolution 2048x1536 that is decoded with an inSampleSize of 4 produces 
	 * a bitmap of approximately 512x384. Loading this into memory uses 0.75MB rather than 
	 * 12MB for the full image (assuming a bitmap configuration of ARGB_8888). Here’s a 
	 * method to calculate a sample size value that is a power of two based on a target 
	 * width and height.
	 * 
	 * @param options
	 * @param reqWidth
	 * @param reqHeight
	 * @return
	 */
	private static int calculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight) {
		// Raw height and width of image    
		final int height = options.outHeight;    
		final int width = options.outWidth;    
		int inSampleSize = 1;    
		
		if (reqHeight > 0 && reqWidth > 0 && (height > reqHeight || width > reqWidth)) {
			final int halfHeight = height / 2;        
			final int halfWidth = width / 2;      
			
			// Calculate the largest inSampleSize value that is a power of 2 and keeps both        
			// height and width larger than the requested height and width.        
			while ((halfHeight / inSampleSize) > reqHeight                
					&& (halfWidth / inSampleSize) > reqWidth) {            
				inSampleSize *= 2;      
			}
		}
		
		if (DEBUG) {
			Log.d(TAG, "!w sample size: " + inSampleSize);
		}
		
		return inSampleSize;
	}
	
	/**
	 * To use this method, first decode with inJustDecodeBounds set to true, 
	 * pass the options through and then decode again using the new inSampleSize 
	 * value and inJustDecodeBounds set to false:
	 * 
	 * @param res
	 * @param resId
	 * @param reqWidth
	 * @param reqHeight
	 * @return
	 */
	public static Bitmap decodeSampledBitmap(Resources res, int resId, int reqWidth, int reqHeight) {
		
		// First decode with inJustDecodeBounds=true to check dimensions    
		final BitmapFactory.Options options = new BitmapFactory.Options();
		options.inJustDecodeBounds = true;    
		BitmapFactory.decodeResource(res, resId, options);
		
		// Calculate inSampleSize    
		options.inSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);    
		
		// Decode bitmap with inSampleSize set    
		options.inJustDecodeBounds = false;    
		return BitmapFactory.decodeResource(res, resId, options);
	}
	
	/**
	 * To use this method, first decode with inJustDecodeBounds set to true, 
	 * pass the options through and then decode again using the new inSampleSize 
	 * value and inJustDecodeBounds set to false:
	 * 
	 * @param pathName
	 * @param reqWidth
	 * @param reqHeight
	 * @return
	 */
	public static Bitmap decodeSampledBitmap(String pathName, int reqWidth, int reqHeight) {
		
		// First decode with inJustDecodeBounds=true to check dimensions    
		final BitmapFactory.Options options = new BitmapFactory.Options();
		options.inJustDecodeBounds = true;    
		BitmapFactory.decodeFile(pathName, options);
		
		// Calculate inSampleSize    
		options.inSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);    
		
		// Decode bitmap with inSampleSize set    
		options.inJustDecodeBounds = false;    
		return BitmapFactory.decodeFile(pathName, options);
	}
	/**
	 * JSON数据的数字类型转换
	 * @param jsonObject
	 * @param key
	 * @return
	 */
   public static int getInt(JSONObject jsonObject, String key){
	   return getInt(jsonObject,key,0);
   }
   /**
    * 带默认参数的JSON数据的数字类型转换
    * @param jsonObject
    * @param key
    * @param defaultValue
    * @return
    */
   public static int getInt(JSONObject jsonObject, String key, int defaultValue){
	   try{
		   return jsonObject.getInt(key);
	   }catch(Exception e){
		   return defaultValue;
	   }
   }

	/**
	 * 获取一个0-time之间的随机数
	 * @param time
     * @return
     */
	public static int getRandom(int time){
		Random random = new Random();
		int delayTime = random.nextInt(time);
		return delayTime;
	}

	public static String assetFilePath(Context context, String assetName) {
		File file = new File(context.getFilesDir(), assetName);
		if (file.exists() && file.length() > 0) {
			return file.getAbsolutePath();
		}

		try (InputStream is = context.getAssets().open(assetName)) {
			try (OutputStream os = new FileOutputStream(file)) {
				byte[] buffer = new byte[4 * 1024];
				int read;
				while ((read = is.read(buffer)) != -1) {
					os.write(buffer, 0, read);
				}
				os.flush();
			}
			return file.getAbsolutePath();
		} catch (IOException e) {
			Log.e(Constants.TAG, "Error process asset " + assetName + " to file path");
		}
		return null;
	}

	public static int[] topK(float[] a, final int topk) {
		float values[] = new float[topk];
		Arrays.fill(values, -Float.MAX_VALUE);
		int ixs[] = new int[topk];
		Arrays.fill(ixs, -1);

		for (int i = 0; i < a.length; i++) {
			for (int j = 0; j < topk; j++) {
				if (a[i] > values[j]) {
					for (int k = topk - 1; k >= j + 1; k--) {
						values[k] = values[k - 1];
						ixs[k] = ixs[k - 1];
					}
					values[j] = a[i];
					ixs[j] = i;
					break;
				}
			}
		}
		return ixs;
	}
}
