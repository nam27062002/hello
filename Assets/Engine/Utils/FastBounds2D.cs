//--------------------------------------------------------------------------------
// FastBounds2D
//--------------------------------------------------------------------------------
// This is an alternative to Unity's Bounds struct, which allows faster intersection
// testing and is optimized for 2D.
//
// NOTE: this is a class, not a struct.  This means it is expensive to 'new' it as
// a temporary/local variable, but will always be passed around by reference.
//--------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class FastBounds2D
{
	public float				x0,y0,x1,y1;
	
	public float				w {get{return x1-x0;}}
	public float				h {get{return y1-y0;}}
	
	//----------------------------------------------------------------------------
	// Constructors
	//----------------------------------------------------------------------------
	public FastBounds2D()
	{
		x0=y0=x1=y1= 0.0f;
	}
	
	public FastBounds2D(ref Bounds b)
	{
		Vector3 bmin = b.min;
		Vector3 bmax = b.max;
		x0 = bmin.x;
		y0 = bmin.y;
		x1 = bmax.x;
		y1 = bmax.y;
	}
	
	public FastBounds2D(Vector3 centre, Vector3 size)
	{
		x0 = centre.x - (size.x*0.5f);
		y0 = centre.y - (size.y*0.5f);
		x1 = x0 + size.x;
		y1 = y0 + size.y;
	}
	
	public FastBounds2D(ref Vector3 centre, ref Vector3 size)
	{
		x0 = centre.x - (size.x*0.5f);
		y0 = centre.y - (size.y*0.5f);
		x1 = x0 + size.x;
		y1 = y0 + size.y;
	}
	
	public FastBounds2D(float centreX, float centreY, float sizeX, float sizeY)
	{
		x0 = centreX - (sizeX*0.5f);
		y0 = centreY - (sizeY*0.5f);
		x1 = x0 + sizeX;
		y1 = y0 + sizeY;
	}
	
	//----------------------------------------------------------------------------
	// Set full bounds (centre and size)
	//----------------------------------------------------------------------------
	public void Set(FastBounds2D b)
	{
		x0=b.x0;
		y0=b.y0;
		x1=b.x1;
		y1=b.y1;
	}
	
	public void Set(ref Bounds b)
	{
		Vector3 bmin = b.min;
		Vector3 bmax = b.max;
		x0 = bmin.x;
		y0 = bmin.y;
		x1 = bmax.x;
		y1 = bmax.y;
	}
	
	public void Set(Vector3 centre, Vector3 size)
	{
		x0 = centre.x - (size.x*0.5f);
		y0 = centre.y - (size.y*0.5f);
		x1 = x0 + size.x;
		y1 = y0 + size.y;
	}
	
	public void Set(ref Vector3 centre, ref Vector3 size)
	{
		x0 = centre.x - (size.x*0.5f);
		y0 = centre.y - (size.y*0.5f);
		x1 = x0 + size.x;
		y1 = y0 + size.y;
	}
	
	public void Set(float centreX, float centreY, float sizeX, float sizeY)
	{
		x0 = centreX - (sizeX*0.5f);
		y0 = centreY - (sizeY*0.5f);
		x1 = x0 + sizeX;
		y1 = y0 + sizeY;
	}
	
	public void SetMinMax(Vector3 vmin, Vector3 vmax)
	{
		x0 = vmin.x;
		y0 = vmin.y;
		x1 = vmax.x;
		y1 = vmax.y;
	}
	
	public void SetMinMax(ref Vector3 vmin, ref Vector3 vmax)
	{
		x0 = vmin.x;
		y0 = vmin.y;
		x1 = vmax.x;
		y1 = vmax.y;
	}
	
	public void SetMinMax(float vx0, float vy0, float vx1, float vy1)
	{
		x0=vx0;
		y0=vy0;
		x1=vx1;
		y1=vy1;
	}


	
	//----------------------------------------------------------------------------
	// Convert to a Unity Bounds struct
	//----------------------------------------------------------------------------
	public void CopyToBounds(ref Bounds b)
	{
		b.center = new Vector3((x0+x1)*0.5f, (y0+y1)*0.5f, 0.0f);
		b.size = new Vector3(x1-x0, y1-y0, 1.0f);
	}
	
	public Bounds ToBounds()
	{
		return new Bounds(new Vector3((x0+x1)*0.5f, (y0+y1)*0.5f, 0.0f), new Vector3(x1-x0, y1-y0, 1.0f));
	}

	public Rect ToRect()
	{
		return new Rect(x0, y0, w, h);
	}
	
	//----------------------------------------------------------------------------
	// Set centre, leave size unchanged
	//----------------------------------------------------------------------------
	public void UpdateCentre(Vector3 c)
	{
		float sx = x1-x0;
		float sy = y1-y0;
		x0 = c.x - (sx*0.5f);
		y0 = c.y - (sy*0.5f);
		x1 = x0 + sx;
		y1 = y0 + sy;
	}
	
	public void UpdateCentre(ref Vector3 c)
	{
		float sx = x1-x0;
		float sy = y1-y0;
		x0 = c.x - (sx*0.5f);
		y0 = c.y - (sy*0.5f);
		x1 = x0 + sx;
		y1 = y0 + sy;
	}
	
	public void UpdateCentre(float cx, float cy)
	{
		float sx = x1-x0;
		float sy = y1-y0;
		x0 = cx - (sx*0.5f);
		y0 = cy - (sy*0.5f);
		x1 = x0 + sx;
		y1 = y0 + sy;
	}
	
	public Vector3 GetCentre()
	{
		float cx = (x0+x1)*0.5f;
		float cy = (y0+y1)*0.5f;
		return new Vector3(cx, cy, 0.0f);
	}
	
	public void GetCentre(out Vector3 c)
	{
		c.x = (x0+x1)*0.5f;
		c.y = (y0+y1)*0.5f;
		c.z = 0.0f;
	}

	public void GetSize( out Vector3 s )
	{
		s.x = x1 - x0;
		s.y = y1 - y0;
		s.z = 0;
	}
	
	//----------------------------------------------------------------------------
	// Set size, leave centre unchanged
	//----------------------------------------------------------------------------
	public void UpdateSize(Vector3 s)
	{
		float cx = (x0+x1)*0.5f;
		float cy = (y0+y1)*0.5f;
		x0 = cx - (s.x*0.5f);
		y0 = cy - (s.y*0.5f);
		x1 = x0 + s.x;
		y1 = y0 + s.y;
	}

	public void UpdateSize(ref Vector3 s)
	{
		float cx = (x0+x1)*0.5f;
		float cy = (y0+y1)*0.5f;
		x0 = cx - (s.x*0.5f);
		y0 = cy - (s.y*0.5f);
		x1 = x0 + s.x;
		y1 = y0 + s.y;
	}

	public void UpdateSize(float sx, float sy)
	{
		float cx = (x0+x1)*0.5f;
		float cy = (y0+y1)*0.5f;
		x0 = cx - (sx*0.5f);
		y0 = cy - (sy*0.5f);
		x1 = x0 + sx;
		y1 = y0 + sy; 
	}
	
	//----------------------------------------------------------------------------
	// Modify size.
	//----------------------------------------------------------------------------
	public void ApplyScale(float s)
	{
		float cx = (x0+x1)*0.5f;
		float cy = (y0+y1)*0.5f;
		float sx = (x1-x0);
		float sy = (y1-y0);
		sx *= s;
		sy *= s;
		x0 = cx-(sx*0.5f);
		y0 = cy-(sy*0.5f);
		x1 = x0+sx;
		y1 = y0+sy;
	}
	
	//----------------------------------------------------------------------------
	// Intersection tests
	//----------------------------------------------------------------------------
	public bool Intersects(FastBounds2D b)
	{
		return !((x1<b.x0) || (y1<b.y0) || (x0>b.x1) || (y0>b.y1));
	}

	public bool Intersects( Bounds b )
	{
		return !((x1<b.min.x) || (y1<b.min.y) || (x0>b.max.x) || (y0>b.max.y));
	}

	public bool Intersects( Rect b )
	{
		return !((x1<b.min.x) || (y1<b.min.y) || (x0>b.max.x) || (y0>b.max.y));
	}
	
	public bool Contains(Vector3 p)
	{
		float x = p.x;
		float y = p.y;
		return (x>=x0) && (x<=x1) && (y>=y0) && (y<=y1);
	}
	
	public bool Contains(ref Vector3 p)
	{
		float x = p.x;
		float y = p.y;
		return (x>=x0) && (x<=x1) && (y>=y0) && (y<=y1);
	}

	public bool Contains(float x, float y)
	{
		return (x>=x0) && (x<=x1) && (y>=y0) && (y<=y1);
	}
	
	// this returns true if the other bounds is completely contained by this one
	public bool Contains(FastBounds2D b)
	{
		return (b.x0>=x0) && (b.x1<=x1) && (b.y0>=y0) && (b.y1<=y1);
	}
	
	//----------------------------------------------------------------------------
	// Misc
	//----------------------------------------------------------------------------
	public void Encapsulate(Vector3 p)
	{
		float x = p.x;
		float y = p.y;
		x0 = Mathf.Min(x0, x);
		x1 = Mathf.Max(x1, x);
		y0 = Mathf.Min(y0, y);
		y1 = Mathf.Max(y1, y);
	}
	
	public void Encapsulate(ref Vector3 p)
	{
		float x = p.x;
		float y = p.y;
		x0 = Mathf.Min(x0, x);
		x1 = Mathf.Max(x1, x);
		y0 = Mathf.Min(y0, y);
		y1 = Mathf.Max(y1, y);
	}
	
	public void Encapsulate(float x, float y)
	{
		x0 = Mathf.Min(x0, x);
		x1 = Mathf.Max(x1, x);
		y0 = Mathf.Min(y0, y);
		y1 = Mathf.Max(y1, y);
	}
	
	public void Encapsulate(FastBounds2D b)
	{
		x0 = Mathf.Min(x0, b.x0);
		x1 = Mathf.Max(x1, b.x1);
		y0 = Mathf.Min(y0, b.y0);
		y1 = Mathf.Max(y1, b.y1);
	}
	
	// this will encapsulate the desired point/bounds by panning only, the size of this bounds will remain unchanged
	public void EncapsulateFixedSize(FastBounds2D b)
	{
		float bx0 = b.x0;
		float bx1 = b.x1;
		float by0 = b.y0;
		float by1 = b.y1;
		
		if(bx0<x0)
		{
			x1 -= (x0-bx0);
			x0 = bx0;
		}
		if(bx1>x1)
		{
			x0 += (bx1-x1);
			x1 = bx1;
		}
		if(by0<y0)
		{
			y1 -= (y0-by0);
			y0 = by0;
		}
		if(by1>y1)
		{
			y0 += (by1-y1);
			y1 = by1;
		}
	}
	
	// clamp the current bounds to make sure no part of it goes outside the specified bounds 'b'
	public void ClipTo(FastBounds2D b)
	{
		float bx0 = b.x0;
		float bx1 = b.x1;
		float by0 = b.y0;
		float by1 = b.y1;
		x0 = Mathf.Clamp(x0, bx0, bx1);
		x1 = Mathf.Clamp(x1, bx0, bx1);
		y0 = Mathf.Clamp(y0, by0, by1);
		y1 = Mathf.Clamp(y1, by0, by1);
	}
	
	// clamp the current bounds between two other bounds.  If the inner bounds is not fully contained by the outer bounds then this may have weird results.
	public void ClampTo(FastBounds2D inner, FastBounds2D outer)
	{
		x0 = Mathf.Clamp(x0, outer.x0, inner.x0);
		x1 = Mathf.Clamp(x1, inner.x1, outer.x1);
		y0 = Mathf.Clamp(y0, outer.y0, inner.y0);
		y1 = Mathf.Clamp(y1, inner.y1, outer.y1);
	}
	
	// stretch the bounds in a certain direction, and by a certain amount, specified as a delta vector
	public void ExpandBy(Vector3 delta)
	{
		float dx = delta.x;
		float dy = delta.y;
		if(dx < 0.0f)
			x0 += dx;
		else
			x1 += dx;
		if(dy < 0.0f)
			y0 += dy;
		else
			y1 += dy;
	}
	
	public void ExpandBy(ref Vector3 delta)
	{
		float dx = delta.x;
		float dy = delta.y;
		if(dx < 0.0f)
			x0 += dx;
		else
			x1 += dx;
		if(dy < 0.0f)
			y0 += dy;
		else
			y1 += dy;
	}
	
	public void ExpandBy(float dx, float dy)
	{
		if(dx < 0.0f)
			x0 += dx;
		else
			x1 += dx;
		if(dy < 0.0f)
			y0 += dy;
		else
			y1 += dy;
	}
	
	public float GetSqDistanceToPoint(Vector2 v)
	{
		float d2 = 0.0f;
		float x = v.x;
		float y = v.y;
		if(x < x0)
		{
			float dx = x0-x;
			d2 += dx*dx;
		}
		else if(x>x1)
		{
			float dx = x-x1;
			d2 += dx*dx;
		}
		if(y<y0)
		{
			float dy = y0-y;
			d2 += dy*dy;
		}
		else if(y>y1)
		{
			float dy = y1-y;
			d2 += dy*dy;
		}
		return d2;
	}
	
	
}
