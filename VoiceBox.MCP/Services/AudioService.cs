using NAudio.Wave;

namespace VoiceBox.MCP.Services;

public class AudioService
{
    private const int SampleRate = 16000;
    private const int Channels = 1;
    private const int BitsPerSample = 16;
    private const double SilenceThreshold = 0.01;
    private const int SilenceDurationMs = 1500;
    private const int MaxRecordingSeconds = 30;

    public async Task<byte[]> RecordUntilSilenceAsync(CancellationToken cancellationToken = default)
    {
        using var waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
            BufferMilliseconds = 100
        };

        using var memoryStream = new MemoryStream();
        using var writer = new WaveFileWriter(memoryStream, waveIn.WaveFormat);

        var silenceStart = DateTime.MinValue;
        var hasDetectedSpeech = false;
        var recordingComplete = new TaskCompletionSource<bool>();

        waveIn.DataAvailable += (sender, e) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                recordingComplete.TrySetResult(true);
                return;
            }

            writer.Write(e.Buffer, 0, e.BytesRecorded);

            var rms = CalculateRms(e.Buffer, e.BytesRecorded);

            if (rms > SilenceThreshold)
            {
                hasDetectedSpeech = true;
                silenceStart = DateTime.MinValue;
            }
            else if (hasDetectedSpeech)
            {
                if (silenceStart == DateTime.MinValue)
                {
                    silenceStart = DateTime.UtcNow;
                }
                else if ((DateTime.UtcNow - silenceStart).TotalMilliseconds >= SilenceDurationMs)
                {
                    recordingComplete.TrySetResult(true);
                }
            }
        };

        waveIn.RecordingStopped += (sender, e) =>
        {
            recordingComplete.TrySetResult(true);
        };

        waveIn.StartRecording();

        // Timeout after max recording duration
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(MaxRecordingSeconds), cancellationToken);
        await Task.WhenAny(recordingComplete.Task, timeoutTask);

        waveIn.StopRecording();
        writer.Flush();

        return memoryStream.ToArray();
    }

    private static double CalculateRms(byte[] buffer, int bytesRecorded)
    {
        double sum = 0;
        int sampleCount = bytesRecorded / 2;

        for (int i = 0; i < bytesRecorded; i += 2)
        {
            short sample = BitConverter.ToInt16(buffer, i);
            double normalized = sample / 32768.0;
            sum += normalized * normalized;
        }

        return Math.Sqrt(sum / sampleCount);
    }
}