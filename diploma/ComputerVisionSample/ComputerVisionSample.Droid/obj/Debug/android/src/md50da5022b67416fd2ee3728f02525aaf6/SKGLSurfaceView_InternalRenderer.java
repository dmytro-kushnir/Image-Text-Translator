package md50da5022b67416fd2ee3728f02525aaf6;


public class SKGLSurfaceView_InternalRenderer
	extends md50da5022b67416fd2ee3728f02525aaf6.SKGLSurfaceViewRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkiaSharp.Views.Android.SKGLSurfaceView+InternalRenderer, SkiaSharp.Views.Android, Version=1.56.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756", SKGLSurfaceView_InternalRenderer.class, __md_methods);
	}


	public SKGLSurfaceView_InternalRenderer () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SKGLSurfaceView_InternalRenderer.class)
			mono.android.TypeManager.Activate ("SkiaSharp.Views.Android.SKGLSurfaceView+InternalRenderer, SkiaSharp.Views.Android, Version=1.56.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756", "", this, new java.lang.Object[] {  });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
