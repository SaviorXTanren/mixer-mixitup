using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class AmazonPollyService : ITextToSpeechService
    {
        public const string AccessKey = "AKIAWQIVQ74JEQWGFQ3A";

        public static readonly IEnumerable<TextToSpeechVoice> AvailableVoices = new List<TextToSpeechVoice>()
        {
            new TextToSpeechVoice("Aditi:standard", "Aditi"),
            new TextToSpeechVoice("Amy:standard", "Amy"),
            new TextToSpeechVoice("Astrid:standard", "Astrid"),
            new TextToSpeechVoice("Bianca:standard", "Bianca"),
            new TextToSpeechVoice("Brian:standard", "Brian"),
            new TextToSpeechVoice("Camila:standard", "Camila"),
            new TextToSpeechVoice("Carla:standard", "Carla"),
            new TextToSpeechVoice("Carmen:standard", "Carmen"),
            new TextToSpeechVoice("Celine:standard", "Céline"),
            new TextToSpeechVoice("Chantal:standard", "Chantal"),
            new TextToSpeechVoice("Conchita:standard", "Conchita"),
            new TextToSpeechVoice("Cristiano:standard", "Cristiano"),
            new TextToSpeechVoice("Dora:standard", "Dóra"),
            new TextToSpeechVoice("Emma:standard", "Emma"),
            new TextToSpeechVoice("Enrique:standard", "Enrique"),
            new TextToSpeechVoice("Ewa:standard", "Ewa"),
            new TextToSpeechVoice("Filiz:standard", "Filiz"),
            new TextToSpeechVoice("Geraint:standard", "Geraint"),
            new TextToSpeechVoice("Giorgio:standard", "Giorgio"),
            new TextToSpeechVoice("Gwyneth:standard", "Gwyneth"),
            new TextToSpeechVoice("Hans:standard", "Hans"),
            new TextToSpeechVoice("Ines:standard", "Inês"),
            new TextToSpeechVoice("Ivy:standard", "Ivy"),
            new TextToSpeechVoice("Jacek:standard", "Jacek"),
            new TextToSpeechVoice("Jan:standard", "Jan"),
            new TextToSpeechVoice("Joanna:standard", "Joanna"),
            new TextToSpeechVoice("Joey:standard", "Joey"),
            new TextToSpeechVoice("Justin:standard", "Justin"),
            new TextToSpeechVoice("Karl:standard", "Karl"),
            new TextToSpeechVoice("Kendra:standard", "Kendra"),
            new TextToSpeechVoice("Kimberly:standard", "Kimberly"),
            new TextToSpeechVoice("Lea:standard", "Léa"),
            new TextToSpeechVoice("Liv:standard", "Liv"),
            new TextToSpeechVoice("Lotte:standard", "Lotte"),
            new TextToSpeechVoice("Lucia:standard", "Lucia"),
            new TextToSpeechVoice("Lupe:standard", "Lupe"),
            new TextToSpeechVoice("Mads:standard", "Mads"),
            new TextToSpeechVoice("Maja:standard", "Maja"),
            new TextToSpeechVoice("Marlene:standard", "Marlene"),
            new TextToSpeechVoice("Mathieu:standard", "Mathieu"),
            new TextToSpeechVoice("Matthew:standard", "Matthew"),
            new TextToSpeechVoice("Maxim:standard", "Maxim"),
            new TextToSpeechVoice("Mia:standard", "Mia"),
            new TextToSpeechVoice("Miguel:standard", "Miguel"),
            new TextToSpeechVoice("Mizuki:standard", "Mizuki"),
            new TextToSpeechVoice("Naja:standard", "Naja"),
            new TextToSpeechVoice("Nicole:standard", "Nicole"),
            new TextToSpeechVoice("Penelope:standard", "Penélope"),
            new TextToSpeechVoice("Raveena:standard", "Raveena"),
            new TextToSpeechVoice("Ricardo:standard", "Ricardo"),
            new TextToSpeechVoice("Ruben:standard", "Ruben"),
            new TextToSpeechVoice("Russell:standard", "Russell"),
            new TextToSpeechVoice("Salli:standard", "Salli"),
            new TextToSpeechVoice("Seoyeon:standard", "Seoyeon"),
            new TextToSpeechVoice("Takumi:standard", "Takumi"),
            new TextToSpeechVoice("Tatyana:standard", "Tatyana"),
            new TextToSpeechVoice("Vicki:standard", "Vicki"),
            new TextToSpeechVoice("Vitoria:standard", "Vitória"),
            new TextToSpeechVoice("Zeina:standard", "Zeina"),
            new TextToSpeechVoice("Zhiyu:standard", "Zhiyu"),
        };

        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.AmazonPolly; } }

        public int VolumeMinimum { get { return 0; } }

        public int VolumeMaximum { get { return 100; } }

        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }

        public int PitchMaximum { get { return 0; } }

        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }

        public int RateMaximum { get { return 0; } }

        public int RateDefault { get { return 0; } }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return AmazonPollyService.AvailableVoices; }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool waitForFinish)
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials(AmazonPollyService.AccessKey, ServiceManager.Get<SecretsService>().GetSecret("AmazonAWSSecretKey"));
            using (AmazonPollyClient client = new AmazonPollyClient(credentials, RegionEndpoint.USWest2))
            {
                string[] splits = voice.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length == 2)
                {
                    string voiceID = splits[0];
                    string voiceEngine = splits[1];

                    SynthesizeSpeechResponse response = await client.SynthesizeSpeechAsync(new SynthesizeSpeechRequest()
                    {
                        VoiceId = voiceID,
                        Engine = new Engine(voiceEngine),
                        OutputFormat = OutputFormat.Mp3,
                        Text = text
                    });

                    if (response.HttpStatusCode == HttpStatusCode.OK && response.AudioStream != null)
                    {
                        MemoryStream stream = new MemoryStream();
                        using (response.AudioStream)
                        {
                            response.AudioStream.CopyTo(stream);
                            stream.Position = 0;
                        }
                        await ServiceManager.Get<IAudioService>().PlayMP3Stream(stream, volume, outputDevice, waitForFinish: waitForFinish);
                    }
                }
            }
        }

        private async Task GenerateVoicesList()
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials(AmazonPollyService.AccessKey, ServiceManager.Get<SecretsService>().GetSecret("AmazonAWSSecretKey"));
            using (AmazonPollyClient client = new AmazonPollyClient(credentials, RegionEndpoint.USWest2))
            {
                DescribeVoicesResponse voices = await client.DescribeVoicesAsync(new DescribeVoicesRequest());

                StringBuilder results = new StringBuilder();
                foreach (Voice v in voices.Voices)
                {
                    if (v.SupportedEngines.Count > 1)
                    {
                        foreach (string engine in v.SupportedEngines)
                        {
                            results.AppendLine($"new TextToSpeechVoice(\"{v.Id.Value}:{engine}\", \"{v.Name} - {engine}\"),");
                        }
                    }
                    else
                    {
                        results.AppendLine($"new TextToSpeechVoice(\"{v.Id.Value}:{v.SupportedEngines.First()}\", \"{v.Name}\"),");
                    }
                }

                await ServiceManager.Get<IFileService>().SaveFile("S:\\voices.txt", results.ToString());
            }
        }
    }
}
