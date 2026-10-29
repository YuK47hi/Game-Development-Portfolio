import os
import wave
import struct
import math
import random

SAMPLE_RATE = 44100

def create_wav(filename, duration, sample_func, stereo=True):
    num_samples = int(SAMPLE_RATE * duration)
    out_dir = os.path.dirname(filename)
    if out_dir and not os.path.exists(out_dir):
        os.makedirs(out_dir, exist_ok=True)
        
    with wave.open(filename, 'w') as wav:
        n_channels = 2 if stereo else 1
        wav.setparams((n_channels, 2, SAMPLE_RATE, num_samples, 'NONE', 'not compressed'))
        frames = bytearray()
        
        for i in range(num_samples):
            t = i / SAMPLE_RATE
            if stereo:
                left, right = sample_func(t, i, num_samples)
                # clamp
                left = max(-1.0, min(1.0, left))
                right = max(-1.0, min(1.0, right))
                s_left = int(left * 32767)
                s_right = int(right * 32767)
                frames.extend(struct.pack('<hh', s_left, s_right))
            else:
                val = sample_func(t, i, num_samples)
                val = max(-1.0, min(1.0, val))
                s_val = int(val * 32767)
                frames.extend(struct.pack('<h', s_val))
                
        wav.writeframes(frames)
    print(f"[Created] {filename} ({duration:.2f}s)")

# Helper note frequencies
NOTES = {
    'C3': 130.81, 'D3': 146.83, 'E3': 164.81, 'F3': 174.61, 'G3': 196.00, 'A3': 220.00, 'B3': 246.94,
    'C4': 261.63, 'D4': 293.66, 'E4': 329.63, 'F4': 349.23, 'G4': 392.00, 'A4': 440.00, 'B4': 493.88,
    'C5': 523.25, 'D5': 587.33, 'E5': 659.25, 'F5': 698.46, 'G5': 783.99, 'A5': 880.00, 'B5': 987.77,
    'C6': 1046.50
}

def tone(freq, t, decay=None):
    if decay:
        return math.sin(2 * math.pi * freq * t) * math.exp(-decay * t)
    return math.sin(2 * math.pi * freq * t)

# --- BGM Novel: Gentle Ambient E-Piano (16 seconds loop) ---
def gen_bgm_novel():
    chords = [
        ['C4', 'E4', 'G4', 'B4'], # Cmaj7
        ['A3', 'C4', 'E4', 'G4'], # Am7
        ['F3', 'A3', 'C4', 'E4'], # Fmaj7
        ['G3', 'B3', 'D4', 'F4']  # G7
    ]
    chord_len = 4.0 # 4 seconds each = 16s total
    
    def sample(t, idx, total):
        # determine which chord
        chord_idx = int(t / chord_len) % len(chords)
        t_chord = t % chord_len
        current_chord = chords[chord_idx]
        
        sig_l = 0.0
        sig_r = 0.0
        
        # Soft electric piano arpeggios
        for i, note in enumerate(current_chord):
            freq = NOTES[note]
            # trigger note every 1.0 second or staggered
            t_note = (t_chord - i * 0.5)
            if t_note > 0:
                env = math.exp(-1.5 * t_note)
                harm = (math.sin(2 * math.pi * freq * t) * 0.6 +
                        math.sin(4 * math.pi * freq * t) * 0.25 +
                        math.sin(6 * math.pi * freq * t) * 0.1) * env
                pan = 0.3 + 0.4 * (i / len(current_chord))
                sig_l += harm * (1.0 - pan)
                sig_r += harm * pan
                
        # Warm sub pad
        base_freq = NOTES[current_chord[0]] / 2
        pad = math.sin(2 * math.pi * base_freq * t) * 0.12 * (1.0 + 0.1 * math.sin(t * 1.5))
        sig_l += pad
        sig_r += pad
        
        return sig_l * 0.5, sig_r * 0.5

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\bgm_novel.wav", 16.0, sample)

# --- BGM Rhythm: 128 BPM Cyber Electronic (15.0 seconds = 8 bars) ---
def gen_bgm_rhythm():
    bpm = 128.0
    beat_sec = 60.0 / bpm
    bar_sec = beat_sec * 4.0 # ~1.875s
    duration = bar_sec * 8 # 15.0s
    
    bass_notes = ['F3', 'G3', 'A3', 'C4', 'F3', 'G3', 'E3', 'A3']
    
    def sample(t, idx, total):
        bar_idx = int(t / bar_sec) % 8
        t_beat = t % beat_sec
        t_bar = t % bar_sec
        beat_idx = int(t_bar / beat_sec)
        
        sig_l = 0.0
        sig_r = 0.0
        
        # 1. Kick on 4 on the floor
        kick_env = math.exp(-25.0 * t_beat)
        kick_pitch = 140.0 * math.exp(-30.0 * t_beat) + 45.0
        kick = math.sin(2 * math.pi * kick_pitch * t_beat) * kick_env * 0.7
        sig_l += kick
        sig_r += kick
        
        # 2. Hi-hat on off-beats (8th notes)
        t_8th = (t % (beat_sec / 2.0))
        if int(t / (beat_sec / 2.0)) % 2 == 1:
            # offbeat open hat
            hat_env = math.exp(-35.0 * t_8th)
            noise = (random.random() * 2.0 - 1.0) * hat_env * 0.25
            sig_l += noise * 0.8
            sig_r += noise * 0.6
        else:
            # tight closed hat
            hat_env = math.exp(-60.0 * t_8th)
            noise = (random.random() * 2.0 - 1.0) * hat_env * 0.15
            sig_l += noise * 0.5
            sig_r += noise * 0.7
            
        # 3. Driving 16th-note synth bass
        t_16th = t % (beat_sec / 4.0)
        bass_freq = NOTES[bass_notes[bar_idx]] / 2.0
        bass_env = math.exp(-12.0 * t_16th)
        saw_approx = (math.sin(2 * math.pi * bass_freq * t) +
                      0.5 * math.sin(4 * math.pi * bass_freq * t) +
                      0.25 * math.sin(6 * math.pi * bass_freq * t)) * bass_env * 0.35
        sig_l += saw_approx
        sig_r += saw_approx
        
        # 4. Lead synth arpeggio (16th notes fast)
        arp_scale = ['A4', 'C5', 'E5', 'G5', 'A5', 'G5', 'E5', 'C5']
        arp_idx = int((t % (beat_sec * 2.0)) / (beat_sec / 4.0)) % len(arp_scale)
        lead_freq = NOTES[arp_scale[arp_idx]]
        lead_env = math.exp(-18.0 * t_16th)
        lead = math.sin(2 * math.pi * lead_freq * t) * lead_env * 0.22
        
        pan = 0.5 + 0.35 * math.sin(t * 8.0)
        sig_l += lead * (1.0 - pan)
        sig_r += lead * pan
        
        return sig_l * 0.6, sig_r * 0.6

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\bgm_rhythm.wav", duration, sample)

