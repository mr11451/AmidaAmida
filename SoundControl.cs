using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AmidaAmida
{
    internal class SoundControl
    {
        static SoundControl? instance;
        private bool soundEnable = false;

        public static SoundControl Instance
        {
            get
            {
                instance ??= new SoundControl();
                return instance;
            }
        }

        public enum ID {
            BtnDown, BtnUp, BtnClick, Select, Start, UnDo, LineSet, Connect, LineNG
        }


        static readonly Dictionary<ID, string> SoundFiles = new()
        {
            { ID.BtnDown, @"resource\btnDown.wav" },
            { ID.BtnUp, @"resource\btnUp.wav" },
            { ID.BtnClick, @"resource\btnPoi.wav" },
            { ID.Select, @"resource\btnSelect.wav" },
            { ID.Start, @"resource\btnStart.wav" },
            { ID.UnDo, @"resource\btnUndo.wav" },
            { ID.LineSet, @"resource\LineSet.wav" },
            { ID.Connect, @"resource\LineConnect.wav" },
            { ID.LineNG, @"resource\LineNg.wav" }
        };

        public SoundControl()
        {
        }

        public void SetEnable()
        {
            soundEnable = true;
        }

        public void PlayID(ID id)
        {
            if (!soundEnable) return;
            Task.Run(() =>
            {
                var fileName = SoundFiles[id];
                var s = new AudioFileReader(fileName);
                var waveOut = new WaveOutEvent();
                waveOut.Init(s);
                waveOut.Play();
            });
        }
    }
}
