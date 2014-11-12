﻿// Copyright(C) David W. Jeske, 2013
// Released to the public domain. 

using System;

using OpenTK;

namespace SimpleScene
{
	public static class OpenTKHelper
	{

	    // MouseToWorldRay
	    //
	    // This takes the view matricies, and a window-local mouse coordinate, and returns a ray in world space.

		public static SSRay MouseToWorldRay(
			Matrix4 projection, 
			Matrix4 view, 
			System.Drawing.Size viewport, 
			Vector2 mouse) 
		{
			// these mouse.Z values are NOT scientific. 
			// Near plane needs to be < -1.5f or we have trouble selecting objects right in front of the camera. (why?)
		    Vector3 pos1 = UnProject(ref projection, view, viewport, new Vector3(mouse.X,mouse.Y,-1.5f)); // near
			Vector3 pos2 = UnProject(ref projection, view, viewport, new Vector3(mouse.X,mouse.Y,1.0f));  // far
			return SSRay.FromTwoPoints(pos1, pos2);
		}

		// UnProject takes a window-local mouse-coordinate, and a Z-coordinate depth [0,1] and 
		// unprojects it, returning the point in world space. To get a ray, UnProject the
		// mouse coordinates at two different z-values.
		//
		// http://www.opentk.com/node/1276#comment-13029

		public static Vector3 UnProject(
			ref Matrix4 projection, 
			Matrix4 view, 
			System.Drawing.Size viewport, 
			Vector3 mouse) 
		{
			Vector4 vec;
		 
			vec.X = 2.0f * mouse.X / (float)viewport.Width - 1;
			vec.Y = -(2.0f * mouse.Y / (float)viewport.Height - 1);
			vec.Z = mouse.Z;
			vec.W = 1.0f;
		 
			Matrix4 viewInv = Matrix4.Invert(view);
			Matrix4 projInv = Matrix4.Invert(projection);
		 
			Vector4.Transform(ref vec, ref projInv, out vec);
			Vector4.Transform(ref vec, ref viewInv, out vec);
		 
			if (vec.W > float.Epsilon || vec.W < -float.Epsilon)
			{
				vec.X /= vec.W;
				vec.Y /= vec.W;
				vec.Z /= vec.W;
			}
		 
			return new Vector3(vec.X,vec.Y,vec.Z);
		}

        
        /// <summary>
        /// Distance from a ray to a point at the closest spot. The ray is assumed to be infinite length.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="point"></param>
        /// <returns></returns>
		public static float DistanceToLine(SSRay ray, Vector3 point, out float distanceAlongRay) {

            // http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line

		    Vector3 a = ray.pos;
		    Vector3 n = ray.dir;
		    Vector3 p = point;

			distanceAlongRay = Vector3.Dot((a-p),n);

		    return ((a-p) - distanceAlongRay * n).Length;
        }

#if false
        public static float DistanceToLine_2(SSRay ray, Vector3 point) {
            return Vector3.Cross(ray.dir, point - ray.pos).Length;
        }

		// http://www.geometrictools.com/Documentation/DistancePointLine.pdf
		public static float DistanceToLine_3(SSRay ray, Vector3 point) {
		    float t0 = Vector3.Dot(ray.dir, ( point - ray.pos) ) / Vector3.Dot(ray.dir,ray.dir);
            float distance = (point - (ray.pos + (t0 * ray.dir))).Length;
            return distance;
		}
#endif

		public static UInt16[] generateLineIndicies(UInt16[] indicies) {
			int line_count = indicies.Length / 3;
			UInt16[] line_indicies = new UInt16[line_count * 6];
			int v = 0;
			for (int i = 2; i < indicies.Length; i += 3) {
				var v1i = indicies [i - 2];
				var v2i = indicies [i - 1];
				var v3i = indicies [i];

				line_indicies [v++] = v1i;
				line_indicies [v++] = v2i;
				line_indicies [v++] = v1i;
				line_indicies [v++] = v3i;
				line_indicies [v++] = v2i;
				line_indicies [v++] = v3i;
			}
			return line_indicies;
		}

        /// <summary>
        /// Computes the distance between two quaternions.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns>distance between quaternions in radians</returns>
        public static float RadialDistanceTo(this Quaternion q1, Quaternion q2) {
            // http://math.stackexchange.com/questions/90081/quaternion-distance

            double inner_product = q1.X * q2.X + q1.Y * q2.Y + q1.Z * q2.Z + q1.W * q2.W;
            return (float)Math.Acos(2.0 * Math.Pow(inner_product,2.0) - 1);
        }

		// http://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
		public static bool TriangleRayIntersectionTest(Vector3 V1, Vector3 V2, Vector3 V3, Vector3 rayStart, Vector3 rayDir, out float contact) {
			Vector3 e1, e2;  //Edge1, Edge2
            Vector3 P, Q, T;
            float det, inv_det, u, v;
            float t;

			contact = 0.0f;
 
            //Find vectors for two edges sharing V1
			e1 = V2 - V1;
			e2 = V3 - V1;
            //Begin calculating determinant - also used to calculate u parameter
			P = Vector3.Cross(rayDir, e2);
            //if determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(e1,P);
			if (det < 0.0f) return false; // backfaced triangle
			if (det > -float.Epsilon && det < float.Epsilon) return false; // triangle parallel to ray
            inv_det = 1.0f / det;
 
            //calculate distance from V1 to ray origin
			T = rayStart - V1;
 
            //Calculate u parameter and test bound
            u = Vector3.Dot(T,P) * inv_det;
            //The intersection lies outside of the triangle
            if(u < 0.0f || u > 1.0f) return false;
 
            //Prepare to test v parameter
			Q = Vector3.Cross(T,e1);
 
            //Calculate V parameter and test bound
			v = Vector3.Dot(rayDir,Q) * inv_det;
            //The intersection lies outside of the triangle
            if(v < 0.0f || u + v  > 1.0f) return false;
 
			t = Vector3.Dot(e2,Q) * inv_det;
  
			if(t > float.Epsilon) { //ray intersection
                contact = t;
                return true;
            }
 
            // No hit, no win
            return false;
		} // fn


