using Microsoft.CognitiveServices.Speech;
using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    internal class WindowsMicrosoftAzureSpeechService : ITextToSpeechConnectableService
    {
        public static readonly IEnumerable<TextToSpeechVoice> AvailableVoices = new List<TextToSpeechVoice>()
        {
            new TextToSpeechVoice("en-IN-AaravNeural", "Aarav - English (India)"),
            new TextToSpeechVoice("hi-IN-AaravNeural", "Aarav - Hindi (India)"),
            new TextToSpeechVoice("mr-IN-AarohiNeural", "Aarohi - Marathi (India)"),
            new TextToSpeechVoice("en-IN-AashiNeural", "Aashi - English (India)"),
            new TextToSpeechVoice("en-GB-AbbiNeural", "Abbi - English (United Kingdom)"),
            new TextToSpeechVoice("ar-OM-AbdullahNeural", "Abdullah - Arabic (Oman)"),
            new TextToSpeechVoice("en-NG-AbeoNeural", "Abeo - English (Nigeria)"),
            new TextToSpeechVoice("es-ES-AbrilNeural", "Abril - Spanish (Spain)"),
            new TextToSpeechVoice("th-TH-AcharaNeural", "Achara - Thai (Thailand)"),
            new TextToSpeechVoice("en-GB-AdaMultilingualNeural", "Ada Multilingual - English (United Kingdom)"),
            new TextToSpeechVoice("af-ZA-AdriNeural", "Adri - Afrikaans (South Africa)"),
            new TextToSpeechVoice("pl-PL-AgnieszkaNeural", "Agnieszka - Polish (Poland)"),
            new TextToSpeechVoice("tr-TR-AhmetNeural", "Ahmet - Turkish (Turkey)"),
            new TextToSpeechVoice("en-US-AIGenerate1Neural", "AIGenerate1 - English (United States)"),
            new TextToSpeechVoice("en-US-AIGenerate2Neural", "AIGenerate2 - English (United States)"),
            new TextToSpeechVoice("kk-KZ-AigulNeural", "Aigul - Kazakh (Kazakhstan)"),
            new TextToSpeechVoice("eu-ES-AinhoaNeural", "Ainhoa - Basque"),
            new TextToSpeechVoice("fr-FR-AlainNeural", "Alain - French (France)"),
            new TextToSpeechVoice("ca-ES-AlbaNeural", "Alba - Catalan (Spain)"),
            new TextToSpeechVoice("cy-GB-AledNeural", "Aled - Welsh (United Kingdom)"),
            new TextToSpeechVoice("mk-MK-AleksandarNeural", "Aleksandar - Macedonian (North Macedonia)"),
            new TextToSpeechVoice("it-IT-AlessioMultilingualNeural", "Alessio Multilingual - Italian (Italy)"),
            new TextToSpeechVoice("es-PE-AlexNeural", "Alex - Spanish (Peru)"),
            new TextToSpeechVoice("en-GB-AlfieNeural", "Alfie - English (United Kingdom)"),
            new TextToSpeechVoice("ar-BH-AliNeural", "Ali - Arabic (Bahrain)"),
            new TextToSpeechVoice("ro-RO-AlinaNeural", "Alina - Romanian (Romania)"),
            new TextToSpeechVoice("es-US-AlonsoNeural", "Alonso - Spanish (United States)"),
            new TextToSpeechVoice("es-ES-AlvaroNeural", "Alvaro - Spanish (Spain)"),
            new TextToSpeechVoice("ar-QA-AmalNeural", "Amal - Arabic (Qatar)"),
            new TextToSpeechVoice("de-DE-AmalaNeural", "Amala - German (Germany)"),
            new TextToSpeechVoice("ar-SY-AmanyNeural", "Amany - Arabic (Syria)"),
            new TextToSpeechVoice("en-US-AmberNeural", "Amber - English (United States)"),
            new TextToSpeechVoice("am-ET-AmehaNeural", "Ameha - Amharic (Ethiopia)"),
            new TextToSpeechVoice("ar-DZ-AminaNeural", "Amina - Arabic (Algeria)"),
            new TextToSpeechVoice("en-US-AnaNeural", "Ana - English (United States)"),
            new TextToSpeechVoice("hy-AM-AnahitNeural", "Anahit - Armenian (Armenia)"),
            new TextToSpeechVoice("en-IN-AnanyaNeural", "Ananya - English (India)"),
            new TextToSpeechVoice("hi-IN-AnanyaNeural", "Ananya - Hindi (India)"),
            new TextToSpeechVoice("ta-SG-AnbuNeural", "Anbu - Tamil (Singapore)"),
            new TextToSpeechVoice("eu-ES-AnderNeural", "Ander - Basque"),
            new TextToSpeechVoice("es-EC-AndreaNeural", "Andrea - Spanish (Ecuador)"),
            new TextToSpeechVoice("es-GT-AndresNeural", "Andres - Spanish (Guatemala)"),
            new TextToSpeechVoice("en-US-AndrewNeural", "Andrew - English (United States)"),
            new TextToSpeechVoice("en-US-AndrewMultilingualNeural", "Andrew Multilingual - English (United States)"),
            new TextToSpeechVoice("fil-PH-AngeloNeural", "Angelo - Filipino (Philippines)"),
            new TextToSpeechVoice("sq-AL-AnilaNeural", "Anila - Albanian (Albania)"),
            new TextToSpeechVoice("en-AU-AnnetteNeural", "Annette - English (Australia)"),
            new TextToSpeechVoice("fr-CA-AntoineNeural", "Antoine - French (Canada)"),
            new TextToSpeechVoice("cs-CZ-AntoninNeural", "Antonin - Czech (Czechia)"),
            new TextToSpeechVoice("pt-BR-AntonioNeural", "Antonio - Portuguese (Brazil)"),
            new TextToSpeechVoice("et-EE-AnuNeural", "Anu - Estonian (Estonia)"),
            new TextToSpeechVoice("ja-JP-AoiNeural", "Aoi - Japanese (Japan)"),
            new TextToSpeechVoice("es-ES-ArabellaMultilingualNeural", "Arabella Multilingual - Spanish (Spain)"),
            new TextToSpeechVoice("id-ID-ArdiNeural", "Ardi - Indonesian (Indonesia)"),
            new TextToSpeechVoice("en-US-AriaNeural", "Aria - English (United States)"),
            new TextToSpeechVoice("fr-CH-ArianeNeural", "Ariane - French (Switzerland)"),
            new TextToSpeechVoice("es-ES-ArnauNeural", "Arnau - Spanish (Spain)"),
            new TextToSpeechVoice("nl-BE-ArnaudNeural", "Arnaud - Dutch (Belgium)"),
            new TextToSpeechVoice("ur-PK-AsadNeural", "Asad - Urdu (Pakistan)"),
            new TextToSpeechVoice("en-US-AshleyNeural", "Ashley - English (United States)"),
            new TextToSpeechVoice("en-KE-AsiliaNeural", "Asilia - English (Kenya)"),
            new TextToSpeechVoice("el-GR-AthinaNeural", "Athina - Greek (Greece)"),
            new TextToSpeechVoice("en-US-AvaNeural", "Ava - English (United States)"),
            new TextToSpeechVoice("en-US-AvaMultilingualNeural", "Ava Multilingual - English (United States)"),
            new TextToSpeechVoice("he-IL-AvriNeural", "Avri - Hebrew (Israel)"),
            new TextToSpeechVoice("ar-OM-AyshaNeural", "Aysha - Arabic (Oman)"),
            new TextToSpeechVoice("az-AZ-BabekNeural", "Babek - Azerbaijani (Latin, Azerbaijan)"),
            new TextToSpeechVoice("az-AZ-BanuNeural", "Banu - Azerbaijani (Latin, Azerbaijan)"),
            new TextToSpeechVoice("bn-IN-BashkarNeural", "Bashkar - Bengali (India)"),
            new TextToSpeechVoice("ar-IQ-BasselNeural", "Bassel - Arabic (Iraq)"),
            new TextToSpeechVoice("mn-MN-BataaNeural", "Bataa - Mongolian (Mongolia)"),
            new TextToSpeechVoice("es-MX-BeatrizNeural", "Beatriz - Spanish (Mexico)"),
            new TextToSpeechVoice("es-CU-BelkysNeural", "Belkys - Spanish (Cuba)"),
            new TextToSpeechVoice("en-GB-BellaNeural", "Bella - English (United Kingdom)"),
            new TextToSpeechVoice("it-IT-BenignoNeural", "Benigno - Italian (Italy)"),
            new TextToSpeechVoice("de-DE-BerndNeural", "Bernd - German (Germany)"),
            new TextToSpeechVoice("fil-PH-BlessicaNeural", "Blessica - Filipino (Philippines)"),
            new TextToSpeechVoice("en-US-BlueNeural", "Blue - English (United States)"),
            new TextToSpeechVoice("ko-KR-BongJinNeural", "BongJin - Korean (Korea)"),
            new TextToSpeechVoice("bg-BG-BorislavNeural", "Borislav - Bulgarian (Bulgaria)"),
            new TextToSpeechVoice("en-US-BrandonNeural", "Brandon - English (United States)"),
            new TextToSpeechVoice("pt-BR-BrendaNeural", "Brenda - Portuguese (Brazil)"),
            new TextToSpeechVoice("en-US-BrianNeural", "Brian - English (United States)"),
            new TextToSpeechVoice("en-US-BrianMultilingualNeural", "Brian Multilingual - English (United States)"),
            new TextToSpeechVoice("fr-FR-BrigitteNeural", "Brigitte - French (France)"),
            new TextToSpeechVoice("it-IT-CalimeroNeural", "Calimero - Italian (Italy)"),
            new TextToSpeechVoice("es-PE-CamilaNeural", "Camila - Spanish (Peru)"),
            new TextToSpeechVoice("es-MX-CandelaNeural", "Candela - Spanish (Mexico)"),
            new TextToSpeechVoice("es-HN-CarlosNeural", "Carlos - Spanish (Honduras)"),
            new TextToSpeechVoice("es-MX-CarlotaNeural", "Carlota - Spanish (Mexico)"),
            new TextToSpeechVoice("en-AU-CarlyNeural", "Carly - English (Australia)"),
            new TextToSpeechVoice("it-IT-CataldoNeural", "Cataldo - Italian (Italy)"),
            new TextToSpeechVoice("es-CL-CatalinaNeural", "Catalina - Spanish (Chile)"),
            new TextToSpeechVoice("es-MX-CecilioNeural", "Cecilio - Spanish (Mexico)"),
            new TextToSpeechVoice("fr-FR-CelesteNeural", "Celeste - French (France)"),
            new TextToSpeechVoice("lo-LA-ChanthavongNeural", "Chanthavong - Lao (Laos)"),
            new TextToSpeechVoice("fr-BE-CharlineNeural", "Charline - French (Belgium)"),
            new TextToSpeechVoice("en-KE-ChilembaNeural", "Chilemba - English (Kenya)"),
            new TextToSpeechVoice("da-DK-ChristelNeural", "Christel - Danish (Denmark)"),
            new TextToSpeechVoice("de-DE-ChristophNeural", "Christoph - German (Germany)"),
            new TextToSpeechVoice("en-US-ChristopherNeural", "Christopher - English (United States)"),
            new TextToSpeechVoice("en-CA-ClaraNeural", "Clara - English (Canada)"),
            new TextToSpeechVoice("fr-FR-ClaudeNeural", "Claude - French (France)"),
            new TextToSpeechVoice("nl-NL-ColetteNeural", "Colette - Dutch (Netherlands)"),
            new TextToSpeechVoice("ga-IE-ColmNeural", "Colm - Irish (Ireland)"),
            new TextToSpeechVoice("en-IE-ConnorNeural", "Connor - English (Ireland)"),
            new TextToSpeechVoice("de-DE-ConradNeural", "Conrad - German (Germany)"),
            new TextToSpeechVoice("en-US-CoraNeural", "Cora - English (United States)"),
            new TextToSpeechVoice("fr-FR-CoralieNeural", "Coralie - French (France)"),
            new TextToSpeechVoice("ja-JP-DaichiNeural", "Daichi - Japanese (Japan)"),
            new TextToSpeechVoice("es-MX-DaliaNeural", "Dalia - Spanish (Mexico)"),
            new TextToSpeechVoice("es-ES-DarioNeural", "Dario - Spanish (Spain)"),
            new TextToSpeechVoice("ru-RU-DariyaNeural", "Dariya - Russian (Russia)"),
            new TextToSpeechVoice("en-AU-DarrenNeural", "Darren - English (Australia)"),
            new TextToSpeechVoice("sw-TZ-DaudiNeural", "Daudi - Swahili (Tanzania)"),
            new TextToSpeechVoice("kk-KZ-DauletNeural", "Daulet - Kazakh (Kazakhstan)"),
            new TextToSpeechVoice("en-US-DavisNeural", "Davis - English (United States)"),
            new TextToSpeechVoice("nl-BE-DenaNeural", "Dena - Dutch (Belgium)"),
            new TextToSpeechVoice("fr-FR-DeniseNeural", "Denise - French (France)"),
            new TextToSpeechVoice("gu-IN-DhwaniNeural", "Dhwani - Gujarati (India)"),
            new TextToSpeechVoice("it-IT-DiegoNeural", "Diego - Italian (Italy)"),
            new TextToSpeechVoice("fa-IR-DilaraNeural", "Dilara - Persian (Iran)"),
            new TextToSpeechVoice("jv-ID-DimasNeural", "Dimas - Javanese (Latin, Indonesia)"),
            new TextToSpeechVoice("ru-RU-DmitryNeural", "Dmitry - Russian (Russia)"),
            new TextToSpeechVoice("pt-BR-DonatoNeural", "Donato - Portuguese (Brazil)"),
            new TextToSpeechVoice("pt-PT-DuarteNeural", "Duarte - Portuguese (Portugal)"),
            new TextToSpeechVoice("en-AU-DuncanNeural", "Duncan - English (Australia)"),
            new TextToSpeechVoice("ka-GE-EkaNeural", "Eka - Georgian (Georgia)"),
            new TextToSpeechVoice("es-AR-ElenaNeural", "Elena - Spanish (Argentina)"),
            new TextToSpeechVoice("es-ES-EliasNeural", "Elias - Spanish (Spain)"),
            new TextToSpeechVoice("en-TZ-ElimuNeural", "Elimu - English (Tanzania)"),
            new TextToSpeechVoice("en-US-ElizabethNeural", "Elizabeth - English (United States)"),
            new TextToSpeechVoice("de-DE-ElkeNeural", "Elke - German (Germany)"),
            new TextToSpeechVoice("en-GB-ElliotNeural", "Elliot - English (United Kingdom)"),
            new TextToSpeechVoice("fr-FR-EloiseNeural", "Eloise - French (France)"),
            new TextToSpeechVoice("it-IT-ElsaNeural", "Elsa - Italian (Italy)"),
            new TextToSpeechVoice("en-AU-ElsieNeural", "Elsie - English (Australia)"),
            new TextToSpeechVoice("es-ES-ElviraNeural", "Elvira - Spanish (Spain)"),
            new TextToSpeechVoice("pt-BR-ElzaNeural", "Elza - Portuguese (Brazil)"),
            new TextToSpeechVoice("tr-TR-EmelNeural", "Emel - Turkish (Turkey)"),
            new TextToSpeechVoice("ro-RO-EmilNeural", "Emil - Romanian (Romania)"),
            new TextToSpeechVoice("es-DO-EmilioNeural", "Emilio - Spanish (Dominican Republic)"),
            new TextToSpeechVoice("en-IE-EmilyNeural", "Emily - English (Ireland)"),
            new TextToSpeechVoice("en-US-EmmaNeural", "Emma - English (United States)"),
            new TextToSpeechVoice("en-US-EmmaMultilingualNeural", "Emma Multilingual - English (United States)"),
            new TextToSpeechVoice("ca-ES-EnricNeural", "Enric - Catalan (Spain)"),
            new TextToSpeechVoice("en-US-EricNeural", "Eric - English (United States)"),
            new TextToSpeechVoice("es-ES-EstrellaNeural", "Estrella - Spanish (Spain)"),
            new TextToSpeechVoice("en-GB-EthanNeural", "Ethan - English (United Kingdom)"),
            new TextToSpeechVoice("lv-LV-EveritaNeural", "Everita - Latvian (Latvia)"),
            new TextToSpeechVoice("en-NG-EzinneNeural", "Ezinne - English (Nigeria)"),
            new TextToSpeechVoice("pt-BR-FabioNeural", "Fabio - Portuguese (Brazil)"),
            new TextToSpeechVoice("it-IT-FabiolaNeural", "Fabiola - Italian (Italy)"),
            new TextToSpeechVoice("fr-CH-FabriceNeural", "Fabrice - French (Switzerland)"),
            new TextToSpeechVoice("ar-KW-FahedNeural", "Fahed - Arabic (Kuwait)"),
            new TextToSpeechVoice("fa-IR-FaridNeural", "Farid - Persian (Iran)"),
            new TextToSpeechVoice("ar-AE-FatimaNeural", "Fatima - Arabic (United Arab Emirates)"),
            new TextToSpeechVoice("es-NI-FedericoNeural", "Federico - Spanish (Nicaragua)"),
            new TextToSpeechVoice("nl-NL-FennaNeural", "Fenna - Dutch (Netherlands)"),
            new TextToSpeechVoice("pt-PT-FernandaNeural", "Fernanda - Portuguese (Portugal)"),
            new TextToSpeechVoice("it-IT-FiammaNeural", "Fiamma - Italian (Italy)"),
            new TextToSpeechVoice("nb-NO-FinnNeural", "Finn - Norwegian Bokm?Ñl (Norway)"),
            new TextToSpeechVoice("de-DE-FlorianMultilingualNeural", "Florian Multilingual - German (Germany)"),
            new TextToSpeechVoice("pt-BR-FranciscaNeural", "Francisca - Portuguese (Brazil)"),
            new TextToSpeechVoice("en-AU-FreyaNeural", "Freya - English (Australia)"),
            new TextToSpeechVoice("hr-HR-GabrijelaNeural", "Gabrijela - Croatian (Croatia)"),
            new TextToSpeechVoice("id-ID-GadisNeural", "Gadis - Indonesian (Indonesia)"),
            new TextToSpeechVoice("kn-IN-GaganNeural", "Gagan - Kannada (India)"),
            new TextToSpeechVoice("fr-BE-GerardNeural", "Gerard - French (Belgium)"),
            new TextToSpeechVoice("es-MX-GerardoNeural", "Gerardo - Spanish (Mexico)"),
            new TextToSpeechVoice("it-IT-GianniNeural", "Gianni - Italian (Italy)"),
            new TextToSpeechVoice("ka-GE-GiorgiNeural", "Giorgi - Georgian (Georgia)"),
            new TextToSpeechVoice("pt-BR-GiovannaNeural", "Giovanna - Portuguese (Brazil)"),
            new TextToSpeechVoice("de-DE-GiselaNeural", "Gisela - German (Germany)"),
            new TextToSpeechVoice("it-IT-GiuseppeNeural", "Giuseppe - Italian (Italy)"),
            new TextToSpeechVoice("es-CO-GonzaloNeural", "Gonzalo - Spanish (Colombia)"),
            new TextToSpeechVoice("ko-KR-GookMinNeural", "GookMin - Korean (Korea)"),
            new TextToSpeechVoice("bs-BA-GoranNeural", "Goran - Bosnian (Bosnia and Herzegovina)"),
            new TextToSpeechVoice("mt-MT-GraceNeural", "Grace - Maltese (Malta)"),
            new TextToSpeechVoice("is-IS-GudrunNeural", "Gudrun - Icelandic (Iceland)"),
            new TextToSpeechVoice("ur-IN-GulNeural", "Gul - Urdu (India)"),
            new TextToSpeechVoice("ps-AF-GulNawazNeural", "Gul Nawaz - Pashto (Afghanistan)"),
            new TextToSpeechVoice("is-IS-GunnarNeural", "Gunnar - Icelandic (Iceland)"),
            new TextToSpeechVoice("en-US-GuyNeural", "Guy - English (United States)"),
            new TextToSpeechVoice("ar-AE-HamdanNeural", "Hamdan - Arabic (United Arab Emirates)"),
            new TextToSpeechVoice("ar-SA-HamedNeural", "Hamed - Arabic (Saudi Arabia)"),
            new TextToSpeechVoice("fi-FI-HarriNeural", "Harri - Finnish (Finland)"),
            new TextToSpeechVoice("hy-AM-HaykNeural", "Hayk - Armenian (Armenia)"),
            new TextToSpeechVoice("ar-TN-HediNeural", "Hedi - Arabic (Tunisia)"),
            new TextToSpeechVoice("ne-NP-HemkalaNeural", "Hemkala - Nepali (Nepal)"),
            new TextToSpeechVoice("fr-FR-HenriNeural", "Henri - French (France)"),
            new TextToSpeechVoice("he-IL-HilaNeural", "Hila - Hebrew (Israel)"),
            new TextToSpeechVoice("sv-SE-HilleviNeural", "Hillevi - Swedish (Sweden)"),
            new TextToSpeechVoice("zh-HK-HiuGaaiNeural", "HiuGaai - Chinese (Cantonese, Traditional)"),
            new TextToSpeechVoice("zh-HK-HiuMaanNeural", "HiuMaan - Chinese (Cantonese, Traditional)"),
            new TextToSpeechVoice("vi-VN-HoaiMyNeural", "HoaiMy - Vietnamese (Vietnam)"),
            new TextToSpeechVoice("en-GB-HollieNeural", "Hollie - English (United Kingdom)"),
            new TextToSpeechVoice("zh-TW-HsiaoChenNeural", "HsiaoChen - Chinese (Taiwanese Mandarin, Traditional)"),
            new TextToSpeechVoice("zh-TW-HsiaoYuNeural", "HsiaoYu - Chinese (Taiwanese Mandarin, Traditional)"),
            new TextToSpeechVoice("pt-BR-HumbertoNeural", "Humberto - Portuguese (Brazil)"),
            new TextToSpeechVoice("ko-KR-HyunsuNeural", "Hyunsu - Korean (Korea)"),
            new TextToSpeechVoice("sq-AL-IlirNeural", "Ilir - Albanian (Albania)"),
            new TextToSpeechVoice("ar-LY-ImanNeural", "Iman - Arabic (Libya)"),
            new TextToSpeechVoice("en-TZ-ImaniNeural", "Imani - English (Tanzania)"),
            new TextToSpeechVoice("it-IT-ImeldaNeural", "Imelda - Italian (Italy)"),
            new TextToSpeechVoice("de-AT-IngridNeural", "Ingrid - German (Austria)"),
            new TextToSpeechVoice("ko-KR-InJoonNeural", "InJoon - Korean (Korea)"),
            new TextToSpeechVoice("es-ES-IreneNeural", "Irene - Spanish (Spain)"),
            new TextToSpeechVoice("it-IT-IrmaNeural", "Irma - Italian (Italy)"),
            new TextToSpeechVoice("it-IT-IsabellaNeural", "Isabella - Italian (Italy)"),
            new TextToSpeechVoice("it-IT-IsabellaMultilingualNeural", "Isabella Multilingual - Italian (Italy)"),
            new TextToSpeechVoice("nb-NO-IselinNeural", "Iselin - Norwegian Bokm?Ñl (Norway)"),
            new TextToSpeechVoice("es-ES-IsidoraMultilingualNeural", "Isidora Multilingual - Spanish (Spain)"),
            new TextToSpeechVoice("ar-DZ-IsmaelNeural", "Ismael - Arabic (Algeria)"),
            new TextToSpeechVoice("en-US-JacobNeural", "Jacob - English (United States)"),
            new TextToSpeechVoice("fr-FR-JacquelineNeural", "Jacqueline - French (France)"),
            new TextToSpeechVoice("su-ID-JajangNeural", "Jajang - Sundanese (Indonesia)"),
            new TextToSpeechVoice("ar-MA-JamalNeural", "Jamal - Arabic (Morocco)"),
            new TextToSpeechVoice("en-PH-JamesNeural", "James - English (Philippines)"),
            new TextToSpeechVoice("de-CH-JanNeural", "Jan - German (Switzerland)"),
            new TextToSpeechVoice("en-US-JaneNeural", "Jane - English (United States)"),
            new TextToSpeechVoice("en-US-JasonNeural", "Jason - English (United States)"),
            new TextToSpeechVoice("es-GQ-JavierNeural", "Javier - Spanish (Equatorial Guinea)"),
            new TextToSpeechVoice("fr-CA-JeanNeural", "Jean - French (Canada)"),
            new TextToSpeechVoice("en-US-JennyNeural", "Jenny - English (United States)"),
            new TextToSpeechVoice("en-US-JennyMultilingualNeural", "Jenny Multilingual - English (United States)"),
            new TextToSpeechVoice("da-DK-JeppeNeural", "Jeppe - Danish (Denmark)"),
            new TextToSpeechVoice("fr-FR-JeromeNeural", "Jerome - French (France)"),
            new TextToSpeechVoice("ko-KR-JiMinNeural", "JiMin - Korean (Korea)"),
            new TextToSpeechVoice("ca-ES-JoanaNeural", "Joana - Catalan (Spain)"),
            new TextToSpeechVoice("en-AU-JoanneNeural", "Joanne - English (Australia)"),
            new TextToSpeechVoice("de-AT-JonasNeural", "Jonas - German (Austria)"),
            new TextToSpeechVoice("es-MX-JorgeNeural", "Jorge - Spanish (Mexico)"),
            new TextToSpeechVoice("mt-MT-JosephNeural", "Joseph - Maltese (Malta)"),
            new TextToSpeechVoice("fr-FR-JosephineNeural", "Josephine - French (France)"),
            new TextToSpeechVoice("es-CR-JuanNeural", "Juan - Spanish (Costa Rica)"),
            new TextToSpeechVoice("pt-BR-JulioNeural", "Julio - Portuguese (Brazil)"),
            new TextToSpeechVoice("en-US-KaiNeural", "Kai - English (United States)"),
            new TextToSpeechVoice("bg-BG-KalinaNeural", "Kalina - Bulgarian (Bulgaria)"),
            new TextToSpeechVoice("ta-MY-KaniNeural", "Kani - Tamil (Malaysia)"),
            new TextToSpeechVoice("es-PR-KarinaNeural", "Karina - Spanish (Puerto Rico)"),
            new TextToSpeechVoice("es-HN-KarlaNeural", "Karla - Spanish (Honduras)"),
            new TextToSpeechVoice("de-DE-KasperNeural", "Kasper - German (Germany)"),
            new TextToSpeechVoice("de-DE-KatjaNeural", "Katja - German (Germany)"),
            new TextToSpeechVoice("en-IN-KavyaNeural", "Kavya - English (India)"),
            new TextToSpeechVoice("hi-IN-KavyaNeural", "Kavya - Hindi (India)"),
            new TextToSpeechVoice("ja-JP-KeitaNeural", "Keita - Japanese (Japan)"),
            new TextToSpeechVoice("en-AU-KenNeural", "Ken - English (Australia)"),
            new TextToSpeechVoice("lo-LA-KeomanyNeural", "Keomany - Lao (Laos)"),
            new TextToSpeechVoice("et-EE-KertNeural", "Kert - Estonian (Estonia)"),
            new TextToSpeechVoice("de-DE-KillianNeural", "Killian - German (Germany)"),
            new TextToSpeechVoice("en-AU-KimNeural", "Kim - English (Australia)"),
            new TextToSpeechVoice("de-DE-KlarissaNeural", "Klarissa - German (Germany)"),
            new TextToSpeechVoice("de-DE-KlausNeural", "Klaus - German (Germany)"),
            new TextToSpeechVoice("ta-LK-KumarNeural", "Kumar - Tamil (Sri Lanka)"),
            new TextToSpeechVoice("en-IN-KunalNeural", "Kunal - English (India)"),
            new TextToSpeechVoice("hi-IN-KunalNeural", "Kunal - Hindi (India)"),
            new TextToSpeechVoice("es-ES-LaiaNeural", "Laia - Spanish (Spain)"),
            new TextToSpeechVoice("ar-BH-LailaNeural", "Laila - Arabic (Bahrain)"),
            new TextToSpeechVoice("ar-SY-LaithNeural", "Laith - Arabic (Syria)"),
            new TextToSpeechVoice("es-MX-LarissaNeural", "Larissa - Spanish (Mexico)"),
            new TextToSpeechVoice("ps-AF-LatifaNeural", "Latifa - Pashto (Afghanistan)"),
            new TextToSpeechVoice("ar-LB-LaylaNeural", "Layla - Arabic (Lebanon)"),
            new TextToSpeechVoice("en-ZA-LeahNeural", "Leah - English (South Africa)"),
            new TextToSpeechVoice("pt-BR-LeilaNeural", "Leila - Portuguese (Brazil)"),
            new TextToSpeechVoice("de-CH-LeniNeural", "Leni - German (Switzerland)"),
            new TextToSpeechVoice("lt-LT-LeonasNeural", "Leonas - Lithuanian (Lithuania)"),
            new TextToSpeechVoice("pt-BR-LeticiaNeural", "Leticia - Portuguese (Brazil)"),
            new TextToSpeechVoice("es-ES-LiaNeural", "Lia - Spanish (Spain)"),
            new TextToSpeechVoice("en-CA-LiamNeural", "Liam - English (Canada)"),
            new TextToSpeechVoice("en-GB-LibbyNeural", "Libby - English (United Kingdom)"),
            new TextToSpeechVoice("es-MX-LibertoNeural", "Liberto - Spanish (Mexico)"),
            new TextToSpeechVoice("it-IT-LisandroNeural", "Lisandro - Italian (Italy)"),
            new TextToSpeechVoice("es-SV-LorenaNeural", "Lorena - Spanish (El Salvador)"),
            new TextToSpeechVoice("es-CL-LorenzoNeural", "Lorenzo - Spanish (Chile)"),
            new TextToSpeechVoice("de-DE-LouisaNeural", "Louisa - German (Germany)"),
            new TextToSpeechVoice("es-MX-LucianoNeural", "Luciano - Spanish (Mexico)"),
            new TextToSpeechVoice("es-EC-LuisNeural", "Luis - Spanish (Ecuador)"),
            new TextToSpeechVoice("sk-SK-LukasNeural", "Lukas - Slovak (Slovakia)"),
            new TextToSpeechVoice("en-ZA-LukeNeural", "Luke - English (South Africa)"),
            new TextToSpeechVoice("en-SG-LunaNeural", "Luna - English (Singapore)"),
            new TextToSpeechVoice("en-US-LunaNeural", "Luna - English (United States)"),
            new TextToSpeechVoice("nl-NL-MaartenNeural", "Maarten - Dutch (Netherlands)"),
            new TextToSpeechVoice("hi-IN-MadhurNeural", "Madhur - Hindi (India)"),
            new TextToSpeechVoice("uz-UZ-MadinaNeural", "Madina - Uzbek (Latin, Uzbekistan)"),
            new TextToSpeechVoice("en-GB-MaisieNeural", "Maisie - English (United Kingdom)"),
            new TextToSpeechVoice("de-DE-MajaNeural", "Maja - German (Germany)"),
            new TextToSpeechVoice("mr-IN-ManoharNeural", "Manohar - Marathi (India)"),
            new TextToSpeechVoice("es-CU-ManuelNeural", "Manuel - Spanish (Cuba)"),
            new TextToSpeechVoice("pt-BR-ManuelaNeural", "Manuela - Portuguese (Brazil)"),
            new TextToSpeechVoice("it-IT-MarcelloMultilingualNeural", "Marcello Multilingual - Italian (Italy)"),
            new TextToSpeechVoice("es-BO-MarceloNeural", "Marcelo - Spanish (Bolivia)"),
            new TextToSpeechVoice("pl-PL-MarekNeural", "Marek - Polish (Poland)"),
            new TextToSpeechVoice("es-PA-MargaritaNeural", "Margarita - Spanish (Panama)"),
            new TextToSpeechVoice("es-CR-MariaNeural", "Maria - Spanish (Costa Rica)"),
            new TextToSpeechVoice("mk-MK-MarijaNeural", "Marija - Macedonian (North Macedonia)"),
            new TextToSpeechVoice("es-MX-MarinaNeural", "Marina - Spanish (Mexico)"),
            new TextToSpeechVoice("es-PY-MarioNeural", "Mario - Spanish (Paraguay)"),
            new TextToSpeechVoice("es-GT-MartaNeural", "Marta - Spanish (Guatemala)"),
            new TextToSpeechVoice("ar-YE-MaryamNeural", "Maryam - Arabic (Yemen)"),
            new TextToSpeechVoice("ja-JP-MasaruMultilingualNeural", "Masaru Multilingual - Japanese (Japan)"),
            new TextToSpeechVoice("es-UY-MateoNeural", "Mateo - Spanish (Uruguay)"),
            new TextToSpeechVoice("sv-SE-MattiasNeural", "Mattias - Swedish (Sweden)"),
            new TextToSpeechVoice("fr-FR-MauriceNeural", "Maurice - French (France)"),
            new TextToSpeechVoice("ja-JP-MayuNeural", "Mayu - Japanese (Japan)"),
            new TextToSpeechVoice("am-ET-MekdesNeural", "Mekdes - Amharic (Ethiopia)"),
            new TextToSpeechVoice("en-GB-MiaNeural", "Mia - English (United Kingdom)"),
            new TextToSpeechVoice("en-US-MichelleNeural", "Michelle - English (United States)"),
            new TextToSpeechVoice("ml-IN-MidhunNeural", "Midhun - Malayalam (India)"),
            new TextToSpeechVoice("en-NZ-MitchellNeural", "Mitchell - English (New Zealand)"),
            new TextToSpeechVoice("ar-QA-MoazNeural", "Moaz - Arabic (Qatar)"),
            new TextToSpeechVoice("te-IN-MohanNeural", "Mohan - Telugu (India)"),
            new TextToSpeechVoice("en-NZ-MollyNeural", "Molly - English (New Zealand)"),
            new TextToSpeechVoice("en-US-MonicaNeural", "Monica - English (United States)"),
            new TextToSpeechVoice("ar-MA-MounaNeural", "Mouna - Arabic (Morocco)"),
            new TextToSpeechVoice("so-SO-MuuseNeural", "Muuse - Somali (Somalia)"),
            new TextToSpeechVoice("bn-BD-NabanitaNeural", "Nabanita - Bangla (Bangladesh)"),
            new TextToSpeechVoice("vi-VN-NamMinhNeural", "NamMinh - Vietnamese (Vietnam)"),
            new TextToSpeechVoice("ja-JP-NanamiNeural", "Nanami - Japanese (Japan)"),
            new TextToSpeechVoice("en-US-NancyNeural", "Nancy - English (United States)"),
            new TextToSpeechVoice("ja-JP-NaokiNeural", "Naoki - Japanese (Japan)"),
            new TextToSpeechVoice("en-AU-NatashaNeural", "Natasha - English (Australia)"),
            new TextToSpeechVoice("en-IN-NeerjaNeural", "Neerja - English (India)"),
            new TextToSpeechVoice("en-AU-NeilNeural", "Neil - English (Australia)"),
            new TextToSpeechVoice("el-GR-NestorasNeural", "Nestoras - Greek (Greece)"),
            new TextToSpeechVoice("cy-GB-NiaNeural", "Nia - Welsh (United Kingdom)"),
            new TextToSpeechVoice("sr-RS-NicholasNeural", "Nicholas - Serbian (Cyrillic, Serbia)"),
            new TextToSpeechVoice("sr-Latn-RS-NicholasNeural", "Nicholas - Serbian (Latin, Serbia)"),
            new TextToSpeechVoice("pt-BR-NicolauNeural", "Nicolau - Portuguese (Brazil)"),
            new TextToSpeechVoice("es-ES-NilNeural", "Nil - Spanish (Spain)"),
            new TextToSpeechVoice("my-MM-NilarNeural", "Nilar - Burmese (Myanmar)"),
            new TextToSpeechVoice("lv-LV-NilsNeural", "Nils - Latvian (Latvia)"),
            new TextToSpeechVoice("gu-IN-NiranjanNeural", "Niranjan - Gujarati (India)"),
            new TextToSpeechVoice("th-TH-NiwatNeural", "Niwat - Thai (Thailand)"),
            new TextToSpeechVoice("en-GB-NoahNeural", "Noah - English (United Kingdom)"),
            new TextToSpeechVoice("hu-HU-NoemiNeural", "Noemi - Hungarian (Hungary)"),
            new TextToSpeechVoice("fi-FI-NooraNeural", "Noora - Finnish (Finland)"),
            new TextToSpeechVoice("ar-KW-NouraNeural", "Noura - Arabic (Kuwait)"),
            new TextToSpeechVoice("es-MX-NuriaNeural", "Nuria - Spanish (Mexico)"),
            new TextToSpeechVoice("pa-IN-OjasNeural", "Ojas - Punjabi (India)"),
            new TextToSpeechVoice("en-GB-OliverNeural", "Oliver - English (United Kingdom)"),
            new TextToSpeechVoice("en-GB-OliviaNeural", "Olivia - English (United Kingdom)"),
            new TextToSpeechVoice("en-GB-OllieMultilingualNeural", "Ollie Multilingual - English (United Kingdom)"),
            new TextToSpeechVoice("ar-LY-OmarNeural", "Omar - Arabic (Libya)"),
            new TextToSpeechVoice("lt-LT-OnaNeural", "Ona - Lithuanian (Lithuania)"),
            new TextToSpeechVoice("ga-IE-OrlaNeural", "Orla - Irish (Ireland)"),
            new TextToSpeechVoice("ms-MY-OsmanNeural", "Osman - Malay (Malaysia)"),
            new TextToSpeechVoice("uk-UA-OstapNeural", "Ostap - Ukrainian (Ukraine)"),
            new TextToSpeechVoice("ta-IN-PallaviNeural", "Pallavi - Tamil (India)"),
            new TextToSpeechVoice("it-IT-PalmiraNeural", "Palmira - Italian (Italy)"),
            new TextToSpeechVoice("es-US-PalomaNeural", "Paloma - Spanish (United States)"),
            new TextToSpeechVoice("es-VE-PaolaNeural", "Paola - Spanish (Venezuela)"),
            new TextToSpeechVoice("es-MX-PelayoNeural", "Pelayo - Spanish (Mexico)"),
            new TextToSpeechVoice("nb-NO-PernilleNeural", "Pernille - Norwegian Bokm?Ñl (Norway)"),
            new TextToSpeechVoice("sl-SI-PetraNeural", "Petra - Slovenian (Slovenia)"),
            new TextToSpeechVoice("it-IT-PierinaNeural", "Pierina - Italian (Italy)"),
            new TextToSpeechVoice("km-KH-PisethNeural", "Piseth - Khmer (Cambodia)"),
            new TextToSpeechVoice("uk-UA-PolinaNeural", "Polina - Ukrainian (Ukraine)"),
            new TextToSpeechVoice("en-IN-PrabhatNeural", "Prabhat - English (India)"),
            new TextToSpeechVoice("bn-BD-PradeepNeural", "Pradeep - Bangla (Bangladesh)"),
            new TextToSpeechVoice("th-TH-PremwadeeNeural", "Premwadee - Thai (Thailand)"),
            new TextToSpeechVoice("as-IN-PriyomNeural", "Priyom - Assamese (India)"),
            new TextToSpeechVoice("sw-KE-RafikiNeural", "Rafiki - Swahili (Kenya)"),
            new TextToSpeechVoice("de-DE-RalfNeural", "Ralf - German (Germany)"),
            new TextToSpeechVoice("ar-LB-RamiNeural", "Rami - Arabic (Lebanon)"),
            new TextToSpeechVoice("es-DO-RamonaNeural", "Ramona - Spanish (Dominican Republic)"),
            new TextToSpeechVoice("ar-IQ-RanaNeural", "Rana - Arabic (Iraq)"),
            new TextToSpeechVoice("pt-PT-RaquelNeural", "Raquel - Portuguese (Portugal)"),
            new TextToSpeechVoice("ar-TN-ReemNeural", "Reem - Arabic (Tunisia)"),
            new TextToSpeechVoice("en-IN-RehaanNeural", "Rehaan - English (India)"),
            new TextToSpeechVoice("hi-IN-RehaanNeural", "Rehaan - Hindi (India)"),
            new TextToSpeechVoice("sw-TZ-RehemaNeural", "Rehema - Swahili (Tanzania)"),
            new TextToSpeechVoice("fr-FR-RemyMultilingualNeural", "Remy Multilingual - French (France)"),
            new TextToSpeechVoice("es-MX-RenataNeural", "Renata - Spanish (Mexico)"),
            new TextToSpeechVoice("it-IT-RinaldoNeural", "Rinaldo - Italian (Italy)"),
            new TextToSpeechVoice("es-PA-RobertoNeural", "Roberto - Spanish (Panama)"),
            new TextToSpeechVoice("es-SV-RodrigoNeural", "Rodrigo - Spanish (El Salvador)"),
            new TextToSpeechVoice("en-US-RogerNeural", "Roger - English (United States)"),
            new TextToSpeechVoice("gl-ES-RoiNeural", "Roi - Galician"),
            new TextToSpeechVoice("sl-SI-RokNeural", "Rok - Slovenian (Slovenia)"),
            new TextToSpeechVoice("en-PH-RosaNeural", "Rosa - English (Philippines)"),
            new TextToSpeechVoice("en-GB-RyanNeural", "Ryan - English (United Kingdom)"),
            new TextToSpeechVoice("en-US-RyanMultilingualNeural", "Ryan Multilingual - English (United States)"),
            new TextToSpeechVoice("gl-ES-SabelaNeural", "Sabela - Galician"),
            new TextToSpeechVoice("ne-NP-SagarNeural", "Sagar - Nepali (Nepal)"),
            new TextToSpeechVoice("ar-YE-SalehNeural", "Saleh - Arabic (Yemen)"),
            new TextToSpeechVoice("ar-EG-SalmaNeural", "Salma - Arabic (Egypt)"),
            new TextToSpeechVoice("ur-IN-SalmanNeural", "Salman - Urdu (India)"),
            new TextToSpeechVoice("es-CO-SalomeNeural", "Salome - Spanish (Colombia)"),
            new TextToSpeechVoice("en-HK-SamNeural", "Sam - English (Hong Kong SAR)"),
            new TextToSpeechVoice("si-LK-SameeraNeural", "Sameera - Sinhala (Sri Lanka)"),
            new TextToSpeechVoice("ar-JO-SanaNeural", "Sana - Arabic (Jordan)"),
            new TextToSpeechVoice("kn-IN-SapnaNeural", "Sapna - Kannada (India)"),
            new TextToSpeechVoice("en-US-SaraNeural", "Sara - English (United States)"),
            new TextToSpeechVoice("ta-LK-SaranyaNeural", "Saranya - Tamil (Sri Lanka)"),
            new TextToSpeechVoice("uz-UZ-SardorNeural", "Sardor - Uzbek (Latin, Uzbekistan)"),
            new TextToSpeechVoice("es-ES-SaulNeural", "Saul - Spanish (Spain)"),
            new TextToSpeechVoice("es-VE-SebastianNeural", "Sebastian - Spanish (Venezuela)"),
            new TextToSpeechVoice("fi-FI-SelmaNeural", "Selma - Finnish (Finland)"),
            new TextToSpeechVoice("ko-KR-SeoHyeonNeural", "SeoHyeon - Korean (Korea)"),
            new TextToSpeechVoice("de-DE-SeraphinaMultilingualNeural", "Seraphina Multilingual - German (Germany)"),
            new TextToSpeechVoice("ar-EG-ShakirNeural", "Shakir - Arabic (Egypt)"),
            new TextToSpeechVoice("ja-JP-ShioriNeural", "Shiori - Japanese (Japan)"),
            new TextToSpeechVoice("te-IN-ShrutiNeural", "Shruti - Telugu (India)"),
            new TextToSpeechVoice("iu-Latn-CA-SiqiniqNeural", "Siqiniq - Inuktitut (Latin, Canada)"),
            new TextToSpeechVoice("iu-Cans-CA-SiqiniqNeural", "Siqiniq - Inuktitut (Syllabics, Canada)"),
            new TextToSpeechVoice("jv-ID-SitiNeural", "Siti - Javanese (Latin, Indonesia)"),
            new TextToSpeechVoice("ml-IN-SobhanaNeural", "Sobhana - Malayalam (India)"),
            new TextToSpeechVoice("es-BO-SofiaNeural", "Sofia - Spanish (Bolivia)"),
            new TextToSpeechVoice("sv-SE-SofieNeural", "Sofie - Swedish (Sweden)"),
            new TextToSpeechVoice("en-GB-SoniaNeural", "Sonia - English (United Kingdom)"),
            new TextToSpeechVoice("ko-KR-SoonBokNeural", "SoonBok - Korean (Korea)"),
            new TextToSpeechVoice("sr-RS-SophieNeural", "Sophie - Serbian (Cyrillic, Serbia)"),
            new TextToSpeechVoice("sr-Latn-RS-SophieNeural", "Sophie - Serbian (Latin, Serbia)"),
            new TextToSpeechVoice("hr-HR-SreckoNeural", "Srecko - Croatian (Croatia)"),
            new TextToSpeechVoice("km-KH-SreymomNeural", "Sreymom - Khmer (Cambodia)"),
            new TextToSpeechVoice("en-US-SteffanNeural", "Steffan - English (United States)"),
            new TextToSpeechVoice("or-IN-SubhasiniNeural", "Subhasini - Oriya (India)"),
            new TextToSpeechVoice("or-IN-SukantNeural", "Sukant - Oriya (India)"),
            new TextToSpeechVoice("ko-KR-SunHiNeural", "Sun-Hi - Korean (Korea)"),
            new TextToSpeechVoice("ta-MY-SuryaNeural", "Surya - Tamil (Malaysia)"),
            new TextToSpeechVoice("ru-RU-SvetlanaNeural", "Svetlana - Russian (Russia)"),
            new TextToSpeechVoice("hi-IN-SwaraNeural", "Swara - Hindi (India)"),
            new TextToSpeechVoice("fr-CA-SylvieNeural", "Sylvie - French (Canada)"),
            new TextToSpeechVoice("ar-JO-TaimNeural", "Taim - Arabic (Jordan)"),
            new TextToSpeechVoice("hu-HU-TamasNeural", "Tamas - Hungarian (Hungary)"),
            new TextToSpeechVoice("es-PY-TaniaNeural", "Tania - Spanish (Paraguay)"),
            new TextToSpeechVoice("bn-IN-TanishaaNeural", "Tanishaa - Bengali (India)"),
            new TextToSpeechVoice("de-DE-TanjaNeural", "Tanja - German (Germany)"),
            new TextToSpeechVoice("iu-Latn-CA-TaqqiqNeural", "Taqqiq - Inuktitut (Latin, Canada)"),
            new TextToSpeechVoice("iu-Cans-CA-TaqqiqNeural", "Taqqiq - Inuktitut (Syllabics, Canada)"),
            new TextToSpeechVoice("es-ES-TeoNeural", "Teo - Spanish (Spain)"),
            new TextToSpeechVoice("es-GQ-TeresaNeural", "Teresa - Spanish (Equatorial Guinea)"),
            new TextToSpeechVoice("pt-BR-ThalitaNeural", "Thalita - Portuguese (Brazil)"),
            new TextToSpeechVoice("pt-BR-ThalitaMultilingualNeural", "Thalita Multilingual - Portuguese (Brazil)"),
            new TextToSpeechVoice("zu-ZA-ThandoNeural", "Thando - Zulu (South Africa)"),
            new TextToSpeechVoice("zu-ZA-ThembaNeural", "Themba - Zulu (South Africa)),"),
            new TextToSpeechVoice("fr-CA-ThierryNeural", "Thierry - French (Canada)"),
            new TextToSpeechVoice("my-MM-ThihaNeural", "Thiha - Burmese (Myanmar)"),
            new TextToSpeechVoice("si-LK-ThiliniNeural", "Thilini - Sinhala (Sri Lanka)"),
            new TextToSpeechVoice("en-GB-ThomasNeural", "Thomas - English (United Kingdom)"),
            new TextToSpeechVoice("en-AU-TimNeural", "Tim - English (Australia)"),
            new TextToSpeechVoice("en-AU-TinaNeural", "Tina - English (Australia)"),
            new TextToSpeechVoice("es-AR-TomasNeural", "Tomas - Spanish (Argentina)"),
            new TextToSpeechVoice("en-US-TonyNeural", "Tony - English (United States)"),
            new TextToSpeechVoice("es-ES-TrianaNeural", "Triana - Spanish (Spain)"),
            new TextToSpeechVoice("su-ID-TutiNeural", "Tuti - Sundanese (Indonesia)"),
            new TextToSpeechVoice("so-SO-UbaxNeural", "Ubax - Somali (Somalia)"),
            new TextToSpeechVoice("ur-PK-UzmaNeural", "Uzma - Urdu (Pakistan)"),
            new TextToSpeechVoice("pa-IN-VaaniNeural", "Vaani - Punjabi (India)"),
            new TextToSpeechVoice("es-UY-ValentinaNeural", "Valentina - Spanish (Uruguay)"),
            new TextToSpeechVoice("pt-BR-ValerioNeural", "Valerio - Portuguese (Brazil)"),
            new TextToSpeechVoice("ta-IN-ValluvarNeural", "Valluvar - Tamil (India)"),
            new TextToSpeechVoice("ta-SG-VenbaNeural", "Venba - Tamil (Singapore)"),
            new TextToSpeechVoice("es-ES-VeraNeural", "Vera - Spanish (Spain)"),
            new TextToSpeechVoice("bs-BA-VesnaNeural", "Vesna - Bosnian (Bosnia and Herzegovina)"),
            new TextToSpeechVoice("es-PR-VictorNeural", "Victor - Spanish (Puerto Rico)"),
            new TextToSpeechVoice("sk-SK-ViktoriaNeural", "Viktoria - Slovak (Slovakia)"),
            new TextToSpeechVoice("fr-FR-VivienneMultilingualNeural", "Vivienne Multilingual - French (France)"),
            new TextToSpeechVoice("cs-CZ-VlastaNeural", "Vlasta - Czech (Czechia)"),
            new TextToSpeechVoice("zh-HK-WanLungNeural", "WanLung - Chinese (Cantonese, Traditional)"),
            new TextToSpeechVoice("en-SG-WayneNeural", "Wayne - English (Singapore)"),
            new TextToSpeechVoice("af-ZA-WillemNeural", "Willem - Afrikaans (South Africa)"),
            new TextToSpeechVoice("en-AU-WilliamNeural", "William - English (Australia)"),
            new TextToSpeechVoice("zh-CN-liaoning-XiaobeiNeural", "Xiaobei - Chinese (Northeastern Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaochenNeural", "Xiaochen - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaochenMultilingualNeural", "Xiaochen Multilingual - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaohanNeural", "Xiaohan - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaomengNeural", "Xiaomeng - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("yue-CN-XiaoMinNeural", "XiaoMin - Chinese (Cantonese, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaomoNeural", "Xiaomo - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-shaanxi-XiaoniNeural", "Xiaoni - Chinese (Zhongyuan Mandarin Shaanxi, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoqiuNeural", "Xiaoqiu - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaorouNeural", "Xiaorou - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoruiNeural", "Xiaorui - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoshuangNeural", "Xiaoshuang - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("wuu-CN-XiaotongNeural", "Xiaotong - Chinese (Wu, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoxiaoNeural", "Xiaoxiao - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoxiaoDialectsNeural", "Xiaoxiao Dialects - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoxiaoMultilingualNeural", "Xiaoxiao Multilingual - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoyanNeural", "Xiaoyan - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoyiNeural", "Xiaoyi - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoyouNeural", "Xiaoyou - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaoyuMultilingualNeural", "Xiaoyu Multilingual - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-XiaozhenNeural", "Xiaozhen - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("es-ES-XimenaNeural", "Ximena - Spanish (Spain)"),
            new TextToSpeechVoice("es-MX-YagoNeural", "Yago - Spanish (Mexico)"),
            new TextToSpeechVoice("en-HK-YanNeural", "Yan - English (Hong Kong SAR)"),
            new TextToSpeechVoice("pt-BR-YaraNeural", "Yara - Portuguese (Brazil)"),
            new TextToSpeechVoice("as-IN-YashicaNeural", "Yashica - Assamese (India)"),
            new TextToSpeechVoice("ms-MY-YasminNeural", "Yasmin - Malay (Malaysia)"),
            new TextToSpeechVoice("mn-MN-YesuiNeural", "Yesui - Mongolian (Mongolia)"),
            new TextToSpeechVoice("es-NI-YolandaNeural", "Yolanda - Spanish (Nicaragua)"),
            new TextToSpeechVoice("ko-KR-YuJinNeural", "YuJin - Korean (Korea)"),
            new TextToSpeechVoice("zh-CN-liaoning-YunbiaoNeural", "Yunbiao - Chinese (Northeastern Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-henan-YundengNeural", "Yundeng - Chinese (Zhongyuan Mandarin Henan, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunfengNeural", "Yunfeng - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunhaoNeural", "Yunhao - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-TW-YunJheNeural", "YunJhe - Chinese (Taiwanese Mandarin, Traditional)"),
            new TextToSpeechVoice("zh-CN-YunjianNeural", "Yunjian - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunjieNeural", "Yunjie - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-guangxi-YunqiNeural", "Yunqi - Chinese (Guangxi Accent Mandarin, Simplified)"),
            new TextToSpeechVoice("yue-CN-YunSongNeural", "YunSong - Chinese (Cantonese, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunxiNeural", "Yunxi - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-sichuan-YunxiNeural", "Yunxi - Chinese (Southwestern Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunxiaNeural", "Yunxia - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-shandong-YunxiangNeural", "Yunxiang - Chinese (Jilu Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunyangNeural", "Yunyang - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunyeNeural", "Yunye - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunyiMultilingualNeural", "Yunyi Multilingual - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("zh-CN-YunzeNeural", "Yunze - Chinese (Mandarin, Simplified)"),
            new TextToSpeechVoice("wuu-CN-YunzheNeural", "Yunzhe - Chinese (Wu, Simplified)"),
            new TextToSpeechVoice("fr-FR-YvesNeural", "Yves - French (France)"),
            new TextToSpeechVoice("fr-FR-YvetteNeural", "Yvette - French (France)"),
            new TextToSpeechVoice("ar-SA-ZariyahNeural", "Zariyah - Arabic (Saudi Arabia)"),
            new TextToSpeechVoice("pl-PL-ZofiaNeural", "Zofia - Polish (Poland)"),
            new TextToSpeechVoice("sw-KE-ZuriNeural", "Zuri - Swahili (Kenya)"),
        };

        public TextToSpeechProviderType ProviderType { get { return TextToSpeechProviderType.MicrosoftAzureSpeech; } }

        public int VolumeMinimum { get { return 0; } }

        public int VolumeMaximum { get { return 100; } }

        public int VolumeDefault { get { return 100; } }

        public int PitchMinimum { get { return 0; } }

        public int PitchMaximum { get { return 0; } }

        public int PitchDefault { get { return 0; } }

        public int RateMinimum { get { return 0; } }

        public int RateMaximum { get { return 0; } }

        public int RateDefault { get { return 0; } }

        private DateTimeOffset lastCommand = DateTimeOffset.MinValue;

        public bool IsUsingCustomSubscriptionKey
        {
            get
            {
                return !string.IsNullOrEmpty(ChannelSession.Settings.MicrosoftAzureSpeechCustomRegionName) &&
                    !string.IsNullOrEmpty(ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey);
            }
        }

        public async Task<Result> TestAccess()
        {
            try
            {
                using (SpeechSynthesizer client = this.GetClient())
                {
                    SynthesisVoicesResult result = await client.GetVoicesAsync();
                    if (result != null && result.Voices != null && result.Voices.Count > 0)
                    {
                        return new Result();
                    }
                    return new Result(result.ErrorDetails);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public IEnumerable<TextToSpeechVoice> GetVoices() { return WindowsMicrosoftAzureSpeechService.AvailableVoices; }

        public async Task Speak(string outputDevice, Guid overlayEndpointID, string text, string voice, int volume, int pitch, int rate, bool ssml, bool waitForFinish)
        {
            if (await this.IsWithinRateLimiting())
            {
                using (SpeechSynthesizer speechSynthesizer = this.GetClient(voice))
                {
                    SpeechSynthesisResult speechSynthesisResult = ssml ? await speechSynthesizer.SpeakSsmlAsync(text) : await speechSynthesizer.SpeakTextAsync(text);
                    if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        MemoryStream stream = new MemoryStream(speechSynthesisResult.AudioData);
                        await ServiceManager.Get<IAudioService>().PlayMP3Stream(stream, volume, outputDevice, waitForFinish: waitForFinish);
                    }
                    else if (speechSynthesisResult.Reason == ResultReason.Canceled)
                    {
                        SpeechSynthesisCancellationDetails cancellationDetails = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                        Logger.Log(LogLevel.Error, $"Azure Speech Synthesis canceled: {cancellationDetails.ErrorCode} - {cancellationDetails.ErrorDetails}");
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, $"Azure Speech Synthesis error: {speechSynthesisResult.Reason}");
                    }
                }
            }
        }

        private async Task<bool> IsWithinRateLimiting()
        {
            if (this.IsUsingCustomSubscriptionKey || ChannelSession.IsDebug() || this.lastCommand.TotalMinutesFromNow() >= 5)
            {
                this.lastCommand = DateTimeOffset.Now;
                return true;
            }
            await ServiceManager.Get<ChatService>().SendMessage(string.Format(Resources.TextToSpeechActionBlockedDueToRateLimiting, Resources.MicrosoftAzureSpeech), StreamingPlatformTypeEnum.All);
            return false;
        }

        private SpeechSynthesizer GetClient(string voice = null)
        {
            SpeechConfig speechConfig = SpeechConfig.FromSubscription(ServiceManager.Get<SecretsService>().GetSecret("AzureSpeechServiceSecret"), "eastus");
            if (this.IsUsingCustomSubscriptionKey)
            {
                speechConfig = SpeechConfig.FromSubscription(ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey, ChannelSession.Settings.MicrosoftAzureSpeechCustomRegionName);
            }

            if (voice != null)
            {
                speechConfig.SpeechSynthesisVoiceName = voice;
            }

            return new SpeechSynthesizer(speechConfig, null);
        }

        private async Task GenerateVoicesList()
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient())
            {
                client.AddHeader("Ocp-Apim-Subscription-Key", ServiceManager.Get<SecretsService>().GetSecret("AzureSpeechServiceSecret"));
                string content = await client.GetStringAsync("https://eastus.tts.speech.microsoft.com/cognitiveservices/voices/list");
                await ServiceManager.Get<IFileService>().SaveFile("S:\\voices.txt", content);
            }
        }
    }
}
