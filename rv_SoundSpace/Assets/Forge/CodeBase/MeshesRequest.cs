//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_WSA
using System.Net;
#endif
using SimpleJSON;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

namespace Autodesk.Forge.ARKit {

	// Only works with the v2 server using the v1 API
	public class MeshesRequest : RequestObjectInterface {

		#region Properties
		// fragId, marterialId, GameObject, JSONNode
		public List<Eppy.Tuple<int, int, GameObject, JSONNode>> fragments { get; set; }
		// dbId, fragId, OpenCTM.Mesh
		public List<Eppy.Tuple<int, int, OpenCTM.Mesh>> _openctm { get; set; }
		private bool createCollider = false;
		#endregion

		#region Constructors
		public MeshesRequest (IForgeLoaderInterface _loader, Uri _uri, string _bearer, List<Eppy.Tuple<int, int>> _components, List<Eppy.Tuple<int, int, GameObject, JSONNode>> _fragments, JSONNode node) : base (_loader, _uri, _bearer) {
			resolved = SceneLoadingStatus.eMesh;
			fragments = _fragments;
			lmvtkDef = node;
			compression = false;
			createCollider = _loader.CreateCollider;
			_openctm = new List<Eppy.Tuple<int, int, OpenCTM.Mesh>> ();
			foreach ( Eppy.Tuple<int, int> item in _components )
				_openctm.Add (new Eppy.Tuple<int, int, OpenCTM.Mesh> (item.Item1, item.Item2, null));
		}

		#endregion

		#region Forge Request Object Interface

#if !UNITY_WSA
		public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null) {
			emitted = DateTime.Now;
			try {
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
				using ( client = new WebClient () ) {
					//client.Headers.Add ("Connection", "keep-alive") ;
					if ( callback != null )
						client.DownloadDataCompleted += new DownloadDataCompletedEventHandler (callback);
					client.Headers.Add ("x-ads-format", "openctm");
					if ( !string.IsNullOrEmpty (bearer) )
						client.Headers.Add ("Authorization", "Bearer " + bearer);
					client.Headers.Add ("Keep-Alive", "timeout=15, max=100");
					if ( compression == true )
						client.Headers.Add ("Accept-Encoding", "gzip, deflate");
					state = SceneLoadingStatus.ePending;
					client.DownloadDataAsync (uri, this);
				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			}
		}
#elif UNITY_WSA
		public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted = DateTime.Now;
			mb.StartCoroutine (_FireRequest_ (callback)) ;
		}

		public override IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) {
			//using ( client =new UnityWebRequest (uri.AbsoluteUri) ) {
			System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
			using ( client =UnityWebRequest.Get (uri.AbsoluteUri) ) {
				//client.SetRequestHeader ("Connection", "keep-alive") ;
				//client.method =UnityWebRequest.kHttpVerbGET ;
				//if ( callback != null )
				//	client.DownloadDataCompleted +=new DownloadDataCompletedEventHandler (callback) ;
				client.SetRequestHeader ("x-ads-format", "openctm") ;
				if ( !string.IsNullOrEmpty (bearer) )
					client.SetRequestHeader ("Authorization", "Bearer " + bearer) ;
				//client.SetRequestHeader ("Keep-Alive", "timeout=15, max=100");
				if ( compression == true )
					client.SetRequestHeader ("Accept-Encoding", "gzip, deflate");
				state =SceneLoadingStatus.ePending ;
				//client.DownloadDataAsync (uri, this) ;
#if UNITY_2017_2_OR_NEWER
				yield return client.SendWebRequest () ;
#else
				yield return client.Send () ;
#endif

				if ( client.isNetworkError || client.isHttpError ) {
					Debug.Log (ForgeLoader.GetCurrentMethod () + " " + client.error + " - " + client.responseCode) ;
					state =SceneLoadingStatus.eError ;
				} else {
					//client.downloadHandler.data
					//client.downloadHandler.text
					if ( callback != null ) {
						DownloadDataCompletedEventArgs args =new DownloadDataCompletedEventArgs (null, false, this) ;
						args.Result =client.downloadHandler.data ;
						callback (this, args) ;
					}
				}
			}
		}
