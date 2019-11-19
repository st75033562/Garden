using DG.Tweening;
using UnityEngine.UI;

namespace Gameboard
{
    [ObjectActionParameter(1)]
    public class ObjectActionSetSliderValue : ObjectAction
    {
        public Slider m_slider;

        public override void Execute(object o, params string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            float value;
            if (float.TryParse(args[0], out value))
            {
                m_slider.DOValue(value, 0.15f);
            }
        }

        public override void Stop()
        {
            m_slider.DOKill();
            m_slider.value = m_slider.maxValue;
        }
    }
}
