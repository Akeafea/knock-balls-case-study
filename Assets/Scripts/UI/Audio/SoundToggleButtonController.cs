using KnockBallsCase.Gameplay.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace KnockBallsCase.UI.Audio
{
    public sealed class SoundToggleButtonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button soundButton;
        [SerializeField] private Graphic buttonGraphic;

        [Header("Colors")]
        [SerializeField] private Color soundOnColor = Color.green;
        [SerializeField] private Color soundOffColor = Color.gray;

        private void Awake()
        {
            if (soundButton == null)
            {
                soundButton = GetComponent<Button>();
            }

            if (buttonGraphic == null && soundButton != null)
            {
                buttonGraphic = soundButton.targetGraphic;
            }
        }

        private void OnEnable()
        {
            if (soundButton != null)
            {
                soundButton.onClick.AddListener(OnSoundButtonClicked);
            }

            RefreshVisual();
        }

        private void OnDisable()
        {
            if (soundButton != null)
            {
                soundButton.onClick.RemoveListener(OnSoundButtonClicked);
            }
        }

        private void OnSoundButtonClicked()
        {
            GlobalSfx2D.ToggleSoundEnabled();
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (buttonGraphic == null)
            {
                return;
            }

            buttonGraphic.color = GlobalSfx2D.IsSoundEnabled ? soundOnColor : soundOffColor;
        }
    }
}