#endif

		//public override void CancelRequest () ;

		public override void ProcessResponse (AsyncCompletedEventArgs e) {
			//TimeSpan tm = DateTime.Now - emitted;
			//UnityEngine.Debug.Log ("Received: " + tm.TotalSeconds.ToString () + " / " + uri.ToString ());
			DownloadDataCompletedEventArgs args = e as DownloadDataCompletedEventArgs;
			try {
				byte [] bytes = args.Result;
				if ( compression )
					bytes = RequestObjectInterface.Decompress (bytes);

				List<Eppy.Tuple<int, int, OpenCTM.Mesh>> tmp = new List<Eppy.Tuple<int, int, OpenCTM.Mesh>> ();
				int len = BitConverter.ToInt32 (bytes, 0);
				for ( int i = 0 ; i < len ; i++ ) {
					int dbId = BitConverter.ToInt32 (bytes, ((i * 3) + 1) * sizeof (Int32));
					int fragId = BitConverter.ToInt32 (bytes, ((i * 3) + 2) * sizeof (Int32));
					int offset = BitConverter.ToInt32 (bytes, ((i * 3) + 3) * sizeof (Int32));
					int end = bytes.Length;
					if ( i < len - 1 )
						end = BitConverter.ToInt32 (bytes, (((i + 1) * 3) + 3) * sizeof (Int32));

					byte [] ctm = RequestObjectInterface.Decompress (bytes.Skip (offset).Take (end - offset).ToArray ());

					System.IO.Stream readMemory = new System.IO.MemoryStream (ctm);// (bytes, offset, end - offset);
					OpenCTM.CtmFileReader reader = new OpenCTM.CtmFileReader (readMemory);
					//_openctm.Add (new Eppy.Tuple<int, int, OpenCTM.Mesh> (dbId, fragId, reader.decode ()));
					Eppy.Tuple<int, int, OpenCTM.Mesh> mesh = _openctm.Single (x => x.Item1 == dbId && x.Item2 == fragId);
					tmp.Add (new Eppy.Tuple<int, int, OpenCTM.Mesh> (mesh.Item1, mesh.Item2, reader.decode ()));
				}
				_openctm.Clear ();
				_openctm = tmp;

				state = SceneLoadingStatus.eReceived;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			} finally {
			}
		}

		public override string GetName () {
			return ("mesh-" + _openctm [0].Item1.ToString () + "-" + _openctm [0].Item2.ToString ());
		}

		public string GetName (int dbId, int fragId) {
			return ("mesh-" + dbId.ToString () + "-" + fragId.ToString ());
		}

		public override GameObject BuildScene (string name, bool saveToDisk = false) {
			try {
				foreach ( Eppy.Tuple<int, int, OpenCTM.Mesh> openctm in _openctm ) {
					OpenCTM.Mesh ctmMesh = openctm.Item3;
					if ( ctmMesh == null || ctmMesh.getVertexCount () == 0 || ctmMesh.getTriangleCount () == 0 ) {
						state = SceneLoadingStatus.eCancelled;
						return (gameObject);
					}

					Eppy.Tuple<int, int, GameObject, JSONNode> item = fragments.Single (x => x.Item1 == openctm.Item2);
					GameObject meshObject = item.Item3;

					Mesh mesh = new Mesh ();
					Vector3 [] vertices = MeshesRequest.getAsVector3 (ctmMesh.vertices);
					mesh.vertices = vertices;
					mesh.triangles = ctmMesh.indices;
					if ( ctmMesh.hasNormals () )
						mesh.normals = MeshesRequest.getAsVector3 (ctmMesh.normals);
					for ( int i = 0 ; i < ctmMesh.getUVCount () ; i++ )
						mesh.SetUVs (i, MeshesRequest.getAsVector2List (ctmMesh.texcoordinates [i].values));

					mesh.RecalculateNormals ();
					mesh.RecalculateBounds ();

					MeshFilter filter = meshObject.AddComponent<MeshFilter> ();
					filter.sharedMesh = mesh;
					MeshRenderer renderer = meshObject.AddComponent<MeshRenderer> ();
					renderer.sharedMaterial = ForgeLoaderEngine.GetDefaultMaterial ();
					if ( createCollider ) {
						MeshCollider collider = meshObject.AddComponent<MeshCollider> ();
						collider.sharedMesh = mesh;
					}
#if UNITY_EDITOR
					if ( saveToDisk ) {
						AssetDatabase.CreateAsset (mesh, ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + GetName (openctm.Item1, openctm.Item2) + ".asset");
						//AssetDatabase.SaveAssets () ;
						//AssetDatabase.Refresh () ;
						//mesh =AssetDatabase.LoadAssetAtPath<Mesh> (ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + name + ".asset") ;
					}
#endif

					// Add our material to the queue
					loader.GetMgr ()._materials.Add (item.Item2);
					//if ( loader.GetMgr ()._materials.Add (item.Item2) ) {
					//	MaterialRequest req = new MaterialRequest (loader, null, bearer, item.Item2, item.Item4);
					//	req.gameObject = meshObject;
					//	if ( fireRequestCallback != null )
					//		fireRequestCallback (this, req);
					//}

				}
				base.BuildScene (name, saveToDisk);
				state = SceneLoadingStatus.eWaitingMaterial;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			}
			return (gameObject);
		}

		#endregion

		#region Methods
		protected static int getInt (byte [] b, int index = 0) {
			//const int len =sizeof (Int32) ;
			//if ( BitConverter.IsLittleEndian )
			//	Array.Reverse (b, index, len) ;
			int i = BitConverter.ToInt32 (b, index);
			return (i);
		}

		protected static int [] getInts (byte [] b, int nb, int index = 0) {
			const int len = sizeof (Int32);
			int [] intArr = new int [nb];
			for ( int i = 0, pos = index ; i < nb ; i++, pos += len ) {
				//if ( BitConverter.IsLittleEndian )
				//	Array.Reverse (b, i * len, len) ;
				intArr [i] = BitConverter.ToInt32 (b, pos);
			}
			return (intArr);
		}

		protected static float [] getFloats (byte [] b, int nb, int index = 0) {
			const int len = sizeof (float);
			float [] floatArr = new float [nb];
			for ( int i = 0, pos = index ; i < nb ; i++, pos += len ) {
				//if ( BitConverter.IsLittleEndian )
				//	Array.Reverse (b, i * len, len) ;
				floatArr [i] = BitConverter.ToSingle (b, pos);
			}
			return (floatArr);
		}

		protected static Vector3 [] getAsVector3 (float [] data, int stride = 3) {
			int nb = data.Length / stride;
			Vector3 [] arr = new Vector3 [nb];
			for ( int i = 0, pos = 0 ; i < nb ; i++, pos += stride ) {
				if ( stride == 3 )
					arr [i] = new Vector3 (data [pos], data [pos + 1], data [pos + 2]);
				else if ( stride == 2 )
					arr [i] = new Vector3 (data [pos], data [pos + 1], 0.0f);
			}
			return (arr);
		}

		protected static Vector2 [] getAsVector2 (float [] data, int stride = 2) {
			int nb = data.Length / stride;
			Vector2 [] arr = new Vector2 [nb];
			for ( int i = 0, pos = 0 ; i < nb ; i++, pos += stride )
				arr [i] = new Vector2 (data [pos], data [pos + 1]);
			return (arr);
		}

		protected static List<Vector2> getAsVector2List (float [] data, int stride = 2) {
			int nb = data.Length / stride;
			List<Vector2> lst = new List<Vector2> (nb);
			for ( int i = 0, pos = 0 ; i < nb ; i++, pos += stride )
				lst.Add (new Vector2 (data [pos], data [pos + 1]));
			return (lst);
		}

		#endregion

	}

}