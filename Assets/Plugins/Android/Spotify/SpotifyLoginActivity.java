package ca.darrylday.spotifyunity;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.spotify.sdk.android.auth.AuthorizationClient;
import com.spotify.sdk.android.auth.AuthorizationRequest;
import com.spotify.sdk.android.auth.AuthorizationResponse;

public class SpotifyLoginActivity extends Activity {

    public static Utils.AuthCallback AuthCallback;
    
    public static void SetAuthCallback(Utils.AuthCallback authCallback) {
        AuthCallback = authCallback;
    }

    private static final int REQUEST_CODE = 1337;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (AuthCallback != null) {
            AuthCallback.onInstance(this);
        }
    }

    protected void onActivityResult(int requestCode, int resultCode, Intent intent) {
        super.onActivityResult(requestCode, resultCode, intent);

        Log.d("SpotifyLoginActivity", "onActivityResult");

        // Check if result comes from the correct activity
        if (requestCode == REQUEST_CODE) {
            AuthorizationResponse response = AuthorizationClient.getResponse(resultCode, intent);

            try {

                switch (response.getType()) {

                    case CODE:

                        Log.d("SpotifyLoginActivity", "CODE");

                        String code = response.getCode();

                        if (AuthCallback != null) {
                            AuthCallback.onResult("CODE", code);
                        }

                        break;

                    case TOKEN:

                        Log.d("SpotifyLoginActivity", "TOKEN");

                        String accessToken = response.getAccessToken();

                        if (AuthCallback != null) {
                            AuthCallback.onResult("TOKEN", accessToken);
                        }

                        break;

                    case ERROR:
                        // Handle error response
                        Log.d("SpotifyLoginActivity", "ERROR");

                        String error = response.getError();

                        Log.d("SpotifyLoginActivity", error);

                        if (AuthCallback != null) {
                            AuthCallback.onError(error);
                        }

                        break;

                    // Most likely auth flow was cancelled
                    default:
                        Log.d("SpotifyLoginActivity", "DEFAULT");

                        if (AuthCallback != null) {
                            AuthCallback.onError("Canceled");
                        }

                        break;
                }
                
            }
            catch (Exception e) {
                
                Log.e("SpotifyLoginActivity", e.getMessage());
                
            }

            setResult(RESULT_OK);
            finish();
        }
    }

    public void Authorize(String clientID, String redirectURI, String[] scopes) {
        AuthorizationRequest.Builder builder =
                new AuthorizationRequest.Builder(clientID, AuthorizationResponse.Type.CODE, redirectURI);

        builder.setScopes(scopes);
        AuthorizationRequest request = builder.build();

        AuthorizationClient.openLoginActivity(this, REQUEST_CODE, request);
    }
}