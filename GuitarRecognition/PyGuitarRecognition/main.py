#-*- encoding=utf-8
import pyaudio
import matplotlib.pyplot as plt
import numpy as np

        
def handle_close(event):
    event.canvas.has_been_closed = True

if __name__ == "__main__":
    ################################################################################
    # ready to device driver
    ################################################################################
    chunk = 4096
    format = pyaudio.paFloat32
    channels = 1
    fs = 192000
    record_seconds = 60 * 60 # 1 hour
    
    p = pyaudio.PyAudio()

    for index in range(0, p.get_device_count()):
        info = p.get_device_info_by_index(index)
        print("{:>3d} {}".format(info['index'], info['name']))
    
    number_str = input("Please, an input text for default device > ").rstrip()
    number = int(number_str)

    default_device = p.get_device_info_by_index(number)
    stream = p.open(format=format, 
        channels=channels, 
        rate = fs,
        input=True,
        output=False,
        input_device_index=number,
        frames_per_buffer=chunk)

    ################################################################################
    # ready to subplots
    ################################################################################
    plt.ion()
    fig = plt.figure(figsize=(8, 8))
    fig.canvas.mpl_connect("close_event", handle_close)
    fig.canvas.has_been_closed = False

    waveform_area = plt.subplot(2, 1, 1)
    fourier_area = plt.subplot(2, 1, 2)
    plt.subplots_adjust(wspace=0.5, hspace=0.5)

    x = np.linspace(0, 10, 100)
    y = np.cos(x)

    waveform_area.set_title('recorded waveform [short size]')
    waveform_area.set_xlabel("time [sec]")
    waveform_area.set_ylabel("relative")
    waveform_line, = waveform_area.plot(x, y)

    fourier_area.set_title("fourier transform")
    fourier_area.set_xlabel("frequency [Hz]")
    fourier_area.set_ylabel("power spectrum dencity")

    while stream.is_active() and not fig.canvas.has_been_closed:
        buffer = np.frombuffer(stream.read(chunk), dtype="f4")

        dt = 1.0 / float(fs)
        times_x = np.arange(0, len(buffer) * dt, dt)
        waveform_line.set_xdata(times_x)
        waveform_line.set_ydata(buffer)
        waveform_area.set_xlim(0, len(buffer) * dt)

        fig.tight_layout()
        fig.canvas.draw()
        fig.canvas.flush_events()
    
    fig.clear()
    plt.close()

    stream.stop_stream()
    stream.close()

    p.terminate()