using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.MachineLearning;
using System.Runtime.InteropServices;

public class FaceDetectScript : MonoBehaviour
{
    const int CAPTURE_WIDTH = 320;
    const int CAPTURE_HEIGHT = 240;

    public Vector2 facepos;
	
	private PXCUPipeline pp;
	
    private CvHaarClassifierCascade cascade;
    private CvCapture capture;

    CvColor[] colors = new CvColor[]{
                new CvColor(0,0,255),
                new CvColor(0,128,255),
                new CvColor(0,255,255),
                new CvColor(0,255,0),
                new CvColor(255,128,0),
                new CvColor(255,255,0),
                new CvColor(255,0,0),
                new CvColor(255,0,255),
            };

    const double Scale = 1.04;
    const double ScaleFactor = 1.139;
    const int MinNeighbors = 2;
	
	//nu Intel
	private Texture2D labelMapImage = null;
	private Texture2D rgbTexture = null;
	
	private short[] depthmap = null;
	private short[] depthStorage = null;
	private short[] uvStorage = null;
	private float[] untrusted = new float[2]{0,0};
	private Vector3[] pos2d = null;
	
	private int[] depthMapSize = new int[2]{0,0};
	private int[] RGBMapSize = new int[2]{0,0};
    private int[] uvMapSize = new int[2]{0,0};
	
	GameObject plane;
	
    // Use this for initialization
    void Start()
    {
        PXCUPipeline.Mode mode=Options.mode&(~PXCUPipeline.Mode.VOICE_RECOGNITION);
		if (mode==0) return;
		
		pp=new PXCUPipeline();
		if (!pp.Init(mode)) {
			print("Unable to initialize the PXCUPipeline");
			return;
		}
		
		plane = GameObject.Find("Plane");
		
		pp.QueryRGBSize(RGBMapSize);
		if (RGBMapSize[0] > 0) {
			Debug.Log("rgb map size: width = " + RGBMapSize[0] + ", height = " + RGBMapSize[1]);
		 	rgbTexture = new Texture2D (RGBMapSize[0], RGBMapSize[1], TextureFormat.ARGB32, false);
		         // use the rgb texture as the rendered texture
		 	plane.renderer.material.mainTexture = rgbTexture;
			pp.QueryDepthMapSize(depthMapSize);
			if (depthMapSize[0] > 0) {
				Debug.Log("depth map size: width = " + depthMapSize[0] + ", height = " + depthMapSize[1]);
				depthStorage = new short[depthMapSize[0] * depthMapSize[1]];
			}
			pp.QueryUVMapSize(uvMapSize);
			if (uvMapSize[0] > 0) {
				Debug.Log("uv map size: width = " + uvMapSize[0] + ", height = " + uvMapSize[1]);
				uvStorage = new short[uvMapSize[0] * uvMapSize[1] * 2];
			}
		}
		
		cascade = CvHaarClassifierCascade.FromFile(@"./Assets/haarcascade_frontalface_alt.xml");
        /*capture = Cv.CreateCameraCapture(0); //bentrok dgn pxcupipeline
        Cv.SetCaptureProperty(capture, CaptureProperty.FrameWidth, CAPTURE_WIDTH);
        Cv.SetCaptureProperty(capture, CaptureProperty.FrameHeight, CAPTURE_HEIGHT);
        IplImage frame = Cv.QueryFrame(capture);
        Debug.Log("width:" + frame.Width + " height:" + frame.Height);*/
        Cv.NamedWindow("FaceDetect");
		
		CvSVM svm = new CvSVM ();
	  	CvTermCriteria criteria = new CvTermCriteria (CriteriaType.Epsilon, 1000, double.Epsilon);
	  	CvSVMParams param = new CvSVMParams (CvSVM.C_SVC, CvSVM.RBF, 10.0, 8.0, 1.0, 10.0, 0.5, 0.1, null, criteria);
	  	
    }

