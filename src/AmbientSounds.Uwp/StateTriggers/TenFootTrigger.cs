using Windows.UI.Xaml;

#nullable enable

namespace AmbientSounds.StateTriggers
{
    /// <summary>
    /// State trigger for whether or not
    /// the current device is in ten foot mode.
    /// </summary>
    public class TenFootTrigger : StateTriggerBase
    {
        public TenFootTrigger()
        {
            SetActive(App.IsTenFoot);
        }
    }
}
