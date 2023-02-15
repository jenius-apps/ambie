namespace AmbientSounds.Tools;

public interface IMediaPlayerFactory
{
    public IMediaPlayer CreatePlayer(bool disableDefaultSystemControls = false);
}