    // Update is called once per frame
    void Update()
    {
        if (pp==null) return;
		if (!pp.AcquireFrame(false)) return;
		
		//IplImage frame = Cv.QueryFrame(capture);
		if(rgbTexture!=null){
			Debug.Log("rgb not null");
			if (pp.QueryRGB(rgbTexture)){
				Debug.Log("query rgb done");
				//rgbTexture.Apply();
				Debug.Log ("de pixelo: "+rgbTexture.GetPixels()[0]);
				
				IplImage frame = Texture2DtoIplImage(rgbTexture);	
			
			
	        	using (IplImage img = Cv.CloneImage(frame))
	        	using (IplImage smallImg = new IplImage(new CvSize(Cv.Round(img.Width / Scale), Cv.Round(img.Height / Scale)), BitDepth.U8, 1))
	        	{
	            
				
	            	using (IplImage gray = new IplImage(img.Size, BitDepth.U8, 1))
	            	{
	              	  Cv.CvtColor(img, gray, ColorConversion.BgrToGray);
	              	  Cv.Resize(gray, smallImg, Interpolation.Linear);
	              	  Cv.EqualizeHist(smallImg, smallImg);
	            	}
	
	            	using (CvMemStorage storage = new CvMemStorage())
	            	{
	                	storage.Clear();
					
					
	               	 CvSeq<CvAvgComp> faces = Cv.HaarDetectObjects(smallImg, cascade, storage, ScaleFactor, MinNeighbors, 0, new CvSize(64, 64));
	
	                
		                for (int i = 0; i < faces.Total; i++)
		                {
		                    CvRect r = faces[i].Value.Rect;
		                    CvPoint center = new CvPoint
		                    {
		                        X = Cv.Round((r.X + r.Width * 0.5) * Scale),
		                        Y = Cv.Round((r.Y + r.Height * 0.5) * Scale)
		                    };
		                    int radius = Cv.Round((r.Width + r.Height) * 0.25 * Scale);
		                    img.Circle(center, radius, colors[i % 8], 3, LineType.AntiAlias, 0);
		                }
	
		                if (faces.Total > 0)
		                {
		                    CvRect r = faces[0].Value.Rect;
		                    //facepos = new Vector2((r.X + r.Width / 2.0f) / CAPTURE_WIDTH, (r.Y + r.Height / 2.0f) / CAPTURE_HEIGHT);
		                }
	            	}
	            	Cv.ShowImage("FaceDetect", img);
		        }
		        
			} // endif queryirasimage
			else{
				Debug.Log("failoo");
			}
		} // endif rgbTexture !null
		else{
			Debug.Log ("rgb NULL");
		}
		pp.ReleaseFrame();
    }

    void OnDestroy()
    {
        Cv.DestroyAllWindows();
        Cv.ReleaseCapture(capture);
        Cv.ReleaseHaarClassifierCascade(cascade);
    }
	
	void OnDisable() {
		if (pp==null) return;
		pp.Dispose();
		pp=null;
    }
	
	//
	private IplImage Texture2DtoIplImage(Texture2D txtur){
		
		int w = 320, h = 240;
		Color[] pixels;
		if(txtur != null){
			pixels = txtur.GetPixels();
			w = txtur.width;
			h = txtur.height;
		}else{
			pixels = new Color[w*h];
		}
		
		//IplImage img = new IplImage(w,h,BitDepth.F32,1);
		
		IplImage img = Cv.CreateImage(new CvSize(w,h),BitDepth.U8,3);
		
		for (int i = 0; i < h; i++)
	    {
			//char ptr = img.ImageData+i*img.WidthStep;
	        for (int j = 0; j < w; j++)
	        {
	            //ptr[3*j+2] = 255;
				CvScalar s = img.Get2D(i,j);
				
				s.Val0 = pixels[i*w+j].r;
				s.Val1 = pixels[i*w+j].g;
            	s.Val2 = pixels[i*w+j].b;
				
				img.Set2D(i,j,s);
	            //Color color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
	            //videoTexture.SetPixel(j, height - i - 1, color);
	        }
	    }
		
		return img;
	}
}

