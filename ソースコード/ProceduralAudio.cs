using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace NovelGame2D;
/// <summary>
/// 数学的な波形計算により、BGMや効果音をプログラム上で動的生成するクラス
/// </summary>
public static class ProceduralAudio
{
    private const int SampleRate = 44100;
    /// <summary>
    /// ノベルパート用BGM（アンビエント・コード進行）を生成する
    /// </summary>
    public static SoundEffect CreateNovelBgm()
    {
        return BuildSoundEffect(16.0f, (t, sampleIdx, totalSamples) =>
        {
            // 4 chords (4.0s each)
            float chordLen = 4.0f;
            int chordIdx = (int)(t / chordLen) % 4;
            float tChord = t % chordLen;
            // 4つのコード進行 (Cmaj7 -> Am7 -> Fmaj7 -> G7)
            float[][] chordFreqs = new float[][]
            {
                new float[] { 261.63f, 329.63f, 392.00f, 493.88f }, // Cmaj7
                new float[] { 220.00f, 261.63f, 329.63f, 392.00f }, // Am7
                new float[] { 174.61f, 220.00f, 261.63f, 329.63f }, // Fmaj7
                new float[] { 196.00f, 246.94f, 293.66f, 349.23f }  // G7
            };

            float[] currentChord = chordFreqs[chordIdx];
            float left = 0f;
            float right = 0f;

            // アルペジオのエレクトリックピアノ音
            for (int i = 0; i < currentChord.Length; i++)
            {
                float freq = currentChord[i];
                float tNote = tChord - (i * 0.5f);
                if (tNote > 0)
                {
                    float env = (float)Math.Exp(-1.5f * tNote);
                    float harm = ((float)Math.Sin(2 * Math.PI * freq * t) * 0.6f +
                                  (float)Math.Sin(4 * Math.PI * freq * t) * 0.25f +
                                  (float)Math.Sin(6 * Math.PI * freq * t) * 0.1f) * env;
                    float pan = 0.3f + 0.4f * ((float)i / currentChord.Length);
                    left += harm * (1.0f - pan);
                    right += harm * pan;
                }
            }

            // 温かみのあるサブパッド音
            float baseFreq = currentChord[0] * 0.5f;
            float pad = (float)Math.Sin(2 * Math.PI * baseFreq * t) * 0.12f * (1.0f + 0.1f * (float)Math.Sin(t * 1.5f));
            left += pad;
            right += pad;

            return (left * 0.45f, right * 0.45f);
        });
    }
    /// <summary>
    /// リズムゲーム用BGM（アップテンポなサイバービート）を生成する
    /// </summary>
    public static SoundEffect CreateRhythmBgm()
    {
        float bpm = 128f;
        float beatSec = 60f / bpm;
        float barSec = beatSec * 4f;
        float totalDuration = barSec * 8; // 15秒ループ

        float[] bassFreqs = new float[] { 87.31f, 98.00f, 110.00f, 130.81f, 87.31f, 98.00f, 82.41f, 110.00f };
        float[] arpScale = new float[] { 440f, 523.25f, 659.25f, 783.99f, 880f, 783.99f, 659.25f, 523.25f };

        Random rnd = new Random(42);
        // Precompute some noise
        float[] noiseBuf = new float[SampleRate];
        for (int i = 0; i < noiseBuf.Length; i++) noiseBuf[i] = (float)(rnd.NextDouble() * 2.0 - 1.0);

        return BuildSoundEffect(totalDuration, (t, sampleIdx, totalSamples) =>
        {
            int barIdx = (int)(t / barSec) % 8;
            float tBeat = t % beatSec;
            float t16th = t % (beatSec / 4f);
            float t8th = t % (beatSec / 2f);

            float left = 0f;
            float right = 0f;

            // 1. キックドラム（4つ打ち）
            float kickEnv = (float)Math.Exp(-28f * tBeat);
            float kickPitch = 130f * (float)Math.Exp(-32f * tBeat) + 45f;
            float kick = (float)Math.Sin(2 * Math.PI * kickPitch * tBeat) * kickEnv * 0.65f;
            left += kick;
            right += kick;

            // 2. ハイハット（8分音符）
            bool isOffbeat = ((int)(t / (beatSec / 2f)) % 2) == 1;
            float hatEnv = isOffbeat ? (float)Math.Exp(-35f * t8th) : (float)Math.Exp(-65f * t8th);
            float noise = noiseBuf[sampleIdx % noiseBuf.Length] * hatEnv * (isOffbeat ? 0.22f : 0.12f);
            left += noise * (isOffbeat ? 0.8f : 0.5f);
            right += noise * (isOffbeat ? 0.6f : 0.7f);

            // 3. 16分音符のドライブベース
            float bFreq = bassFreqs[barIdx];
            float bEnv = (float)Math.Exp(-12f * t16th);
            float bass = ((float)Math.Sin(2 * Math.PI * bFreq * t) +
                          0.5f * (float)Math.Sin(4 * Math.PI * bFreq * t) +
                          0.25f * (float)Math.Sin(6 * Math.PI * bFreq * t)) * bEnv * 0.32f;
            left += bass;
            right += bass;

            // 4. シンセアルペジオ
            int arpIdx = (int)((t % (beatSec * 2f)) / (beatSec / 4f)) % arpScale.Length;
            float aFreq = arpScale[arpIdx];
            float aEnv = (float)Math.Exp(-18f * t16th);
            float lead = (float)Math.Sin(2 * Math.PI * aFreq * t) * aEnv * 0.2f;
            float pan = 0.5f + 0.35f * (float)Math.Sin(t * 8f);
            left += lead * (1f - pan);
            right += lead * pan;

            return (left * 0.5f, right * 0.5f);
        });
    }
    /// <summary>
    /// エンディング用BGM（感動的なストリングスとベル）を生成する
    /// </summary>
    public static SoundEffect CreateEndingBgm()
    {
        float[][] chords = new float[][]
        {
            new float[] { 174.61f, 220.00f, 261.63f, 329.63f }, // Fmaj7
            new float[] { 196.00f, 246.94f, 293.66f, 392.00f }, // G
            new float[] { 164.81f, 196.00f, 246.94f, 329.63f }, // Em7
            new float[] { 220.00f, 261.63f, 329.63f, 440.00f }  // Am
        };
        float chordLen = 4.0f;
        float[] melodyFreqs = new float[] { 659.25f, 783.99f, 880.00f, 987.77f, 1046.50f, 987.77f, 880.00f, 783.99f };

        return BuildSoundEffect(16.0f, (t, sampleIdx, totalSamples) =>
        {
            int chordIdx = (int)(t / chordLen) % 4;
            float[] chord = chords[chordIdx];

            float left = 0f;
            float right = 0f;

            // 豊かなストリングス音
            for (int i = 0; i < chord.Length; i++)
            {
                float freq = chord[i];
                float vib = 1f + 0.003f * (float)Math.Sin(t * 5f + i);
                left += (float)Math.Sin(2 * Math.PI * (freq * 0.998f) * vib * t) * 0.11f;
                right += (float)Math.Sin(2 * Math.PI * (freq * 1.002f) * vib * t) * 0.11f;
            }

            // 高音のベルメロディ
            int melIdx = (int)(t / 2.0f) % melodyFreqs.Length;
            float tMel = t % 2.0f;
            if (tMel < 1.6f)
            {
                float melFreq = melodyFreqs[melIdx];
                float env = (float)Math.Exp(-2.2f * tMel);
                float bell = ((float)Math.Sin(2 * Math.PI * melFreq * t) +
                              0.35f * (float)Math.Sin(4 * Math.PI * melFreq * t)) * env * 0.22f;
                left += bell * 0.6f;
                right += bell * 0.6f;
            }

            return (left * 0.45f, right * 0.45f);
        });
    }
    // --- 各種効果音（SE）生成メソッド ---
    public static SoundEffect CreateClickSe()
    {
        return BuildSoundEffect(0.05f, (t, idx, total) =>
        {
            float env = (float)Math.Exp(-90f * t);
            float val = (float)Math.Sin(2 * Math.PI * 1400f * t) * env * 0.4f;
            return (val, val);
        });
    }

