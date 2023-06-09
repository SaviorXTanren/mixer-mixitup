using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class AmazonPollyService : ITextToSpeechService
    {
        public const string AccessKey = "AKIA6LQACJSKZSQUOSOM";

        public static readonly IEnumerable<TextToSpeechVoice> AvailableVoices = new List<TextToSpeechVoice>()
        {
            new TextToSpeechVoice("Aditi:standard", "Aditi"),
            new TextToSpeechVoice("Adriano:neural", "Adriano"),
            new TextToSpeechVoice("Amy:neural", "Amy - neural"),
            new TextToSpeechVoice("Amy:standard", "Amy - standard"),
            new TextToSpeechVoice("Andres:neural", "Andrés"),
            new TextToSpeechVoice("Aria:neural", "Aria"),
            new TextToSpeechVoice("Arlet:neural", "Arlet"),
            new TextToSpeechVoice("Arthur:neural", "Arthur"),
            new TextToSpeechVoice("Astrid:standard", "Astrid"),
            new TextToSpeechVoice("Ayanda:neural", "Ayanda"),
            new TextToSpeechVoice("Bianca:neural", "Bianca - neural"),
            new TextToSpeechVoice("Bianca:standard", "Bianca - standard"),
            new TextToSpeechVoice("Brian:neural", "Brian - neural"),
            new TextToSpeechVoice("Brian:standard", "Brian - standard"),
            new TextToSpeechVoice("Camila:neural", "Camila - neural"),
            new TextToSpeechVoice("Camila:standard", "Camila - standard"),
            new TextToSpeechVoice("Carla:standard", "Carla"),
            new TextToSpeechVoice("Carmen:standard", "Carmen"),
            new TextToSpeechVoice("Celine:standard", "Céline"),
            new TextToSpeechVoice("Chantal:standard", "Chantal"),
            new TextToSpeechVoice("Conchita:standard", "Conchita"),
            new TextToSpeechVoice("Cristiano:standard", "Cristiano"),
            new TextToSpeechVoice("Daniel:neural", "Daniel"),
            new TextToSpeechVoice("Dora:standard", "Dóra"),
            new TextToSpeechVoice("Elin:neural", "Elin"),
            new TextToSpeechVoice("Emma:neural", "Emma - neural"),
            new TextToSpeechVoice("Emma:standard", "Emma - standard"),
            new TextToSpeechVoice("Enrique:standard", "Enrique"),
            new TextToSpeechVoice("Ewa:standard", "Ewa"),
            new TextToSpeechVoice("Filiz:standard", "Filiz"),
            new TextToSpeechVoice("Gabrielle:neural", "Gabrielle"),
            new TextToSpeechVoice("Geraint:standard", "Geraint"),
            new TextToSpeechVoice("Giorgio:standard", "Giorgio"),
            new TextToSpeechVoice("Gwyneth:standard", "Gwyneth"),
            new TextToSpeechVoice("Hala:neural", "Hala"),
            new TextToSpeechVoice("Hannah:neural", "Hannah"),
            new TextToSpeechVoice("Hans:standard", "Hans"),
            new TextToSpeechVoice("Hiujin:neural", "Hiujin"),
            new TextToSpeechVoice("Ida:neural", "Ida"),
            new TextToSpeechVoice("Ines:neural", "Inês - neural"),
            new TextToSpeechVoice("Ines:standard", "Inês - standard"),
            new TextToSpeechVoice("Ivy:neural", "Ivy - neural"),
            new TextToSpeechVoice("Ivy:standard", "Ivy - standard"),
            new TextToSpeechVoice("Jacek:standard", "Jacek"),
            new TextToSpeechVoice("Jan:standard", "Jan"),
            new TextToSpeechVoice("Joanna:neural", "Joanna - neural"),
            new TextToSpeechVoice("Joanna:standard", "Joanna - standard"),
            new TextToSpeechVoice("Joey:neural", "Joey - neural"),
            new TextToSpeechVoice("Joey:standard", "Joey - standard"),
            new TextToSpeechVoice("Justin:neural", "Justin - neural"),
            new TextToSpeechVoice("Justin:standard", "Justin - standard"),
            new TextToSpeechVoice("Kajal:neural", "Kajal"),
            new TextToSpeechVoice("Karl:standard", "Karl"),
            new TextToSpeechVoice("Kazuha:neural", "Kazuha"),
            new TextToSpeechVoice("Kendra:neural", "Kendra - neural"),
            new TextToSpeechVoice("Kendra:standard", "Kendra - standard"),
            new TextToSpeechVoice("Kevin:neural", "Kevin"),
            new TextToSpeechVoice("Kimberly:neural", "Kimberly - neural"),
            new TextToSpeechVoice("Kimberly:standard", "Kimberly - standard"),
            new TextToSpeechVoice("Laura:neural", "Laura"),
            new TextToSpeechVoice("Lea:neural", "Léa - neural"),
            new TextToSpeechVoice("Lea:standard", "Léa - standard"),
            new TextToSpeechVoice("Liam:neural", "Liam"),
            new TextToSpeechVoice("Lisa:neural", "Lisa"),
            new TextToSpeechVoice("Liv:standard", "Liv"),
            new TextToSpeechVoice("Lotte:standard", "Lotte"),
            new TextToSpeechVoice("Lucia:neural", "Lucia - neural"),
            new TextToSpeechVoice("Lucia:standard", "Lucia - standard"),
            new TextToSpeechVoice("Lupe:neural", "Lupe - neural"),
            new TextToSpeechVoice("Lupe:standard", "Lupe - standard"),
            new TextToSpeechVoice("Mads:standard", "Mads"),
            new TextToSpeechVoice("Maja:standard", "Maja"),
            new TextToSpeechVoice("Marlene:standard", "Marlene"),
            new TextToSpeechVoice("Mathieu:standard", "Mathieu"),
            new TextToSpeechVoice("Matthew:neural", "Matthew - neural"),
            new TextToSpeechVoice("Matthew:standard", "Matthew - standard"),
            new TextToSpeechVoice("Maxim:standard", "Maxim"),
            new TextToSpeechVoice("Mia:neural", "Mia - neural"),
            new TextToSpeechVoice("Mia:standard", "Mia - standard"),
            new TextToSpeechVoice("Miguel:standard", "Miguel"),
            new TextToSpeechVoice("Mizuki:standard", "Mizuki"),
            new TextToSpeechVoice("Naja:standard", "Naja"),
            new TextToSpeechVoice("Niamh:neural", "Niamh"),
            new TextToSpeechVoice("Nicole:standard", "Nicole"),
            new TextToSpeechVoice("Ola:neural", "Ola"),
            new TextToSpeechVoice("Olivia:neural", "Olivia"),
            new TextToSpeechVoice("Pedro:neural", "Pedro"),
            new TextToSpeechVoice("Penelope:standard", "Penélope"),
            new TextToSpeechVoice("Raveena:standard", "Raveena"),
            new TextToSpeechVoice("Remi:neural", "Rémi"),
            new TextToSpeechVoice("Ricardo:standard", "Ricardo"),
            new TextToSpeechVoice("Ruben:standard", "Ruben"),
            new TextToSpeechVoice("Russell:standard", "Russell"),
            new TextToSpeechVoice("Ruth:neural", "Ruth"),
            new TextToSpeechVoice("Salli:neural", "Salli - neural"),
            new TextToSpeechVoice("Salli:standard", "Salli - standard"),
            new TextToSpeechVoice("Seoyeon:neural", "Seoyeon - neural"),
            new TextToSpeechVoice("Seoyeon:standard", "Seoyeon - standard"),
            new TextToSpeechVoice("Sergio:neural", "Sergio"),
            new TextToSpeechVoice("Sofie:neural", "Sofie"),
            new TextToSpeechVoice("Stephen:neural", "Stephen"),
            new TextToSpeechVoice("Suvi:neural", "Suvi"),
            new TextToSpeechVoice("Takumi:neural", "Takumi - neural"),
            new TextToSpeechVoice("Takumi:standard", "Takumi - standard"),
            new TextToSpeechVoice("Tatyana:standard", "Tatyana"),
            new TextToSpeechVoice("Thiago:neural", "Thiago"),
            new TextToSpeechVoice("Tomoko:neural", "Tomoko"),
            new TextToSpeechVoice("Vicki:neural", "Vicki - neural"),
            new TextToSpeechVoice("Vicki:standard", "Vicki - standard"),
            new TextToSpeechVoice("Vitoria:neural", "Vitória - neural"),
            new TextToSpeechVoice("Vitoria:standard", "Vitória - standard"),
            new TextToSpeechVoice("Zeina:standard", "Zeina"),
            new TextToSpeechVoice("Zhiyu:neural", "Zhiyu - neural"),
            new TextToSpeechVoice("Zhiyu:standard", "Zhiyu - standard"),
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

        public async Task Speak(Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool waitForFinish)
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
                        // Play audio stream
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
