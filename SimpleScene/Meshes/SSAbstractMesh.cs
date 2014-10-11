// Copyright(C) David W. Jeske, 2013
// Released to the public domain. Use, modify and relicense at will.

using System;
using System.Collections.Generic;
using OpenTK;

namespace SimpleScene 
{
	public abstract class SSAbstractMesh {
		public delegate bool traverseFn<T>(T state, Vector3 V1, Vector3 V2, Vector3 V3);
		public abstract void RenderMesh (ref SSRenderConfig renderConfig);

		public abstract IEnumerable<Vector3> EnumeratePoints();
		public virtual bool TraverseTriangles<T>(T state, traverseFn<T> fn) {
			return true;
		}

		public bool TraverseTriangles(traverseFn<Object> fn) {
			return this.TraverseTriangles<Object>(new Object(), fn);
		}

        public virtual void Update(float timeDelta, Vector3 pos) { }
	}

}