        public static bool intersectRayAABox1(SSRay ray, SSAABB box, ref float tnear, ref float tfar) {
            // r.dir is unit direction vector of ray
            Vector3 dirfrac = new Vector3();
            float t;
            dirfrac.X = 1.0f / ray.dir.X;
            dirfrac.Y = 1.0f / ray.dir.Y;
            dirfrac.Z = 1.0f / ray.dir.Z;
            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            float t1 = (box.min.X - ray.pos.X)*dirfrac.X;
            float t2 = (box.max.X - ray.pos.X)*dirfrac.X;
            float t3 = (box.min.Y - ray.pos.Y)*dirfrac.Y;
            float t4 = (box.max.Y - ray.pos.Y)*dirfrac.Y;
            float t5 = (box.min.Z - ray.pos.Z)*dirfrac.Z;
            float t6 = (box.max.Z - ray.pos.Z)*dirfrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but whole AABB is behing us
            if (tmax < 0)
            {
                t = tmax;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                t = tmax;
                return false;
            }

            t = tmin;
            return true;

        }

        // Ray to AABB (AxisAlignedBoundingBox)
        // http://gamedev.stackexchange.com/questions/18436/most-efficient-aabb-vs-ray-collision-algorithms
       
        public static bool intersectRayAABox2(SSRay ray, SSAABB box, ref float tnear, ref float tfar) {
            Vector3d T_1 = new Vector3d();
            Vector3d T_2 = new Vector3d(); // vectors to hold the T-values for every direction
            double t_near = double.MinValue; // maximums defined in float.h
            double t_far = double.MaxValue;

            for (int i = 0; i < 3; i++){ //we test slabs in every direction
                if (ray.dir[i] == 0){ // ray parallel to planes in this direction
                    if ((ray.pos[i] < box.min[i]) || (ray.pos[i] > box.max[i])) {
                        return false; // parallel AND outside box : no intersection possible
                    }
                } else { // ray not parallel to planes in this direction
                    T_1[i] = (box.min[i] - ray.pos[i]) / ray.dir[i];
                    T_2[i] = (box.max[i] - ray.pos[i]) / ray.dir[i];

                    if(T_1[i] > T_2[i]){ // we want T_1 to hold values for intersection with near plane
                        var swp = T_2; // swap
                        T_1 = swp; T_2 = T_1;   
                    }
                    if (T_1[i] > t_near){
                        t_near = T_1[i];
                    }
                    if (T_2[i] < t_far){
                        t_far = T_2[i];
                    }
                    if( (t_near > t_far) || (t_far < 0) ){
                        return false;
                    }
                }
            }
            tnear = (float)t_near; tfar = (float)t_far; // put return values in place
            return true; // if we made it here, there was an intersection - YAY
        }

        public static float DegreeToRadian(this float angleInDegrees) {
			return (float)Math.PI * angleInDegrees / 180.0f;
		}

        public static float RadianToDegree(this float angleInRadians) {
			return 180f * (angleInRadians / (float)Math.PI);            
		}

		public static float Clamp(float value, float min, float max) {
			value = (value < min ? min : value);
			value = (value > max ? max : value);
			return value;
		}

        public static void TwoPerpAxes(Vector3 zAxis, 
            out Vector3 xAxis, 
            out Vector3 yAxis,
            float delta = 0.01f) 
        {
            // pick two perpendicular axes to an axis
            zAxis.Normalize();
            if (Math.Abs(zAxis.X) < delta
                && Math.Abs(zAxis.Y) < delta) { // special case
                xAxis = Vector3.UnitX;
            } else {
                xAxis = new Vector3(zAxis.Y, -zAxis.X, 0.0f);
            }
            yAxis = Vector3.Cross(zAxis, xAxis);
        }

        public static Vector3 ProjectCoord(Vector3 pt, 
            Vector3 dirX, Vector3 dirY, Vector3 dirZ) 
        {
            // projects a point onto 3 axes
            // (assumes dir vectors are unit length)
            Vector3 ret;
            ret.X = Vector3.Dot(pt, dirX);
            ret.Y = Vector3.Dot(pt, dirY);
            ret.Z = Vector3.Dot(pt, dirZ);
            return ret;
        }

        public static bool RectsOverlap(Vector2 r1Min, Vector2 r1Max,
                                        Vector2 r2Min, Vector2 r2Max) 
        {
            // return true when two rectangles overlap in 2D
            return !(r1Max.X < r2Min.X || r2Max.X < r1Min.X
                || r1Max.Y < r2Min.Y || r2Max.Y < r1Min.Y);
        }
	}
}

