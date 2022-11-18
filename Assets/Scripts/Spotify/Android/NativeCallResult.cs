#if UNITY_ANDROID

using System;
using UnityEngine;

namespace Spotify.Android
{
	public class NativeCallResult<T> : CallResult<T>
	{
		public NativeCallResult(AndroidJavaObject nativeObject)
		{
			nativeObject.Call<AndroidJavaObject>("setResultCallback", new ResultCallback<T>((result) =>
			{
				SetResult(result);
			})).Dispose();

			nativeObject.Call<AndroidJavaObject>("setErrorCallback", new ErrorCallback((error) =>
			{
				SetError(new Exception(error.Call<string>("getMessage")));
			})).Dispose();

			_cancelAction = () => nativeObject.Call("cancel");
		}
	}

	public class NativeCallResult : CallResult
    {
		public NativeCallResult(AndroidJavaObject nativeObject)
		{
			nativeObject.Call<AndroidJavaObject>("setResultCallback", new ResultCallback<AndroidJavaObject>((result) =>
			{
				result.Dispose();
				SetResult(CallResult.Empty);
			})).Dispose();

			nativeObject.Call<AndroidJavaObject>("setErrorCallback", new ErrorCallback((error) =>
			{
				SetError(new Exception(error.Call<string>("getMessage")));
			})).Dispose();

			_cancelAction = () => nativeObject.Call("cancel");
		}
	}
}

#endif