package ca.darrylday.spotifyunity;

public class Utils {
    public interface AuthCallback {
        void onInstance(SpotifyLoginActivity instance);
        void onResult(String type, String token);
        void onError(String error);
    }
}