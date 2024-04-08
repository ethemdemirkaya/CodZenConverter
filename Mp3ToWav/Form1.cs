using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace Mp3ToWav
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void ConvertMp3ToWavWithProgressBar(string mp3FilePath, string wavFilePath)
        {
            progressBar1.Visible = true;
            label2.Visible = true;
            using (Mp3FileReader reader = new Mp3FileReader(mp3FilePath))
            {
                // Örnekleme frekansını 44100 Hz olarak ayarlayın ve 16 bit derinlik kullanın
                var waveFormat = new WaveFormat(44100, 16, 1); // Mono (tek kanallı) ses

                // PCM akışını oluşturun ve önceki WaveFormat'ten yeni WaveFormat'e dönüştürün
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = (int)reader.Length;

                    // Normalleştirme işlemi için ölçek faktörü hesaplayın
                    float max = 0;
                    byte[] tempBuffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = pcmStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                    {
                        for (int i = 0; i < bytesRead / 2; i += 2) // 16 bit için ikişer adımla ilerle
                        {
                            short sample = BitConverter.ToInt16(tempBuffer, i);
                            float sampleAbs = Math.Abs(sample);
                            if (sampleAbs > max)
                                max = sampleAbs;
                        }
                    }

                    float scale = short.MaxValue / max;

                    // PCM akışını tekrar okuyarak normalleştirme işlemi uygulayın ve WAV dosyasına yazın
                    pcmStream.Position = 0; // Akışı başa al
                    using (WaveFileWriter writer = new WaveFileWriter(wavFilePath, waveFormat))
                    {
                        while ((bytesRead = pcmStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i += 2)
                            {
                                short sample = BitConverter.ToInt16(tempBuffer, i);
                                float normalizedSample = sample * scale;
                                byte[] normalizedSampleBytes = BitConverter.GetBytes((short)normalizedSample);
                                writer.Write(normalizedSampleBytes, 0, normalizedSampleBytes.Length);
                            }
                            progressBar1.Value = (int)reader.Position;
                        }
                    }
                }
            }
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "MP3 files (*.mp3)|*.mp3";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string mp3FilePath = openFileDialog1.FileName;
                string wavFilePath = mp3FilePath.Replace(".mp3", ".wav");

                ConvertMp3ToWavWithProgressBar(mp3FilePath, wavFilePath);

                MessageBox.Show("Dönüşüm tamamlandı!");
                progressBar1.Visible = false;
                label2.Visible = false;
            }
        }
    }
}

