package ca.darrylday.spotifyandroidtest;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.spotify.sdk.android.auth.AuthorizationClient;
import com.spotify.sdk.android.auth.AuthorizationRequest;
import com.spotify.sdk.android.auth.AuthorizationResponse;

public class SpotifyLoginActivity extends Activity {

    public static Utils.TokenCallback TokenCallback;

    private static final int REQUEST_CODE = 1337;
    private static final String CLIENT_ID = "a1e7f821360540b6ac02c8c6366229ca";
    private static final String REDIRECT_URI = "http://ca.darrylday.spotifyunity/callback";

    public static void SetTokenCallback(Utils.TokenCallback callback) {
        TokenCallback = callback;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onStart() {
        super.onStart();

        AuthorizationRequest.Builder builder =
                new AuthorizationRequest.Builder(CLIENT_ID, AuthorizationResponse.Type.TOKEN, REDIRECT_URI);

        builder.setScopes(new String[]{"user-read-playback-state", "app-remote-control", "playlist-read-private", "playlist-read-collaborative", "user-read-playback-position", "user-library-modify", "playlist-modify-private", "playlist-modify-public", "user-read-email", "user-top-read", "user-read-recently-played", "user-library-read"});
        AuthorizationRequest request = builder.build();

        AuthorizationClient.openLoginActivity(this, REQUEST_CODE, request);
    }

    protected void onActivityResult(int requestCode, int resultCode, Intent intent) {
        super.onActivityResult(requestCode, resultCode, intent);

        Log.d("SpotifyLoginActivity", "onActivityResult");

        // Check if result comes from the correct activity
        if (requestCode == REQUEST_CODE) {
            AuthorizationResponse response = AuthorizationClient.getResponse(resultCode, intent);

            Intent resultIntent = new Intent();

            switch (response.getType()) {
                // Response was successful and contains auth token
                case TOKEN:

                    String token = response.getAccessToken();
                    resultIntent.putExtra("AccessToken", token);
                    setResult(RESULT_OK, resultIntent);

                    if (TokenCallback != null) {
                        TokenCallback.onToken(token);
                    }

                    break;

                // Auth flow returned an error
                case ERROR:
                    // Handle error response
                    Log.d("SpotifyLoginActivity", "ERROR");

                    resultIntent.putExtra("Error", response.getError());
                    setResult(2, resultIntent);

                    break;

                // Most likely auth flow was cancelled
                default:
                    Log.d("SpotifyLoginActivity", "DEFAULT");

                    setResult(3);

                    break;
            }

            finish();
        }
    }

}