    public static SoundEffect CreateHitSe(bool isPerfect)
    {
        float duration = isPerfect ? 0.35f : 0.25f;
        return BuildSoundEffect(duration, (t, idx, total) =>
        {
            if (isPerfect)
            {
                float env = (float)Math.Exp(-11f * t);
                float s1 = (float)Math.Sin(2 * Math.PI * 1046.5f * t); // C6
                float s2 = (float)Math.Sin(2 * Math.PI * 1318.5f * t); // E6
                float s3 = (float)Math.Sin(2 * Math.PI * 2093.0f * t); // C7 sparkle
                float val = (s1 * 0.4f + s2 * 0.35f + s3 * 0.25f) * env * 0.6f;
                return (val * 0.85f, val);
            }
            else
            {
                float env = (float)Math.Exp(-14f * t);
                float s1 = (float)Math.Sin(2 * Math.PI * 880f * t);  // A5
                float s2 = (float)Math.Sin(2 * Math.PI * 1174f * t); // D6
                float val = (s1 * 0.5f + s2 * 0.35f) * env * 0.5f;
                return (val, val * 0.85f);
            }
        });
    }

    public static SoundEffect CreateMissSe()
    {
        return BuildSoundEffect(0.18f, (t, idx, total) =>
        {
            float env = (float)Math.Exp(-26f * t);
            float pitch = 180f * (float)Math.Exp(-16f * t) + 60f;
            float val = (float)Math.Sin(2 * Math.PI * pitch * t) * env * 0.4f;
            return (val, val);
        });
    }

