// Copyright(C) David W. Jeske, 2013
// Released to the public domain. Use, modify and relicense at will.

using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SimpleScene
{
    public class SSObjectMesh : SSObject
    {
		public SSObjectMesh () { }        
		public SSObjectMesh (SSAbstractMesh mesh) : base() {
            this.Mesh = mesh;
        }
		
        private SSAbstractMesh _mesh;
		public SSAbstractMesh Mesh {
          get { return _mesh; }
          set { _mesh = value; _setupMesh(); }
        }
        
        public override void Render (ref SSRenderConfig renderConfig)
		{
			if (_mesh != null) {
				base.Render (ref renderConfig);
				this._mesh.RenderMesh (ref renderConfig);

				if (renderConfig.renderBoundingSpheres && this.boundingSphere != null) {
                    this.boundingSphere.Render(ref renderConfig);

				}
				if (renderConfig.renderCollisionShells && this.collisionShell != null) {
                    this.collisionShell.Pos = this.Pos;
                    this.collisionShell.Scale = this.Scale;
                    this.collisionShell.Render(ref renderConfig);
				}
            }
        }

        private void _setupMesh() {
            if (_mesh != null) {
                // compute and setup bounding sphere
                float radius = computeRadius();
				this.boundingSphere = new SSObjectSphere(radius);
				this.Changed += (sender) => { 
					this.boundingSphere.Pos = this.Pos;
					this.boundingSphere.Scale = this.Scale;
				};
				// Console.WriteLine("constructed collision shell of radius {0}",radius);

				// TODO: make a more detailed collision mesh

				// notify listeners..
				ObjectChanged(); 
			} 
        }

        protected virtual float computeRadius() {
            float ret = 0f;
            foreach (var point in _mesh.EnumeratePoints()) {
                ret = Math.Max(ret, point.Length);
            }
            return ret;
        }
			
		public override bool PreciseIntersect (ref SSRay worldSpaceRay, ref float distanceAlongRay)
		{
			SSRay localRay = worldSpaceRay.Transformed (this.worldMat.Inverted ());
			SSAbstractMesh mesh = this._mesh;
			bool hit = false;			  
			float localNearestContact = float.MaxValue;
			if (mesh == null) {
				return true; // no mesh to test
			} else {
				// precise meshIntersect
				bool global_hit = mesh.TraverseTriangles ((state, V1, V2, V3) => {
					float contact;
					if (OpenTKHelper.TriangleRayIntersectionTest (V1, V2, V3, localRay.pos, localRay.dir, out contact)) {
						hit = true;
						localNearestContact = Math.Min (localNearestContact, contact);
						Console.WriteLine ("Triangle Hit @ {0} : Object {1}", contact, Name);
					}
					return false; // don't short circuit
				});
				if (hit) {
					float worldSpaceContactDistance = -localNearestContact * this.Scale.LengthFast;
					Console.WriteLine ("Nearest Triangle Hit @ {0} vs Sphere {1} : Object {2}", worldSpaceContactDistance, distanceAlongRay, Name);
					distanceAlongRay = worldSpaceContactDistance;
				}
				return global_hit || hit;
			}			     
		}
    }
}