# --- BGM Ending: Uplifting & Moving (16 seconds loop) ---
def gen_bgm_ending():
    chords = [
        ['F3', 'A3', 'C4', 'E4'], # Fmaj7
        ['G3', 'B3', 'D4', 'G4'], # G
        ['E3', 'G3', 'B3', 'E4'], # Em7
        ['A3', 'C4', 'E4', 'A4']  # Am
    ]
    chord_len = 4.0
    
    def sample(t, idx, total):
        chord_idx = int(t / chord_len) % len(chords)
        t_chord = t % chord_len
        current_chord = chords[chord_idx]
        
        sig_l = 0.0
        sig_r = 0.0
        
        # Lush string pad
        for i, note in enumerate(current_chord):
            freq = NOTES[note]
            # slight detune for stereo width
            vibrato = 1.0 + 0.003 * math.sin(t * 5.0 + i)
            s_l = math.sin(2 * math.pi * (freq * 0.998) * vibrato * t) * 0.12
            s_r = math.sin(2 * math.pi * (freq * 1.002) * vibrato * t) * 0.12
            sig_l += s_l
            sig_r += s_r
            
        # Bell-like high melody
        melody_notes = ['E5', 'G5', 'A5', 'B5', 'C6', 'B5', 'A5', 'G5']
        mel_idx = int(t / 2.0) % len(melody_notes)
        t_mel = t % 2.0
        if t_mel < 1.5:
            mel_freq = NOTES[melody_notes[mel_idx]]
            mel_env = math.exp(-2.5 * t_mel)
            bell = (math.sin(2 * math.pi * mel_freq * t) +
                    0.3 * math.sin(4 * math.pi * mel_freq * t)) * mel_env * 0.2
            sig_l += bell * 0.6
            sig_r += bell * 0.6
            
        return sig_l * 0.5, sig_r * 0.5

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\bgm_ending.wav", 16.0, sample)

# --- Sound Effects (SE) ---
def gen_sfx():
    # 1. se_click.wav (UI click)
    def sfx_click(t, idx, total):
        env = math.exp(-80.0 * t)
        val = math.sin(2 * math.pi * 1200.0 * t) * env * 0.5
        return val, val
    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\se_click.wav", 0.06, sfx_click)

    # 2. se_perfect.wav (Bright dual chime)
    def sfx_perfect(t, idx, total):
        env = math.exp(-10.0 * t)
        s1 = math.sin(2 * math.pi * 1046.5 * t) # C6
        s2 = math.sin(2 * math.pi * 1318.5 * t) # E6
        s3 = math.sin(2 * math.pi * 2093.0 * t) # C7 sparkle
        val = (s1 * 0.4 + s2 * 0.35 + s3 * 0.2) * env * 0.7
        return val * 0.8, val

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\se_perfect.wav", 0.35, sfx_perfect)

    # 3. se_great.wav (Pleasant chime)
    def sfx_great(t, idx, total):
        env = math.exp(-12.0 * t)
        s1 = math.sin(2 * math.pi * 880.0 * t) # A5
        s2 = math.sin(2 * math.pi * 1174.6 * t) # D6
        val = (s1 * 0.5 + s2 * 0.3) * env * 0.6
        return val, val * 0.8

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\se_great.wav", 0.28, sfx_great)

    # 4. se_miss.wav (Damped low thud)
    def sfx_miss(t, idx, total):
        env = math.exp(-25.0 * t)
        pitch = 180.0 * math.exp(-15.0 * t) + 60.0
        val = math.sin(2 * math.pi * pitch * t) * env * 0.4
        return val, val

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\se_miss.wav", 0.2, sfx_miss)

    # 5. se_sync.wav (Resonance chord sweep)
    def sfx_sync(t, idx, total):
        env = math.sin(math.pi * (t / 1.5)) ** 0.8
        freqs = [261.63, 329.63, 392.0, 523.25, 659.25, 1046.5] # C major chord sweep
        val = 0.0
        for i, f in enumerate(freqs):
            val += math.sin(2 * math.pi * f * (1.0 + 0.005 * math.sin(t * 8.0 + i)) * t) * (1.0 / len(freqs))
        val *= env * 0.7
        return val, val

    create_wav(r"E:\GAME\NovelGame2D\Content\Audio\se_sync.wav", 1.5, sfx_sync)

if __name__ == '__main__':
    print("Generating game audio files...")
    gen_bgm_novel()
    gen_bgm_rhythm()
    gen_bgm_ending()
    gen_sfx()
    print("All audio files generated successfully!")
