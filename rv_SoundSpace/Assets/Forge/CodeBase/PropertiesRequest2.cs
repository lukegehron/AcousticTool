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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_WSA
using System.Net;
#elif UNITY_WSA
using UnityEngine.Networking;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class PropertiesRequest2 : RequestObjectInterface {

		#region Properties
		// dbId, GameObject
		public List<Eppy.Tuple<int, GameObject>> dbIds { get; set; }
		// dbId, ForgeProperties
		public Dictionary<int, ForgeProperties> properties { get; set; }

		#endregion

		#region Constructors
		public PropertiesRequest2 (IForgeLoaderInterface _loader, Uri _uri, string _bearer, List<Eppy.Tuple<int, GameObject>> _dbIds) : base (_loader, _uri, _bearer) {
			resolved = SceneLoadingStatus.eProperties;
			dbIds = _dbIds;
			compression = true;
			properties = new Dictionary<int, ForgeProperties> ();
		}

		#endregion

		#region Forge Request Object Interface
#if !UNITY_WSA
		public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null) {
			emitted = DateTime.Now;
			try {
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
				using ( client = new WebClient () ) {
					if ( callback != null ) {
						if ( compression == true )
							client.DownloadDataCompleted += new DownloadDataCompletedEventHandler (callback);
						else
							client.DownloadStringCompleted += new DownloadStringCompletedEventHandler (callback);
					}
					if ( !string.IsNullOrEmpty (bearer) )
						client.Headers.Add ("Authorization", "Bearer " + bearer);
					client.Headers.Add ("Keep-Alive", "timeout=15, max=100");
					if ( compression == true )
						client.Headers.Add ("Accept-Encoding", "gzip, deflate");
					state = SceneLoadingStatus.ePending;
					if ( compression == true )
						client.DownloadDataAsync (uri, this);
					else
						client.DownloadStringAsync (uri, this);
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
				//	client.DownloadStringCompleted +=new DownloadStringCompletedEventHandler (callback) ;
				if ( !string.IsNullOrEmpty (bearer) )
					client.SetRequestHeader ("Authorization", "Bearer " + bearer) ;
				//client.SetRequestHeader ("Keep-Alive", "timeout=15, max=100");
				if ( compression == true )
					client.SetRequestHeader ("Accept-Encoding", "gzip, deflate");
				state =SceneLoadingStatus.ePending ;
				//client.DownloadStringAsync (uri, this) ;
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
						if ( compression == true ) {
							DownloadDataCompletedEventArgs args = new DownloadDataCompletedEventArgs (null, false, this);
							args.Result = client.downloadHandler.data;
							callback (this, args);
						} else {
							DownloadStringCompletedEventArgs args =new DownloadStringCompletedEventArgs (null, false, this) ;
							args.Result =client.downloadHandler.text ;
							callback (this, args) ;
						}
					}
				}
			}
		}
#endif

		//public override void CancelRequest () ;

		public override void ProcessResponse (AsyncCompletedEventArgs e) {
			//TimeSpan tm = DateTime.Now - emitted;
			//UnityEngine.Debug.Log ("Received: " + tm.TotalSeconds.ToString () + " / " + uri.ToString ());
			DownloadStringCompletedEventArgs args = e as DownloadStringCompletedEventArgs;
			string result = "";
			if ( args == null ) {
				DownloadDataCompletedEventArgs args2 = e as DownloadDataCompletedEventArgs;
				byte [] bytes = args2.Result;
				//WebHeaderCollection headerCollection = this.client.ResponseHeaders;
				//for ( int i = 0 ; i < headerCollection.Count ; i++ ) {
				//	if ( headerCollection.GetKey (i).ToLower () == "content-encoding" && headerCollection.Get (i).ToLower () == "gzip" ) {
				//		Debug.Log (headerCollection.GetKey (i));
				//	}
				//}
				byte [] b = RequestObjectInterface.Decompress (bytes);
				result = Encoding.UTF8.GetString (b);
			} else {
				result = args.Result;
			}
			try {
				lmvtkDef = JSON.Parse (result);
				state = SceneLoadingStatus.eReceived;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			} finally {
			}
		}

		public override string GetName () {
			return ("properties-" + dbIds [0].Item1.ToString ());
		}

		public string GetName (int dbId) {
			return ("properties-" + dbId.ToString ());
		}

		public override GameObject BuildScene (string name, bool saveToDisk = false) {
			try {
				Dictionary<int, JSONNode> dict = new Dictionary<int, JSONNode> ();
				foreach ( KeyValuePair<string, JSONNode> pair in lmvtkDef )
					dict.Add (pair.Value ["id"].AsInt, pair.Value);
				foreach ( Eppy.Tuple<int, GameObject> index in dbIds ) {
					JSONNode jsonProps = dict [index.Item1];

					ForgeProperties propertiesOnObj = index.Item2.AddComponent<ForgeProperties> ();
					propertiesOnObj.Properties = jsonProps;

					JSONNode j = FindProperty (jsonProps, "name");
					if ( j != null ) {
						name = GetDefaultValueIfUndefined (j, "value", "");
						if ( !string.IsNullOrEmpty (name) )
							index.Item2.name = name;
					}
				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			}

			base.BuildScene (name, saveToDisk);
			return (gameObject);
		}

		#endregion

		#region Methods
		public static string GetDefaultValueIfUndefined (JSONNode j, string name, string defaultValue = "") {
			string ret = j [name].Value == "null" || string.IsNullOrEmpty (j [name].Value) ? defaultValue : j [name].Value;
			return (ret);
		}

		public static JSONNode FindProperty (JSONNode j, string name) {
			//Dictionary<string, List<JSONNode>> properties = new Dictionary<string, List<JSONNode>> ();
			foreach ( JSONNode child in j ["props"].AsArray ) {
				string test = child ["name"].Value;
				if ( test != name )
					continue;
				return (child);
			}
			return (null);
		}

		#endregion

	}

}