    public static SoundEffect CreateSyncSe()
    {
        return BuildSoundEffect(1.6f, (t, idx, total) =>
        {
            float env = (float)Math.Pow(Math.Sin(Math.PI * (t / 1.6f)), 0.8);
            float[] freqs = new float[] { 261.63f, 329.63f, 392.0f, 523.25f, 659.25f, 1046.5f };
            float val = 0f;
            for (int i = 0; i < freqs.Length; i++)
            {
                float f = freqs[i];
                val += (float)Math.Sin(2 * Math.PI * f * (1.0f + 0.005f * Math.Sin(t * 8f + i)) * t) * (1.0f / freqs.Length);
            }
            val *= env * 0.65f;
            return (val, val);
        });
    }
    /// <summary>
    /// バイト配列を組み立ててWAV形式のSoundEffectオブジェクトを構築するヘルパーメソッド
    /// </summary>
    private static SoundEffect BuildSoundEffect(float durationSec, Func<float, int, int, (float Left, float Right)> sampleGen)
    {
        int numSamples = (int)(SampleRate * durationSec);
        int subchunk2Size = numSamples * 2 * 2; // 2 channels * 16 bits (2 bytes)
        int chunkSize = 36 + subchunk2Size;

        using (MemoryStream ms = new MemoryStream(44 + subchunk2Size))
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(chunkSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);          // Subchunk1Size (16 for PCM)
            bw.Write((short)1);    // AudioFormat (1 = PCM)
            bw.Write((short)2);    // NumChannels (2 = Stereo)
            bw.Write(SampleRate);  // SampleRate
            bw.Write(SampleRate * 2 * 2); // ByteRate
            bw.Write((short)4);    // BlockAlign
            bw.Write((short)16);   // BitsPerSample

            // data subchunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(subchunk2Size);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                var (l, r) = sampleGen(t, i, numSamples);

                // clamp
                l = Math.Clamp(l, -1.0f, 1.0f);
                r = Math.Clamp(r, -1.0f, 1.0f);

                short sL = (short)(l * 32767f);
                short sR = (short)(r * 32767f);

                bw.Write(sL);
                bw.Write(sR);
            }

            bw.Flush();
            ms.Position = 0;
            return SoundEffect.FromStream(ms);
        }
    }
}
