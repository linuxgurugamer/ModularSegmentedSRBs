
using UnityEngine;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AudibleAlertCore : MonoBehaviour
    {
        static public AudibleAlertCore fetch;

        internal AlarmSoundPlayer soundplayer; // = new AlarmSoundPlayer();

        internal const string SOUND_DIR = "ModularSegmentedSRBs/Audio/";
        internal static string normalAlert = "Siren_Noise";
        bool Paused = false;
        bool soundPlaying = false;
        float startTime;
        float endTime;
        int cnt = 0;

        Log Log = new Log("ModularSegmentedSRBs.AlertCore");

        void Awake()
        {
            soundplayer = new AlarmSoundPlayer();
        }

        void Start()
        {
            fetch = this;
            soundplayer.Initialize(SOUND_DIR + normalAlert);
            Paused = true; // start out paused

            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnpause);
        }

        void StopSound()
        {
            if (soundplayer.SoundPlaying())
                soundplayer.StopSound();
        }

        void OnPause()
        {
            Paused = true;
            if (soundplayer == null)
                return;
            StopSound();

        }

        void OnDestroy()
        {
            StopSound();
            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnpause);
        }

        void OnUnpause()
        {
            Paused = false;
        }

        public void ActivateAlarm(int len)
        {
            Paused = false;
            startTime = Time.time;
            endTime = startTime + len;
            soundPlaying = true;
        }

        public void DeactivateAlarm()
        {
            OnPause();
        }

        void FixedUpdate()
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().alarm && cnt++ > 10)
            {
                cnt = 0;

                if (HighLogic.LoadedSceneIsFlight && !Paused && soundPlaying)
                {
                    if (Time.time < endTime)
                    {
                        soundplayer.SetVolume(HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().masterVolume);
                        if (soundplayer != null && !soundplayer.SoundPlaying()) //If the sound isn't playing, play the sound.
                        {
                            {
                                soundplayer.PlaySound(); //Plays sound
                            }
                        }
                    }
                    else
                    {
                        soundPlaying = false;
                        OnPause();
                    }
                }
            }
        }
    }
}
